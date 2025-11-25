using Godot;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public partial class FriendsService : Node
{
	private static FriendsService _instance;
	public static FriendsService Instance => _instance;

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
			GD.PrintErr("FIREBASE_PROJECT_ID not found in .env file");
		}
	}

	public async Task<UserData> SearchUserByUsernameAsync(string username)
	{
		string idToken = await FirebaseAuthManager.Instance.GetIdTokenAsync();
		if (string.IsNullOrEmpty(idToken))
			return null;

		string url = $"{_firestoreUrl}:runQuery";
		
		var query = new
		{
			structuredQuery = new
			{
				from = new[] { new { collectionId = "users" } },
				where = new
				{
					fieldFilter = new
					{
						field = new { fieldPath = "username" },
						op = "EQUAL",
						value = new { stringValue = username }
					}
				},
				limit = 1
			}
		};

		try
		{
			var response = await MakeFirestoreRequestAsync(url, query, idToken, "POST");
			
			if (response.Success && response.Data.ValueKind == JsonValueKind.Array)
			{
				var results = response.Data.EnumerateArray().ToList();
				if (results.Count > 0 && results[0].TryGetProperty("document", out var doc))
				{
					return ConvertFromFirestoreDocument(doc);
				}
			}
			
			return null;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Failed to search user: {ex.Message}");
			return null;
		}
	}

	public async Task<bool> SendFriendRequestAsync(string toUserId, string toUsername)
	{
		string idToken = await FirebaseAuthManager.Instance.GetIdTokenAsync();
		var currentUserId = FirebaseAuthManager.Instance.UserId;
		
		if (string.IsNullOrEmpty(idToken) || string.IsNullOrEmpty(currentUserId))
			return false;

		var currentUserData = await FirestoreService.Instance.GetUserAsync(currentUserId);
		if (currentUserData == null)
			return false;

		var friendRequest = FriendRequest.Create(
			currentUserId,
			currentUserData.Username,
			toUserId,
			toUsername
		);

		string requestId = $"{currentUserId}_{toUserId}";
		string url = $"{_firestoreUrl}/friendRequests/{requestId}";
		
		var firestoreDoc = ConvertFriendRequestToFirestore(friendRequest);

		try
		{
			var response = await MakeFirestoreRequestAsync(url, firestoreDoc, idToken, "PATCH");
			return response.Success;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Failed to send friend request: {ex.Message}");
			return false;
		}
	}

	public async Task<List<string>> GetFriendUsernamesAsync(List<string> friendIds)
	{
		var usernames = new List<string>();
		
		foreach (var friendId in friendIds)
		{
			var friendData = await FirestoreService.Instance.GetUserAsync(friendId);
			if (friendData != null)
			{
				usernames.Add(friendData.Username);
			}
		}
		
		return usernames;
	}

	private object ConvertFriendRequestToFirestore(FriendRequest request)
	{
		return new
		{
			fields = new
			{
				fromUserId = new { stringValue = request.FromUserId },
				fromUsername = new { stringValue = request.FromUsername },
				toUserId = new { stringValue = request.ToUserId },
				toUsername = new { stringValue = request.ToUsername },
				status = new { stringValue = request.Status },
				timestamp = new { integerValue = request.Timestamp.ToString() }
			}
		};
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
			GD.PrintErr($"Failed to convert user document: {ex.Message}");
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