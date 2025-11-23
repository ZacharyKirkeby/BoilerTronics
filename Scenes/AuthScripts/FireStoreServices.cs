using Godot;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

public partial class FirestoreService : Node
{
	private static FirestoreService _instance;
	public static FirestoreService Instance => _instance;

	private string _projectId;
	private string _firestoreUrl;

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
		_projectId = config.GetValue("FIREBASE_PROJECT_ID");
		_firestoreUrl = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents";

		if (string.IsNullOrEmpty(_projectId))
		{
			GD.PrintErr("FIREBASE_PROJECT_ID not found in .env file!");
		}
		else
		{
			GD.Print("Firestore configuration loaded");
		}
	}

	public async Task<bool> CreateUserAsync(string userId, UserData userData)
	{
		string idToken = await FirebaseAuthManager.Instance.GetIdTokenAsync();
		if (string.IsNullOrEmpty(idToken))
		{
			GD.PrintErr("No authentication token available");
			return false;
		}

		string url = $"{_firestoreUrl}/users/{userId}";
		var firestoreDoc = ConvertToFirestoreDocument(userData);
		
		try
		{
			var response = await MakeFirestoreRequestAsync(url, firestoreDoc, idToken, "PATCH");
			if (response.Success)
			{
				GD.Print($"User document created: {userData.Username}");
			}
			return response.Success;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Failed to create user: {ex.Message}");
			return false;
		}
	}

	public async Task<UserData> GetUserAsync(string userId)
	{
		string idToken = await FirebaseAuthManager.Instance.GetIdTokenAsync();
		if (string.IsNullOrEmpty(idToken))
		{
			GD.PrintErr("No authentication token available");
			return null;
		}

		string url = $"{_firestoreUrl}/users/{userId}";
		
		try
		{
			var response = await MakeFirestoreRequestAsync(url, null, idToken, "GET");
			
			if (response.Success)
			{
				var userData = ConvertFromFirestoreDocument(response.Data);
				GD.Print($"User data loaded: {userData.Username}");
				return userData;
			}
			
			return null;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Failed to get user: {ex.Message}");
			return null;
		}
	}
	private object ConvertToFirestoreDocument(UserData userData)
	{
		return new
		{
			fields = new
			{
				username = new { stringValue = userData.Username },
				uuid = new { stringValue = userData.Uuid },
				friends = new
				{
					arrayValue = new
					{
						values = ConvertListToFirestoreArray(userData.Friends)
					}
				},
				achievementsUnlocked = new
				{
					arrayValue = new
					{
						values = ConvertListToFirestoreArray(userData.AchievementsUnlocked)
					}
				},
				hoursPlayed = new { doubleValue = userData.HoursPlayed },
				easterEggsFound = new
				{
					arrayValue = new
					{
						values = ConvertListToFirestoreArray(userData.EasterEggsFound)
					}
				}
			}
		};
	}

	private object[] ConvertListToFirestoreArray(List<string> list)
	{
		var array = new object[list.Count];
		for (int i = 0; i < list.Count; i++)
		{
			array[i] = new { stringValue = list[i] };
		}
		return array;
	}

	private UserData ConvertFromFirestoreDocument(JsonElement doc)
	{
		try
		{
			var fields = doc.GetProperty("fields");
			
			return new UserData
			{
				Username = GetStringField(fields, "username"),
				Uuid = GetStringField(fields, "uuid"),
				Friends = GetArrayField(fields, "friends"),
				AchievementsUnlocked = GetArrayField(fields, "achievementsUnlocked"),
				HoursPlayed = GetDoubleField(fields, "hoursPlayed"),
				EasterEggsFound = GetArrayField(fields, "easterEggsFound")
			};
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Failed to convert Firestore document: {ex.Message}");
			return null;
		}
	}

	private string GetStringField(JsonElement fields, string fieldName)
	{
		if (fields.TryGetProperty(fieldName, out var field) &&
		    field.TryGetProperty("stringValue", out var value))
		{
			return value.GetString();
		}
		return string.Empty;
	}

	private float GetDoubleField(JsonElement fields, string fieldName)
	{
		if (fields.TryGetProperty(fieldName, out var field))
		{
			if (field.TryGetProperty("doubleValue", out var value))
			{
				return (float)value.GetDouble();
			}
			else if (field.TryGetProperty("integerValue", out var intValue))
			{
				return float.Parse(intValue.GetString());
			}
		}
		return 0f;
	}

	private List<string> GetArrayField(JsonElement fields, string fieldName)
	{
		var list = new List<string>();
		
		if (fields.TryGetProperty(fieldName, out var field) &&
		    field.TryGetProperty("arrayValue", out var arrayValue) &&
		    arrayValue.TryGetProperty("values", out var values))
		{
			foreach (var item in values.EnumerateArray())
			{
				if (item.TryGetProperty("stringValue", out var stringValue))
				{
					list.Add(stringValue.GetString());
				}
			}
		}
		
		return list;
	}

	private async Task<FirestoreResponse> MakeFirestoreRequestAsync(string url, object payload, string idToken, string method = "POST")
	{
		var httpRequest = new HttpRequest();
		AddChild(httpRequest);

		var headers = new List<string>
		{
			"Content-Type: application/json",
			$"Authorization: Bearer {idToken}"
		};

		var taskCompletionSource = new TaskCompletionSource<FirestoreResponse>();

		httpRequest.RequestCompleted += (long result, long responseCode, string[] responseHeaders, byte[] body) =>
		{
			httpRequest.QueueFree();

			if (responseCode >= 200 && responseCode < 300)
			{
				string responseText = Encoding.UTF8.GetString(body);
				var data = string.IsNullOrEmpty(responseText) ? new JsonElement() : JsonSerializer.Deserialize<JsonElement>(responseText);
				taskCompletionSource.SetResult(new FirestoreResponse { Success = true, Data = data });
			}
			else
			{
				string errorText = Encoding.UTF8.GetString(body);
				GD.PrintErr($"Firestore error ({responseCode}): {errorText}");
				taskCompletionSource.SetResult(new FirestoreResponse { Success = false });
			}
		};

		HttpClient.Method httpMethod = method switch
		{
			"GET" => HttpClient.Method.Get,
			"PATCH" => HttpClient.Method.Patch,
			"DELETE" => HttpClient.Method.Delete,
			_ => HttpClient.Method.Post
		};

		if (payload != null)
		{
			string jsonPayload = JsonSerializer.Serialize(payload);
			httpRequest.Request(url, headers.ToArray(), httpMethod, jsonPayload);
		}
		else
		{
			httpRequest.Request(url, headers.ToArray(), httpMethod);
		}

		return await taskCompletionSource.Task;
	}

	private class FirestoreResponse
	{
		public bool Success { get; set; }
		public JsonElement Data { get; set; }
	}
}