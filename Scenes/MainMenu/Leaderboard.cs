using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class Leaderboard : CenterContainer
{
	static private Label firstName;
	static private Label firstScore;
	static private Label secondName;
	static private Label secondScore;
	static private Label thirdName;
	static private Label thirdScore;
	static private Label fourthName;
	static private Label fourthScore;
	static private Label fifthName;
	static private Label fifthScore;
	static private Label sixthName;
	static private Label sixthScore;
	static private Label extraName;
	static private Label extraScore;

	private List<(string Name, float Score)> leaderboard = new();
	private List<(string Name, float Score)> friendsLeaderboard = new();
	private bool showingFriends = false;

	private FirebaseAuthManager _authManager;
	private FirestoreService _firestoreService;
	private Button friendsToggleButton;
	private int currentLevelId = 0;

	public override void _Ready()
	{
		InitializeServices();

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

		_authManager = FirebaseAuthManager.Instance;
		_authManager.AuthenticationChanged += OnLoginStateChanged;

		friendsToggleButton = GetNode<Button>("%FriendsToggleButton");
		friendsToggleButton.Pressed += OnFriendsTogglePressed;

		UpdateFriendsButtonVisibility();
		UpdateLeaderboard();
	}

	private void InitializeServices()
	{
		_authManager = FirebaseAuthManager.Instance ?? new FirebaseAuthManager();
		AddChild(_authManager);

		_firestoreService = FirestoreService.Instance ?? new FirestoreService();
		AddChild(_firestoreService);
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

	private async void OnFriendsTogglePressed()
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

	private void OnLoginStateChanged(bool loggedIn)
	{
		UpdateFriendsButtonVisibility();

		if (showingFriends && loggedIn)
			_ = LoadFriendsLeaderboard(currentLevelId);

		if (!loggedIn)
		{
			showingFriends = false;
			friendsToggleButton.Text = "Friends";
			UpdateLeaderboard();
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
		GD.Print("Loading friends leaderboard for level: ", levelIdStr);

		var friendScores = await _firestoreService.GetFriendScoresAsync(levelIdStr, 10);
		friendsLeaderboard.Clear();

		foreach (var score in friendScores)
			friendsLeaderboard.Add((score.Username, score.Score / 100f));

		var userScore = await _firestoreService.GetUserBestScoreAsync(_authManager.UserId, levelIdStr);
		if (userScore != null && !friendsLeaderboard.Any(e => e.Name == userScore.Username))
			friendsLeaderboard.Add((userScore.Username, userScore.Score / 100f));

		friendsLeaderboard = friendsLeaderboard.OrderByDescending(e => e.Score).ToList();
		DisplayLeaderboard(friendsLeaderboard);
	}

	private async Task _on_option_button_item_selectedAsync(int index)
	{
		currentLevelId = index;
		showingFriends = false;
		friendsToggleButton.Text = "Friends";
		await UpdateLeaderboardAsync();
		UpdateFriendsButtonVisibility();	
	}

	private async void UpdateLeaderboard()
	{
		if (_authManager != null && _authManager.IsAuthenticated && !showingFriends)
		{
			await LoadGlobalLeaderboard(currentLevelId);
		}
		else
		{
			LoadLocalLeaderboard(currentLevelId);
			DisplayLeaderboard(leaderboard.OrderByDescending(e => e.Score).ToList());
		}
	}

	private async Task UpdateLeaderboardAsync()
	{
		if (_authManager != null && _authManager.IsAuthenticated && !showingFriends)
		{
			await LoadGlobalLeaderboard(currentLevelId);
		}
		else
		{
			LoadLocalLeaderboard(currentLevelId);
			DisplayLeaderboard(leaderboard.OrderByDescending(e => e.Score).ToList());
		}
	}


	private async Task LoadGlobalLeaderboard(int levelId)
	{
		if (_firestoreService == null)
		{
			GD.PrintErr("Cannot load global leaderboard: FirestoreService is null");
			return;
		}

		string levelIdStr = $"level_{levelId}";
		GD.Print("Loading global leaderboard for level: ", levelIdStr);

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
		catch (Exception ex)
		{
			GD.PrintErr("Failed to load global leaderboard: ", ex.Message);
			LoadLocalLeaderboard(levelId);
			DisplayLeaderboard(leaderboard.OrderByDescending(e => e.Score).ToList());
		}
	}

	private void LoadLocalLeaderboard(int levelId)
	{
		leaderboard.Clear();
		LoadLeaderboard(levelId, leaderboard);
		bool retrievedLocalScore = LoadScore(levelId, leaderboard);
		if (!retrievedLocalScore)
			leaderboard.Add(("You", 0.0f));
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

		extraName = GetNodeOrNull<Label>("%extraName");
		extraScore = GetNodeOrNull<Label>("%extraScore");

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
		string SavePath = "user://Leaderboard/leaderboard" + levelId + ".leaderboard";
		// GD.Print("Leaderboard: SaveScore: path: ", SavePath);


		var saveFile = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
		// 'using' keyword means that this is automatically disposed of when going out of scope

		// if we can't open the file, then try and make the directory
		// and then try to open the file again
		if (saveFile == null)
		{
			GD.Print("Leaderboard: Could not save, err: ", FileAccess.GetOpenError());
			GD.Print("Leaderboard: Trying to create (recursive) directory(s) instead:");

			var dirSuccess = DirAccess.MakeDirRecursiveAbsolute("user://Leaderboard/leaderboard");

			// if 'ERROR' == 0, then good. else, not so good.
			if (dirSuccess != 0)
			{
				GD.Print("Leaderboard: Failed to make recursive directory(s): " + "user://Leaderboard/leaderboard");
				return;
			}

			// try again
			saveFile = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);

			if (saveFile == null)
			{
				GD.Print("Leaderboard: Could not save, err: ", FileAccess.GetOpenError());
				GD.Print("Leaderboard: Aborting save process.");
				return;
			}
			else
			{
				GD.Print("Leaderboard: Successfully created recursive directories and save file. Continue saving process now.");
			}
		}

		// BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

		Godot.Collections.Dictionary<string, Variant> data =
			new Godot.Collections.Dictionary<string, Variant>()
			{
				{ "username", scoreName },
				{ "score", scoreValue }
			};

		saveFile.StoreLine(Json.Stringify(data));
		((FileAccess)saveFile).Close();

		GD.Print("BoilerTronicsLevel: Leaderboard: Successfully saved to local, name: ", scoreName, ", score: ", scoreValue);
	}

	public static bool LoadScore(int levelId, List<(string, float)> scoreList)
	{
		string path = $"user://Leaderboard/leaderboard{levelId}.leaderboard";
		if (!FileAccess.FileExists(path)) return false;

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