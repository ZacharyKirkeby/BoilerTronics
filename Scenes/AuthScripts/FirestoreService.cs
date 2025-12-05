using Godot;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

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

		string url = $"{_firestoreUrl}/users?documentId={userId}";
		var firestoreDoc = ConvertToFirestoreDocument(userData);

		try
		{
			// Use POST for creating new documents with a specific ID
			var response = await MakeFirestoreRequestAsync(url, firestoreDoc, idToken, "POST");
			if (response.Success)
			{
				GD.Print($"User document created: {userData.Username}");
			}
			else if (response.ResponseCode == 409)
			{
				// Document already exists
				GD.Print("User already exists, updating instead");
				string updateUrl = $"{_firestoreUrl}/users/{userId}";
				response = await MakeFirestoreRequestAsync(updateUrl, firestoreDoc, idToken, "PATCH");
			}
			return response.Success;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Failed to create user: {ex.Message}");
			return false;
		}
	}

	private async Task<bool> CreateOrUpdateDocumentAsync(string collectionPath, string documentId, object firestoreDoc, string idToken)
	{
		// Try creating first
		string createUrl = $"{_firestoreUrl}/{collectionPath}?documentId={documentId}";
		var response = await MakeFirestoreRequestAsync(createUrl, firestoreDoc, idToken, "POST");

		if (response.Success)
		{
			return true;
		}
		else if (response.ResponseCode == 409)
		{
			// Document exists, update instead
			string updateUrl = $"{_firestoreUrl}/{collectionPath}/{documentId}";
			response = await MakeFirestoreRequestAsync(updateUrl, firestoreDoc, idToken, "PATCH");
			return response.Success;
		}

		return false;
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

			// 404 is expected when document doesn't exist - not an error
			if (response.ResponseCode == 404)
			{
				GD.Print($"User document not found for {userId} (this is normal for new users)");
				return null;
			}

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

	public async Task<bool> UpdateUserFieldAsync(string userId, string fieldName, object value)
	{
		string idToken = await FirebaseAuthManager.Instance.GetIdTokenAsync();
		if (string.IsNullOrEmpty(idToken))
		{
			GD.PrintErr("No authentication token available");
			return false;
		}

		string url = $"{_firestoreUrl}/users/{userId}?updateMask.fieldPaths={fieldName}";

		var firestoreDoc = new
		{
			fields = new Dictionary<string, object>
			{
				{ fieldName, ConvertToFirestoreValue(value) }
			}
		};

		try
		{
			var response = await MakeFirestoreRequestAsync(url, firestoreDoc, idToken, "PATCH");
			if (response.Success)
			{
				GD.Print($"Updated field: {fieldName}");
			}
			return response.Success;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Failed to update field {fieldName}: {ex.Message}");
			return false;
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

	private object ConvertToFirestoreValue(object value)
	{
		return value switch
		{
			string s => new { stringValue = s },
			int i => new { integerValue = i.ToString() },
			long l => new { integerValue = l.ToString() },
			float f => new { doubleValue = f },
			double d => new { doubleValue = d },
			bool b => new { booleanValue = b },
			List<string> list => new
			{
				arrayValue = new
				{
					values = ConvertListToFirestoreArray(list)
				}
			},
			_ => new { stringValue = value.ToString() }
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

		var tcs = new TaskCompletionSource<FirestoreResponse>();

		httpRequest.RequestCompleted += (long result, long responseCode, string[] responseHeaders, byte[] body) =>
		{
			httpRequest.QueueFree();
			string text = Encoding.UTF8.GetString(body);

			if (responseCode >= 200 && responseCode < 300)
			{
				var data = string.IsNullOrEmpty(text)
					? new JsonElement()
					: JsonSerializer.Deserialize<JsonElement>(text);

				tcs.SetResult(new FirestoreResponse { Success = true, Data = data, ResponseCode = (int)responseCode });
			}
			else
			{
				if (responseCode == 404)
				{
					GD.Print($"Document not found (404): {url}");
				}
				else
				{
					GD.PrintErr($"Firestore error ({responseCode}): {text}");
				}
				tcs.SetResult(new FirestoreResponse { Success = false, ResponseCode = (int)responseCode });
			}
		};

		HttpClient.Method httpMethod = method switch
		{
			"GET" => HttpClient.Method.Get,
			"PUT" => HttpClient.Method.Put,
			"PATCH" => HttpClient.Method.Patch,
			"DELETE" => HttpClient.Method.Delete,
			_ => HttpClient.Method.Post
		};

		if (payload != null)
		{
			string json = JsonSerializer.Serialize(payload);
			httpRequest.Request(url, headers.ToArray(), httpMethod, json);
		}
		else
		{
			httpRequest.Request(url, headers.ToArray(), httpMethod);
		}

		return await tcs.Task;
	}


	// call this to get a set of friends data
	public async Task<List<ScoreData>> GetFriendScoresAsync(string levelId, int limit = 10)
	{
		string idToken = await FirebaseAuthManager.Instance.GetIdTokenAsync();
		if (string.IsNullOrEmpty(idToken))
		{
			GD.PrintErr("No authentication token available");
			return new List<ScoreData>();
		}

		// get current user's friends list
		string userId = FirebaseAuthManager.Instance.GetCurrentUserId();
		var userData = await GetUserAsync(userId);
		if (userData == null || userData.Friends.Count == 0)
		{
			GD.Print("No friends found");
			return new List<ScoreData>();
		}

		var friendScores = new List<ScoreData>();
		int fetched = 0;

		// get scores
		foreach (string friendId in userData.Friends)
		{
			if (fetched >= limit) break;

			var score = await GetUserBestScoreAsync(friendId, levelId);
			if (score != null)
			{
				friendScores.Add(score);
				fetched++;
			}
		}

		// Sort by score descending - idk how this will work
		friendScores.Sort((a, b) => b.Score.CompareTo(a.Score));

		GD.Print($"Retrieved {friendScores.Count} friend scores for level {levelId}");
		return friendScores;
	}

	// For leaving a sevel or maybe only on completion? idk 
	public async Task<bool> SaveScoreAsync(string levelId, ScoreData scoreData)
	{
		string idToken = await FirebaseAuthManager.Instance.GetIdTokenAsync();
		if (string.IsNullOrEmpty(idToken))
		{
			GD.PrintErr("No authentication token available");
			return false;
		}

		string userId = FirebaseAuthManager.Instance.UserId;
		scoreData.UserId = userId;
		scoreData.LevelId = levelId;

		// Generate unique score ID- may be overkill 
		string scoreId = $"{userId}_{Guid.NewGuid()}";
		string url = $"{_firestoreUrl}/scores/{levelId}/entries/{scoreId}";

		var firestoreDoc = ConvertScoreToFirestoreDocument(scoreData);

		try
		{
			var response = await MakeFirestoreRequestAsync(url, firestoreDoc, idToken, "PATCH");
			if (response.Success)
			{
				GD.Print($"Score saved: {scoreData.Score} for level {levelId}");

				// Also update user's best score if this is better
				await UpdateBestScoreIfNeeded(levelId, scoreData, idToken);
			}
			return response.Success;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Failed to save score: {ex.Message}");
			return false;
		}
	}

	// helper function
	private async Task UpdateBestScoreIfNeeded(string levelId, ScoreData newScore, string idToken)
	{
		string userId = newScore.UserId;
		var currentBest = await GetUserBestScoreAsync(userId, levelId);

		if (currentBest == null || newScore.Score > currentBest.Score)
		{
			string url = $"{_firestoreUrl}/leaderboards/{levelId}/rankings/{userId}";
			var firestoreDoc = ConvertScoreToFirestoreDocument(newScore);

			await MakeFirestoreRequestAsync(url, firestoreDoc, idToken, "PATCH");
			GD.Print($"Updated best score for user {userId} on level {levelId}");
		}
	}

	// im so goated at naming shit
	public async Task<ScoreData> GetUserBestScoreAsync(string userId, string levelId)
	{
		string idToken = await FirebaseAuthManager.Instance.GetIdTokenAsync();
		if (string.IsNullOrEmpty(idToken))
		{
			GD.PrintErr("No authentication token available");
			return null;
		}

		string url = $"{_firestoreUrl}/leaderboards/{levelId}/rankings/{userId}";

		try
		{
			var response = await MakeFirestoreRequestAsync(url, null, idToken, "GET");

			if (response.ResponseCode == 404)
			{
				return null; // User hasn't played this level yet
			}

			if (response.Success)
			{
				return ConvertFromFirestoreScoreDocument(response.Data);
			}

			return null;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Failed to get user best score: {ex.Message}");
			return null;
		}
	}


	string CleanForDocId(string input)
	{
		// Firestore must not contain /, ?, #, or spaces
		var invalid = new[] { '/', '?', '#', ' ', ',' };
		foreach (var c in invalid)
			input = input.Replace(c.ToString(), "");
		return input;
	}

	private string BuildUpdateMaskQuery()
	{
		string[] fields =
		{
		"levelId",
		"creatorId",
		"creatorName",
		"levelName",
		"description",
		"levelDataJson",
		"difficulty",
		"tags"
	};

		return "?" + string.Join("&", fields.Select(f => $"updateMask.fieldPaths={f}"));
	}



	// once again my naming is a banger
	public async Task<string> SaveLevelAsync(LevelData levelData)
	{
		string idToken = await FirebaseAuthManager.Instance.GetIdTokenAsync();
		if (string.IsNullOrEmpty(idToken))
			return null;

		string userId = FirebaseAuthManager.Instance.UserId;
		levelData.CreatorId = userId;

		string levelId = $"{userId}_{Guid.NewGuid()}";
		levelData.LevelId = levelId;

		var firestoreDoc = ConvertLevelToFirestoreDocument(levelData);

		string safeId = CleanForDocId(levelId);

		// Use POST
		string url = $"{_firestoreUrl}/levels?documentId={safeId}";

		try
		{
			var response = await MakeFirestoreRequestAsync(url, firestoreDoc, idToken, "POST");

			if (response.Success)
			{
				GD.Print($"Level saved successfully: {levelId}");
				return levelId;
			}
			else if (response.ResponseCode == 409)
			{
				// Document already exists
				GD.Print("Level exists, updating...");
				string updateUrl = $"{_firestoreUrl}/levels/{safeId}{BuildUpdateMaskQuery()}";
				response = await MakeFirestoreRequestAsync(updateUrl, firestoreDoc, idToken, "PATCH");
				return response.Success ? levelId : null;
			}

			return null;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Failed to save level: {ex.Message}");
			return null;
		}
	}


	// retrive server stored level
	public async Task<LevelData> GetLevelAsync(string levelId)
	{
		string idToken = await FirebaseAuthManager.Instance.GetIdTokenAsync();
		if (string.IsNullOrEmpty(idToken))
		{
			GD.PrintErr("No authentication token available");
			return null;
		}

		string url = $"{_firestoreUrl}/levels/{levelId}?currentDocument.exists=true";

		try
		{
			var response = await MakeFirestoreRequestAsync(url, null, idToken, "GET");

			if (response.ResponseCode == 404)
			{
				GD.Print($"Level {levelId} not found");
				return null;
			}

			if (response.Success)
			{
				return ConvertFromFirestoreLevelDocument(response.Data);
			}

			return null;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Failed to get level: {ex.Message}");
			return null;
		}
	}

	public async Task<List<LevelData>> GetUserLevelsAsync(string userId, int limit = 50)
	{
		string idToken = await FirebaseAuthManager.Instance.GetIdTokenAsync();
		if (string.IsNullOrEmpty(idToken))
		{
			GD.PrintErr("No authentication token available");
			return new List<LevelData>();
		}
		// this may not work idfk
		string url = $"{_firestoreUrl}/levels?pageSize={limit}";

		try
		{
			var response = await MakeFirestoreRequestAsync(url, null, idToken, "GET");

			if (response.Success)
			{
				var levels = new List<LevelData>();

				if (response.Data.TryGetProperty("documents", out var documents))
				{
					foreach (var doc in documents.EnumerateArray())
					{
						var level = ConvertFromFirestoreLevelDocument(doc);
						if (level != null && level.CreatorId == userId)
						{
							levels.Add(level);
						}
					}
				}

				GD.Print($"Retrieved {levels.Count} levels for user {userId}");
				return levels;
			}

			return new List<LevelData>();
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Failed to get user levels: {ex.Message}");
			return new List<LevelData>();
		}
	}

	// idk if we need this but fuck it we ball
	public async Task<bool> DeleteLevelAsync(string levelId, string userId)
	{
		string idToken = await FirebaseAuthManager.Instance.GetIdTokenAsync();
		if (string.IsNullOrEmpty(idToken))
		{
			GD.PrintErr("No authentication token available");
			return false;
		}

		// Verify you own this level
		var level = await GetLevelAsync(levelId);
		if (level == null || level.CreatorId != userId)
		{
			GD.PrintErr("User does not own this level or level not found");
			return false;
		}

		string url = $"{_firestoreUrl}/levels/{levelId}";

		try
		{
			var response = await MakeFirestoreRequestAsync(url, null, idToken, "DELETE");
			if (response.Success)
			{
				GD.Print($"Level deleted: {levelId}");
			}
			return response.Success;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Failed to delete level: {ex.Message}");
			return false;
		}
	}

	public async Task<LeaderboardData> GetLeaderboardAroundUserAsync(string levelId, string userId, int range = 5)
	{
		string idToken = await FirebaseAuthManager.Instance.GetIdTokenAsync();
		if (string.IsNullOrEmpty(idToken))
		{
			GD.PrintErr("No authentication token available");
			return null;
		}
		var allScores = await GetGlobalLeaderboardAsync(levelId, 1000);

		var leaderboardData = new LeaderboardData
		{
			AboveUser = new List<ScoreData>(),
			UserScore = null,
			BelowUser = new List<ScoreData>(),
			UserRank = -1,
			TotalEntries = allScores.Count
		};

		int userIndex = allScores.FindIndex(s => s.UserId == userId);

		if (userIndex == -1)
		{
			GD.Print($"User {userId} not found on leaderboard for level {levelId}");
			return leaderboardData;
		}

		leaderboardData.UserScore = allScores[userIndex];
		leaderboardData.UserRank = userIndex + 1;

		// Get scores above user
		int startAbove = Math.Max(0, userIndex - range);
		for (int i = startAbove; i < userIndex; i++)
		{
			leaderboardData.AboveUser.Add(allScores[i]);
		}

		// Get scores below user
		int endBelow = Math.Min(allScores.Count, userIndex + range + 1);
		for (int i = userIndex + 1; i < endBelow; i++)
		{
			leaderboardData.BelowUser.Add(allScores[i]);
		}

		GD.Print($"User rank: {leaderboardData.UserRank} of {leaderboardData.TotalEntries}");
		return leaderboardData;
	}

	// i also have no clue if this will work yet
	public async Task<int> GetUserRankAsync(string levelId, string userId)
	{
		var allScores = await GetGlobalLeaderboardAsync(levelId, 1000);
		int userIndex = allScores.FindIndex(s => s.UserId == userId);
		return userIndex == -1 ? -1 : userIndex + 1;
	}

	public async Task<List<ScoreData>> GetGlobalLeaderboardAsync(string levelId, int limit = 10)
	{
		string idToken = await FirebaseAuthManager.Instance.GetIdTokenAsync();
		if (string.IsNullOrEmpty(idToken))
		{
			GD.PrintErr("No authentication token available");
			return new List<ScoreData>();
		}

		// Query the leaderboard collection for this level
		string url = $"{_firestoreUrl}/leaderboards/{levelId}/rankings?pageSize={limit}";

		try
		{
			var response = await MakeFirestoreRequestAsync(url, null, idToken, "GET");

			if (response.Success)
			{
				var scores = new List<ScoreData>();

				if (response.Data.TryGetProperty("documents", out var documents))
				{
					foreach (var doc in documents.EnumerateArray())
					{
						var score = ConvertFromFirestoreScoreDocument(doc);
						if (score != null)
						{
							scores.Add(score);
						}
					}
				}

				// Sort by score descending - we shall see
				scores.Sort((a, b) => b.Score.CompareTo(a.Score));

				GD.Print($"Retrieved {scores.Count} scores for global leaderboard");
				return scores.Take(limit).ToList();
			}

			return new List<ScoreData>();
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Failed to get global leaderboard: {ex.Message}");
			return new List<ScoreData>();
		}
	}

	private object ConvertScoreToFirestoreDocument(ScoreData scoreData)
	{
		var fields = new Dictionary<string, object>
	{
		{ "userId", new { stringValue = scoreData.UserId } },
		{ "username", new { stringValue = scoreData.Username } },
		{ "levelId", new { stringValue = scoreData.LevelId } },
		{ "score", new { integerValue = scoreData.Score.ToString() } }
	};

		if (scoreData.Metadata != null && scoreData.Metadata.Count > 0)
		{
			fields["metadata"] = new { stringValue = JsonSerializer.Serialize(scoreData.Metadata) };
		}

		return new { fields };
	}

	private ScoreData ConvertFromFirestoreScoreDocument(JsonElement doc)
	{
		try
		{
			var fields = doc.GetProperty("fields");

			var scoreData = new ScoreData
			{
				UserId = GetStringField(fields, "userId"),
				Username = GetStringField(fields, "username"),
				LevelId = GetStringField(fields, "levelId"),
				Score = GetIntField(fields, "score")
			};

			// Parse metadata if exists
			if (fields.TryGetProperty("metadata", out var metadataField) &&
				metadataField.TryGetProperty("stringValue", out var metadataJson))
			{
				scoreData.Metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(metadataJson.GetString());
			}

			return scoreData;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Failed to convert score document: {ex.Message}");
			return null;
		}
	}

	private object ConvertLevelToFirestoreDocument(LevelData levelData)
	{
		var fields = new Dictionary<string, object>
	{
		{ "levelId", new { stringValue = levelData.LevelId } },
		{ "creatorId", new { stringValue = levelData.CreatorId } },
		{ "creatorName", new { stringValue = levelData.CreatorName } },
		{ "levelName", new { stringValue = levelData.LevelName } },
		{ "description", new { stringValue = levelData.Description ?? "" } },
		{ "levelDataJson", new { stringValue = levelData.LevelDataJson } },
		{ "difficulty", new { stringValue = levelData.Difficulty ?? "medium" } }
	};

		if (levelData.Tags != null && levelData.Tags.Count > 0)
		{
			fields["tags"] = new
			{
				arrayValue = new
				{
					values = levelData.Tags.Select(t => new { stringValue = t }).ToArray()
				}
			};
		}

		return new { fields };
	}

	private Dictionary<string, object> FirestoreString(string v)
	{
		return new Dictionary<string, object> { { "stringValue", v } };
	}

	private LevelData ConvertFromFirestoreLevelDocument(JsonElement doc)
	{
		try
		{
			var fields = doc.GetProperty("fields");

			var levelData = new LevelData
			{
				LevelId = GetStringField(fields, "levelId"),
				CreatorId = GetStringField(fields, "creatorId"),
				CreatorName = GetStringField(fields, "creatorName"),
				LevelName = GetStringField(fields, "levelName"),
				Description = GetStringField(fields, "description"),
				LevelDataJson = GetStringField(fields, "levelDataJson"),
				Difficulty = GetStringField(fields, "difficulty"),
				Tags = GetArrayField(fields, "tags")
			};

			return levelData;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Failed to convert level document: {ex.Message}");
			return null;
		}
	}

	private int GetIntField(JsonElement fields, string fieldName)
	{
		if (fields.TryGetProperty(fieldName, out var field))
		{
			if (field.TryGetProperty("integerValue", out var value))
			{
				return int.Parse(value.GetString());
			}
			else if (field.TryGetProperty("doubleValue", out var doubleValue))
			{
				return (int)doubleValue.GetDouble();
			}
		}
		return 0;
	}


	private class FirestoreResponse
	{
		public bool Success { get; set; }
		public JsonElement Data { get; set; }
		public int ResponseCode { get; set; }
	}
}