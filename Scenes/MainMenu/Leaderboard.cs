using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class Leaderboard : CenterContainer
{
	static private Label firstName, firstScore;
	static private Label secondName, secondScore;
	static private Label thirdName, thirdScore;
	static private Label fourthName, fourthScore;
	static private Label fifthName, fifthScore;
	static private Label sixthName, sixthScore;
	static private Label extraName, extraScore;
	[Export] public Vector2I LevelNums {get; set;} = new Vector2I(6, 5);
	private List<(string Name, float Score)> leaderboard = new();
	private List<(string Name, float Score)> friendsLeaderboard = new();
	private bool showingFriends = false;

	private FirebaseAuthManager _authManager;
	private FirestoreService _firestoreService;
	private Button friendsToggleButton;
	private int currentLevelId = 11;

	// Hardcoded bc fml
	private readonly Dictionary<int, List<(string Name, float Score)>> defaultLeaderboards = new()
	{
		{ 0, new List<(string Name, float Score)>
			{
				("Abhi", 45.23f),
				("Keenan", 47.89f),
				("Ethan", 51.34f),
				("Ethen", 53.67f),
				("Zach", 56.12f),
				("You", 58.45f)
			}
		},
		{ 1, new List<(string Name, float Score)>
			{
				("Ethan", 62.45f),
				("Zach", 65.78f),
				("Ethen", 68.23f),
				("Keenan", 71.56f),
				("Abhi", 74.89f),
				("You", 77.34f)
			}
		},
		{ 2, new List<(string Name, float Score)>
			{
				("Ethan", 82.67f),
				("You", 85.12f),
				("Ethan", 88.45f),
				("Ethen", 91.78f),
				("Keenan", 94.23f),
				("Zach", 97.56f)
			}
		}
		// Add more levels as needed
	};

	public override void _Ready()
	{
		GD.Print("here");
		InitializeServices();
		GD.Print("init");
		firstName = GetNode<Label>("%firstName");
		firstScore = GetNode<Label>("%firstScore");
		secondName = GetNode<Label>("%secondName");
		secondScore = GetNode<Label>("%secondScore");
		thirdName = GetNode<Label>("%thirdName");
		thirdScore = GetNode<Label>("%thirdScore");
		fourthName = GetNode<Label>("%fourthName");
		fourthScore = GetNode<Label>("%fourthScore");
		fifthName = GetNode<Label>("%fifthName");
		fifthScore = GetNode<Label>("%fifthScore");
		sixthName = GetNode<Label>("%sixthName");
		sixthScore = GetNode<Label>("%sixthScore");

		extraName = GetNodeOrNull<Label>("%extraName");
		extraScore = GetNodeOrNull<Label>("%extraScore");
		
		_authManager = FirebaseAuthManager.Instance;
		_authManager.AuthenticationChanged += async (loggedIn) => await OnLoginStateChanged(loggedIn);
		
		friendsToggleButton = GetNode<Button>("%FriendsToggleButton");
		friendsToggleButton.Pressed += async () => await OnFriendsTogglePressed();

		GetLeaderboards();
		

		UpdateFriendsButtonVisibility();
		_ = UpdateLeaderboardAsync();
	}

	private void GetLeaderboards()
	{
		for (int i = 1; i <= LevelNums.Y; i++) {
			for (int j = 1; j <= LevelNums.X; j++) {
				String levelnum = i.ToString() + j.ToString();
				GetNode<OptionButton>("%OptionButton").AddItem("Level " + i.ToString() + "-" + j.ToString(), levelnum.ToInt());
			}
		}
		GetNode<OptionButton>("%OptionButton").GetSelectedId();
	}

	private void InitializeServices()
	{
		MainMenu.InitializeServices();
		this._authManager = MainMenu.GetFirebaseAuthManager();
		this._firestoreService = MainMenu.GetFirestoreService();
	}

	private async void UpdateFriendsButtonVisibility()
	{
		if (_authManager == null || !_authManager.IsAuthenticated)
		{
			friendsToggleButton.Visible = false;
			return;
		}

		var userData = await _firestoreService.GetUserAsync(_authManager.UserId);
		friendsToggleButton.Visible = userData != null && userData.Friends.Count > 0;
	}

	private async Task OnFriendsTogglePressed()
	{
		showingFriends = !showingFriends;

		if (showingFriends)
		{
			friendsToggleButton.Text = "Global";
			await LoadFriendsLeaderboard(currentLevelId);
		}
		else
		{
			friendsToggleButton.Text = "Friends";
			await UpdateLeaderboardAsync();
		}
	}

	private async Task OnLoginStateChanged(bool loggedIn)
	{
		UpdateFriendsButtonVisibility();

		if (loggedIn)
		{
			showingFriends = false;
			friendsToggleButton.Text = "Friends";
			await UpdateLeaderboardAsync();
		}
		else
		{
			showingFriends = false;
			friendsToggleButton.Text = "Friends";
			currentLevelId = 0;

			leaderboard.Clear();
			LoadDefaultLeaderboard(currentLevelId);
			DisplayLeaderboard(leaderboard);

			if (extraName != null) extraName.Visible = false;
			if (extraScore != null) extraScore.Visible = false;
		}
	}

	private async Task LoadFriendsLeaderboard(int levelId)
	{
		if (_firestoreService == null || _authManager == null || !_authManager.IsAuthenticated)
		{
			GD.PrintErr("Cannot load friends leaderboard: not authenticated");
			return;
		}

		string levelIdStr = $"level_{levelId}";
		var friendScores = await _firestoreService.GetFriendScoresAsync(levelIdStr, 10);
		friendsLeaderboard.Clear();
		GD.Print(friendScores);
		foreach (var score in friendScores)
			friendsLeaderboard.Add((score.Username, score.Score / 100f));

		var userScore = await _firestoreService.GetUserBestScoreAsync(_authManager.UserId, levelIdStr);
		if (userScore != null && !friendsLeaderboard.Any(e => e.Name == userScore.Username))
			friendsLeaderboard.Add((userScore.Username, userScore.Score / 100f));

		friendsLeaderboard = friendsLeaderboard.OrderByDescending(e => e.Score).ToList();
		DisplayLeaderboard(friendsLeaderboard);
	}

	private void _on_option_button_item_selected(int index)
	{
		int id = GetNode<OptionButton>("%OptionButton").GetSelectedId();
		GD.Print("id: ", id);
		_ =  _on_option_button_item_selectedAsync(id);
	}

	private async Task _on_option_button_item_selectedAsync(int index)
	{
		currentLevelId = index;
		showingFriends = false;
		friendsToggleButton.Text = "Friends";
		await UpdateLeaderboardAsync();
		UpdateFriendsButtonVisibility();
	}

	private async Task UpdateLeaderboardAsync()
	{
		if (_authManager != null && _authManager.IsAuthenticated && !showingFriends)
		{
			GD.Print("Level ID:" + currentLevelId);
			await LoadGlobalLeaderboard(currentLevelId);
		}
		else
		{
			GD.Print("Load Local");
			LoadLocalLeaderboard(currentLevelId);
		}
	}

	private async Task LoadGlobalLeaderboard(int levelId)
	{
		if (_firestoreService == null) return;

		string levelIdStr = $"level_{levelId}";

		try
		{
			var globalScores = await _firestoreService.GetGlobalLeaderboardAsync(levelIdStr, 6);
			leaderboard.Clear();

			foreach (var scoreEntry in globalScores)
				leaderboard.Add((scoreEntry.Username, scoreEntry.Score / 100f));

			var userScore = await _firestoreService.GetUserBestScoreAsync(_authManager.UserId, levelIdStr);
			if (userScore != null && !leaderboard.Any(e => e.Name == userScore.Username))
				leaderboard.Add((userScore.Username, userScore.Score / 100f));

			leaderboard = leaderboard.OrderByDescending(e => e.Score).ToList();
			DisplayLeaderboard(leaderboard);
		}
		catch
		{
			LoadLocalLeaderboard(levelId);
		}
	}

	private void LoadLocalLeaderboard(int levelId)
	{
		leaderboard.Clear();
		
		// Try to load from file first
		LoadLeaderboard(levelId, leaderboard);
		bool hasScore = LoadScore(levelId, leaderboard);
		
		// If no data exists, use default leaderboard
		if (leaderboard.Count == 0)
		{
			LoadDefaultLeaderboard(levelId);
		}
		// If only user score exists, add it to default leaderboard
		else if (leaderboard.Count == 1 && hasScore)
		{
			var userScore = leaderboard[0];
			LoadDefaultLeaderboard(levelId);
			if (!leaderboard.Any(e => e.Name == userScore.Name))
			{
				leaderboard.Add(userScore);
			}
			leaderboard = leaderboard.OrderByDescending(e => e.Score).ToList();
		}
		
		DisplayLeaderboard(leaderboard.OrderByDescending(e => e.Score).ToList());
	}

	private void LoadDefaultLeaderboard(int levelId)
	{
		leaderboard.Clear();
		
		// Use default leaderboard for this level if it exists
		if (defaultLeaderboards.ContainsKey(levelId))
		{
			leaderboard.AddRange(defaultLeaderboards[levelId]);
		}
		else
		{
			// Fallback to level 0 defaults if specific level doesn't exist
			leaderboard.AddRange(defaultLeaderboards[0]);
		}
	}

	private void DisplayLeaderboard(List<(string Name, float Score)> scoreList)
	{
		var labels = new (Label name, Label score)[]
		{
			(firstName, firstScore),
			(secondName, secondScore),
			(thirdName, thirdScore),
			(fourthName, fourthScore),
			(fifthName, fifthScore),
			(sixthName, sixthScore)
		};

		for (int i = 0; i < labels.Length; i++)
		{
			if (i < scoreList.Count)
			{
				labels[i].name.Text = scoreList[i].Name;
				labels[i].score.Text = scoreList[i].Score.ToString("F2");
			}
			else
			{
				labels[i].name.Text = "-";
				labels[i].score.Text = "-";
			}
		}

		var topEntries = scoreList.Take(6).ToList();
		var playerEntry = scoreList.FirstOrDefault(e => e.Name == "You");
		bool playerInTop = topEntries.Any(e => e.Name == "You");

		if (extraName != null && extraScore != null)
		{
			if (!playerInTop && playerEntry.Name != null)
			{
				extraName.Text = playerEntry.Name;
				extraScore.Text = playerEntry.Score.ToString("F2");
				extraName.Visible = extraScore.Visible = true;
				extraName.AddThemeColorOverride("font_color", new Color(0.3f, 0.3f, 0.3f));
				extraScore.AddThemeColorOverride("font_color", new Color(0.3f, 0.3f, 0.3f));
			}
			else
			{
				extraName.Visible = extraScore.Visible = false;
			}
		}
	}

	public void LoadLeaderboard(int levelId, List<(string, float)> scoreList)
	{
		string path = $"res://Resources/Levels/leaderboard{levelId}.leaderboard";
		if (!FileAccess.FileExists(path)) return;

		using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
		while (file.GetPosition() < file.GetLength())
		{
			var jsonString = file.GetLine();
			var json = new Json();
			if (json.Parse(jsonString) != Error.Ok) continue;

			var nodeData = new Godot.Collections.Dictionary<string, Variant>((Godot.Collections.Dictionary)json.Data);
			string name = (string)nodeData["username"];
			float score = (float)nodeData["score"];
			scoreList.Add((name, score));
		}
	}

	public static void SaveScore(int levelId, string scoreName, float scoreValue)
	{
		string path = $"user://Leaderboard/leaderboard{levelId}.leaderboard";
		var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
		if (file == null)
		{
			DirAccess.MakeDirRecursiveAbsolute("user://Leaderboard");
			file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
			if (file == null) return;
		}

		var data = new Godot.Collections.Dictionary<string, Variant>
		{
			{ "username", scoreName },
			{ "score", scoreValue }
		};
		file.StoreLine(Json.Stringify(data));
		file.Close();
	}

	public static bool LoadScore(int levelId, List<(string, float)> scoreList)
	{
		string path = $"user://Leaderboard/leaderboard{levelId}.leaderboard";
		if (!FileAccess.FileExists(path))
		{
			return false;
		} 

		using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
		var jsonString = file.GetLine();
		var json = new Json();
		if (json.Parse(jsonString) != Error.Ok) return false;

		var nodeData = new Godot.Collections.Dictionary<string, Variant>((Godot.Collections.Dictionary)json.Data);
		string name = (string)nodeData["username"];
		float score = (float)nodeData["score"];
		scoreList.Add((name, score));
		return true;
	}
}