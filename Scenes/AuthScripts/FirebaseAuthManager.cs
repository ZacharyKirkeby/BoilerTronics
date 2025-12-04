using Godot;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public partial class FirebaseAuthManager : Node
{
    private static FirebaseAuthManager _instance;
    public static FirebaseAuthManager Instance => _instance;

    private string _firebaseApiKey;
    private const string AUTH_URL = "https://identitytoolkit.googleapis.com/v1/accounts";
    private const string TOKEN_URL = "https://securetoken.googleapis.com/v1/token";

    // Authentication state
    private string _idToken;
    private string _refreshToken;
    private string _userId;
    private string _email;
    private DateTime _tokenExpiry;

    public bool IsAuthenticated => !string.IsNullOrEmpty(_idToken) && DateTime.UtcNow < _tokenExpiry;
    public string UserId => _userId;
    public string Email => _email;

    [Signal]
    public delegate void AuthenticationChangedEventHandler(bool isAuthenticated);

    public override void _EnterTree()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            QueueFree();
        }
    }

    public override void _Ready()
    {
        LoadConfiguration();
    }

    private void LoadConfiguration()
    {
        var config = EnvironmentConfig.Instance;
        _firebaseApiKey = config.GetValue("FIREBASE_API_KEY");

        if (string.IsNullOrEmpty(_firebaseApiKey))
        {
            GD.PrintErr("FIREBASE_API_KEY not found in .env file");
        }
        else
        {
            GD.Print("Firebase Auth configuration loaded");
        }
    }

    public async Task<AuthResult> RegisterAsync(string email, string password)
    {
        var payload = new
        {
            email = email,
            password = password,
            returnSecureToken = true
        };

        var result = await AuthRequestAsync($"{AUTH_URL}:signUp?key={_firebaseApiKey}", payload);

        // successful, create user document in Firestore
        if (result.Success)
        {
            await EnsureUserDocumentExists();
            EmitSignal(SignalName.AuthenticationChanged, true);
        }

        return result;
    }

    public async Task<AuthResult> SignInAsync(string email, string password)
    {
        var payload = new
        {
            email = email,
            password = password,
            returnSecureToken = true
        };

        var result = await AuthRequestAsync($"{AUTH_URL}:signInWithPassword?key={_firebaseApiKey}", payload);

        // ensure user document exists
        if (result.Success)
        {
            await EnsureUserDocumentExists();
            EmitSignal(SignalName.AuthenticationChanged, true);
        }

        return result;
    }

    private async Task EnsureUserDocumentExists()
    {
        if (string.IsNullOrEmpty(_userId) || string.IsNullOrEmpty(_email))
            return;

        var firestoreService = FirestoreService.Instance;
        if (firestoreService == null)
            return;

        // Try to get existing user
        var existingUser = await firestoreService.GetUserAsync(_userId);

        // If user doesn't exist, create default user - ngl i have no idea whayt im doing
        if (existingUser == null)
        {
            GD.Print($"Creating new user document for {_email}");
            var defaultUser = UserData.CreateDefault(_userId, _email);
            await firestoreService.CreateUserAsync(_userId, defaultUser);
        }
    }

    public void SignOut()
    {
        _idToken = null;
        _refreshToken = null;
        _userId = null;
        _email = null;
        _tokenExpiry = DateTime.MinValue;

        EmitSignal(SignalName.AuthenticationChanged, false);
        GD.Print("User signed out");
    }

    public async Task<string> GetIdTokenAsync()
    {
        if (!IsAuthenticated && !string.IsNullOrEmpty(_refreshToken))
        {
            await RefreshTokenAsync();
        }

        return _idToken;
    }

    private async Task<bool> RefreshTokenAsync()
    {
        if (string.IsNullOrEmpty(_refreshToken))
            return false;

        var payload = new
        {
            grant_type = "refresh_token",
            refresh_token = _refreshToken
        };

        try
        {
            var response = await MakeHttpRequestAsync($"{TOKEN_URL}?key={_firebaseApiKey}", payload);

            if (response.Success)
            {
                var data = response.Data;
                _idToken = data.GetProperty("id_token").GetString();
                _refreshToken = data.GetProperty("refresh_token").GetString();
                _userId = data.GetProperty("user_id").GetString();

                int expiresIn = int.Parse(data.GetProperty("expires_in").GetString());
                _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn);

                GD.Print("Token refreshed successfully");
                return true;
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Token refresh failed: {ex.Message}");
        }

        return false;
    }

    private async Task<AuthResult> AuthRequestAsync(string url, object payload)
    {
        try
        {
            var response = await MakeHttpRequestAsync(url, payload);

            if (response.Success)
            {
                var data = response.Data;
                _idToken = data.GetProperty("idToken").GetString();
                _refreshToken = data.GetProperty("refreshToken").GetString();
                _userId = data.GetProperty("localId").GetString();
                _email = data.GetProperty("email").GetString();

                int expiresIn = int.Parse(data.GetProperty("expiresIn").GetString());
                _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn);

                GD.Print($"Authentication successful: {_email}");

                return new AuthResult { Success = true };
            }
            else
            {
                string errorMsg = ParseFirebaseError(response.ErrorMessage);
                GD.PrintErr($"Authentication failed: {errorMsg}");
                return new AuthResult { Success = false, ErrorMessage = errorMsg };
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Authentication request failed: {ex.Message}");
            return new AuthResult { Success = false, ErrorMessage = "Network error. Please check your connection." };
        }
    }

    private async Task<HttpResponse> MakeHttpRequestAsync(string url, object payload)
    {
        var httpRequest = new HttpRequest();
        AddChild(httpRequest);

        string jsonPayload = JsonSerializer.Serialize(payload);
        string[] headers = new[] { "Content-Type: application/json" };

        var taskCompletionSource = new TaskCompletionSource<HttpResponse>();

        httpRequest.RequestCompleted += (long result, long responseCode, string[] responseHeaders, byte[] body) =>
        {
            httpRequest.QueueFree();

            if (responseCode >= 200 && responseCode < 300)
            {
                string responseText = Encoding.UTF8.GetString(body);
                var data = JsonSerializer.Deserialize<JsonElement>(responseText);
                taskCompletionSource.SetResult(new HttpResponse { Success = true, Data = data });
            }
            else
            {
                string errorText = Encoding.UTF8.GetString(body);
                taskCompletionSource.SetResult(new HttpResponse { Success = false, ErrorMessage = errorText });
            }
        };

        httpRequest.Request(url, headers, HttpClient.Method.Post, jsonPayload);

        return await taskCompletionSource.Task;
    }

    private string ParseFirebaseError(string errorJson)
    {
        try
        {
            var error = JsonSerializer.Deserialize<JsonElement>(errorJson);
            string errorMessage = error.GetProperty("error").GetProperty("message").GetString();

            GD.PrintErr(errorMessage);
            return errorMessage switch
            {
                "EMAIL_EXISTS" => "An account with this email already exists.",
                "INVALID_EMAIL" => "The email address is invalid.",
                "WEAK_PASSWORD" => "Password should be at least 6 characters.",
                "EMAIL_NOT_FOUND" => "No account found with this email.",
                "INVALID_PASSWORD" => "Incorrect password.",
                "USER_DISABLED" => "This account has been disabled.",
                "INVALID_LOGIN_CREDENTIALS" => "Invalid email or password.",
                _ => "Authentication failed. Please try again."
            };
        }
        catch
        {
            return "An error occurred. Please try again.";
        }
    }

    public string GetCurrentUserId()
    {
        return _userId;
    }

    private class HttpResponse
    {
        public bool Success { get; set; }
        public JsonElement Data { get; set; }
        public string ErrorMessage { get; set; }
    }
}

public class AuthResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
}