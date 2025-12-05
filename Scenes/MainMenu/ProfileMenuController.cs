using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ProfileMenuController : Control
{
	private FirebaseAuthManager _authManager;
	public bool IsAuthenticated() { return _authManager.IsAuthenticated; }	// publicly facing function to get authentication status
	private FirestoreService _firestoreService;
	private FriendsService _friendsService;
	private UserData _currentUserData;
	public UserData GetUserData() { return _currentUserData; }	// publicly facing function to return UserData

	private HBoxContainer _mainContainer;
	private VBoxContainer _profileContainer;
	private VBoxContainer _friendsContainer;

	private float _pollTimer = 0f;

	// Cached values for diff detection
	private int _cachedFriendsCount = -1;
	private int _cachedRequestsCount = -1;
	private int _cachedAchievements = -1;
	private float _cachedHoursPlayed = -1;
	
	// publicly facing manager such that it is possible for other scripts to get _UserData and etc
	public static ProfileMenuController GlobalManager;


	public override void _Ready()
	{
		GlobalManager = this;
		
		// Get services
		_authManager = FirebaseAuthManager.Instance;
		_firestoreService = FirestoreService.Instance;
		_friendsService = FriendsService.Instance;

		if (_friendsService == null)
		{
			_friendsService = new FriendsService();
			GetTree().Root.AddChild(_friendsService);
		}

		// Build the new layout
		BuildLayout();

		// Subscribe to auth changes
		if (_authManager != null)
		{
			_authManager.AuthenticationChanged += OnAuthenticationChanged;

			// If already authenticated, load user data immediately
			if (_authManager.IsAuthenticated)
			{
				LoadUserDataDeferred();
			}
		}

		// Initial update
		UpdateProfileMenu();
	}

	private void UpdateProfileStatsOnly()
	{
		if (!IsInstanceValid(_profileContainer)) return;
		if (!IsInstanceValid(_friendsContainer)) return;

		if (_profileContainer == null)
			return;

		// Ensure the grid exists, create if missing
		GridContainer grid;
		if (_profileContainer.GetChildCount() < 2 || !(_profileContainer.GetChild(1) is GridContainer existingGrid))
		{
			grid = new GridContainer();
			grid.Columns = 2;
			_profileContainer.AddChild(grid);
		}
		else
		{
			grid = existingGrid;
		}

		foreach (Node child in grid.GetChildren())
			child.QueueFree();

		AddStatLabel(grid, $"Hours Played: {_currentUserData.HoursPlayed:F1}");
		AddStatLabel(grid, $"Friends: {_currentUserData.Friends.Count}");
		//AddStatLabel(grid, $"Achievements: {_currentUserData.AchievementsUnlocked.Count}");
		//AddStatLabel(grid, $"Easter Eggs: {_currentUserData.EasterEggsFound.Count}");
	}

	private void UpdateFriendRequestsOnly(List<FriendRequest> requests)
	{
		if (!IsInstanceValid(_profileContainer)) return;
		if (!IsInstanceValid(_friendsContainer)) return;

		// Get or create the RequestsList container
		var scroll = _friendsContainer.GetNodeOrNull<ScrollContainer>("RequestsScollContainer");
		var requestsList = scroll.GetNodeOrNull<VBoxContainer>("RequestsList");
		if (requestsList == null)
		{
			// Remove old children inside the scroll container (if any)
			foreach (var child in scroll.GetChildren())
				child.QueueFree();

			// Recreate the list while preserving formatting
			requestsList = new VBoxContainer();
			requestsList.Name = "RequestsList";
			requestsList.AddThemeConstantOverride("separation", 8);
			requestsList.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;

			scroll.AddChild(requestsList);
		}

		// Clear previous children
		foreach (Node c in requestsList.GetChildren())
			c.QueueFree();

		if (requests.Count == 0)
		{
			var l = CreateLabel("No pending requests", 14);
			l.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
			requestsList.AddChild(l);
			return;
		}

		foreach (var request in requests)
		{
			var row = new HBoxContainer();
			row.AddThemeConstantOverride("separation", 8);

			var from = CreateLabel($"{request.FromUsername}", 16);
			row.AddChild(from);

			var accept = CreateSmallButton("✓");
			accept.CustomMinimumSize = new Vector2(35, 30);
			accept.AddThemeColorOverride("font_color", new Color(0, 0.6f, 0, 1));
			accept.Pressed += async () => await OnAcceptFriendRequest(request);
			row.AddChild(accept);

			var decline = CreateSmallButton("✗");
			decline.CustomMinimumSize = new Vector2(35, 30);
			decline.AddThemeColorOverride("font_color", new Color(0.8f, 0, 0, 1));
			decline.Pressed += async () => await OnDeclineFriendRequest(request);
			row.AddChild(decline);

			requestsList.AddChild(row);
		}
	}


	private async void UpdateFriendsListOnly()
	{

		if (!IsInstanceValid(_profileContainer)) return;
		if (!IsInstanceValid(_friendsContainer)) return;

		if (_friendsContainer == null)
			return;

		// Ensure a ScrollContainer exists for friends list
		var friendsScroll = _friendsContainer.GetNodeOrNull<ScrollContainer>("FriendsScroll");
		VBoxContainer friendsList;

		if (friendsScroll == null)
		{
			friendsScroll = new ScrollContainer();
			friendsScroll.Name = "FriendsScroll";
			_friendsContainer.AddChild(friendsScroll);

			friendsList = new VBoxContainer();
			friendsList.Name = "FriendsList";
			friendsScroll.AddChild(friendsList);
		}
		else
		{
			friendsList = friendsScroll.GetChildOrNull<VBoxContainer>(0);
			if (friendsList == null)
			{
				friendsList = new VBoxContainer();
				friendsList.Name = "FriendsList";
				friendsScroll.AddChild(friendsList);
			}
		}

		foreach (Node c in friendsList.GetChildren())
			c.QueueFree();

		if (_currentUserData.Friends.Count == 0)
		{
			var noFriends = CreateLabel("No friends yet", 16);
			noFriends.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
			friendsList.AddChild(noFriends);
			return;
		}

		var friendNames = await _friendsService.GetFriendUsernamesAsync(_currentUserData.Friends);
		foreach (var name in friendNames)
		{
			friendsList.AddChild(CreateLabel($"• {name}", 18));
		}
	}


	private async System.Threading.Tasks.Task PollForUpdatesAsync()
	{
		// get fresh user data
		var newUserData = await _firestoreService.GetUserAsync(_authManager.UserId);
		if (newUserData == null)
			return;

		// FRIEND REQUEST COUNT
		var incomingRequests = await _friendsService.GetIncomingFriendRequestsAsync(_authManager.UserId);
		int newReqCount = incomingRequests.Count;

		// FRIEND COUNT
		int newFriendCount = newUserData.Friends.Count;

		// HOURS PLAYED + ACHIEVEMENTS
		float newHours = newUserData.HoursPlayed;
		int newAchievements = newUserData.AchievementsUnlocked.Count;

		bool updateStats = false;
		bool updateRequests = false;
		bool updateFriends = false;

		GD.Print("Polling");

		if (_cachedFriendsCount != newFriendCount)
			updateStats = updateFriends = true;
		if (_cachedRequestsCount != newReqCount)
			updateRequests = true;
		if (_cachedHoursPlayed != newHours || _cachedAchievements != newAchievements)
			updateStats = true;

		// apply new cache
		_cachedFriendsCount = newFriendCount;
		_cachedRequestsCount = newReqCount;
		_cachedHoursPlayed = newHours;
		_cachedAchievements = newAchievements;

		// update local current user data
		_currentUserData = newUserData;

		// perform partial updates
		if (updateStats)
			UpdateProfileStatsOnly();

		if (updateRequests)
			UpdateFriendRequestsOnly(incomingRequests);

		if (updateFriends)
			UpdateFriendsListOnly();
	}


	public override void _Process(double delta)
	{
		if (!IsVisibleInTree() || !(_authManager?.IsAuthenticated ?? false))
			return;

		_pollTimer += (float)delta;
		if (_pollTimer >= 15f)
		{
			_pollTimer = 0f;
			_ = PollForUpdatesAsync();
		}
	}



	public override void _ExitTree()
	{
		if (_authManager != null)
		{
			_authManager.AuthenticationChanged -= OnAuthenticationChanged;
		}
	}

	private void BuildLayout()
	{
		foreach (Node child in GetChildren())
		{
			child.QueueFree();
		}

		var rootContainer = new VBoxContainer();
		rootContainer.AnchorRight = 1;
		rootContainer.AnchorBottom = 1;
		rootContainer.AddThemeConstantOverride("separation", 20);
		AddChild(rootContainer);

		_mainContainer = new HBoxContainer();
		_mainContainer.CustomMinimumSize = new Vector2(0, 550);
		_mainContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
		_mainContainer.AddThemeConstantOverride("separation", 30);
		_mainContainer.Alignment = BoxContainer.AlignmentMode.Center;
		rootContainer.AddChild(_mainContainer);

		var profilePanel = CreateStyledPanel(new Vector2(900, 550));
		_mainContainer.AddChild(profilePanel);

		_profileContainer = new VBoxContainer();
		_profileContainer.AnchorRight = 1;
		_profileContainer.AnchorBottom = 1;
		_profileContainer.GrowHorizontal = Control.GrowDirection.Both;
		_profileContainer.GrowVertical = Control.GrowDirection.Both;
		_profileContainer.AddThemeConstantOverride("separation", 80);
		profilePanel.AddChild(_profileContainer);

		var profileTitle = CreateTitleLabel("Profile");
		_profileContainer.AddChild(profileTitle);

		var friendsPanel = CreateStyledPanel(new Vector2(500, 550));
		_mainContainer.AddChild(friendsPanel);

		_friendsContainer = new VBoxContainer();
		_friendsContainer.AnchorRight = 1;
		_friendsContainer.AnchorBottom = 1;
		_friendsContainer.GrowHorizontal = Control.GrowDirection.Both;
		_friendsContainer.GrowVertical = Control.GrowDirection.Both;
		_friendsContainer.AddThemeConstantOverride("separation", 15);
		friendsPanel.AddChild(_friendsContainer);

		var friendsTitle = CreateTitleLabel("Friends");
		_friendsContainer.AddChild(friendsTitle);

		var backButton = CreateBackButton();
		rootContainer.AddChild(backButton);
	}

	private Button CreateBackButton()
	{
		var backButton = new Button();
		backButton.Text = "Back";
		backButton.CustomMinimumSize = new Vector2(600, 100);
		backButton.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;

		var font = GD.Load<FontFile>("res://Resources/Fonts/VCR_OSD_MONO_1.001.ttf");
		backButton.AddThemeFontOverride("font", font);
		backButton.AddThemeFontSizeOverride("font_size", 76);
		backButton.AddThemeColorOverride("font_color", new Color(0, 0, 0, 1));

		var normalStyle = GD.Load<StyleBox>("res://Resources/ButtonThemes/menubuttonnorm.tres");
		var hoverStyle = GD.Load<StyleBox>("res://Resources/ButtonThemes/menubuttonhover.tres");
		backButton.AddThemeStyleboxOverride("normal", normalStyle);
		backButton.AddThemeStyleboxOverride("hover", hoverStyle);
		backButton.AddThemeStyleboxOverride("focus", normalStyle);

		backButton.Pressed += () =>
		{
			GetParent().GetNode<Control>("SettingsMenu").Visible = false;
			GetParent().GetNode<Control>("ProfileMenu").Visible = false;
			GetParent().GetNode<Control>("Leaderboard").Visible = false;
			GetParent().GetNode<Control>("MainMenu").Visible = true;
		};

		return backButton;
	}

	private Panel CreateStyledPanel(Vector2 size)
	{
		var panel = new Panel();
		panel.CustomMinimumSize = size;
		panel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;

		var styleBox = new StyleBoxFlat();
		styleBox.BgColor = new Color(0.8117647f, 0.7254902f, 0.5686275f, 1);
		styleBox.BorderWidthLeft = 4;
		styleBox.BorderWidthTop = 4;
		styleBox.BorderWidthRight = 4;
		styleBox.BorderWidthBottom = 4;
		styleBox.BorderColor = new Color(0, 0, 0, 1);
		styleBox.CornerRadiusTopLeft = 20;
		styleBox.CornerRadiusTopRight = 20;
		styleBox.CornerRadiusBottomRight = 20;
		styleBox.CornerRadiusBottomLeft = 20;

		panel.AddThemeStyleboxOverride("panel", styleBox);

		return panel;
	}

	private Label CreateTitleLabel(string text)
	{
		var label = new Label();
		label.Text = text;
		label.HorizontalAlignment = HorizontalAlignment.Center;
		label.AddThemeColorOverride("font_color", new Color(1, 1, 1, 1));
		label.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.69411767f));
		label.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0, 1));
		label.AddThemeConstantOverride("shadow_offset_x", 5);
		label.AddThemeConstantOverride("shadow_offset_y", 14);
		label.AddThemeConstantOverride("outline_size", 6);

		var font = GD.Load<FontFile>("res://Resources/Fonts/VCR_OSD_MONO_1.001.ttf");
		label.AddThemeFontOverride("font", font);
		label.AddThemeFontSizeOverride("font_size", 70);

		return label;
	}

	private async void LoadUserDataDeferred()
	{
		await LoadUserData();
		UpdateProfileMenu();
	}

	private async void OnAuthenticationChanged(bool isAuthenticated)
	{
		// Stop polling during logout
		if (!isAuthenticated)
			_pollTimer = 0f;

		if (isAuthenticated)
		{
			await LoadUserData();
		}
		else
		{
			_currentUserData = null;
		}

		if (IsInstanceValid(this) && Visible)
		{
			CallDeferred(nameof(UpdateProfileMenu));
		}
	}


	private async System.Threading.Tasks.Task LoadUserData()
	{
		if (_authManager == null || !_authManager.IsAuthenticated)
			return;

		_currentUserData = await _firestoreService.GetUserAsync(_authManager.UserId);

		if (_currentUserData == null)
		{
			_currentUserData = UserData.CreateDefault(_authManager.UserId, _authManager.Email);
			await _firestoreService.CreateUserAsync(_authManager.UserId, _currentUserData);
		}
	}

	public void UpdateProfileMenu()
	{
		if (!IsInstanceValid(_profileContainer) || !IsInstanceValid(_friendsContainer))
			return;

		// Clear profile content (keep title)
		for (int i = _profileContainer.GetChildCount() - 1; i >= 1; i--)
		{
			var child = _profileContainer.GetChild(i);
			if (IsInstanceValid(child))
				child.QueueFree();
		}

		// Clear friends content (keep title)
		for (int i = _friendsContainer.GetChildCount() - 1; i >= 1; i--)
		{
			var child = _friendsContainer.GetChild(i);
			if (IsInstanceValid(child))
				child.QueueFree();
		}

		if (_authManager.IsAuthenticated && _currentUserData != null)
		{
			ShowAuthenticatedProfile();
			ShowFriendsPanel();
		}
		else
		{
			_profileContainer.GetChild<Label>(0).Text = "Profile";
			ShowLoginForm();
		}
	}


	private void ShowAuthenticatedProfile()
	{
		// Update title
		_profileContainer.GetChild<Label>(0).Text = _currentUserData.Username;

		// Stats grid
		var gridContainer = new GridContainer();
		gridContainer.Columns = 2;
		gridContainer.AddThemeConstantOverride("h_separation", 150);
		gridContainer.AddThemeConstantOverride("v_separation", 30);
		gridContainer.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;

		AddStatLabel(gridContainer, $"Hours Played: {_currentUserData.HoursPlayed:F1}");
		AddStatLabel(gridContainer, $"Friends: {_currentUserData.Friends.Count}");
		//AddStatLabel(gridContainer, $"Achievements: {_currentUserData.AchievementsUnlocked.Count}");
		//AddStatLabel(gridContainer, $"Easter Eggs: {_currentUserData.EasterEggsFound.Count}");

		_profileContainer.AddChild(gridContainer);

		 var buttonContainer = new VBoxContainer();
		buttonContainer.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;

		buttonContainer.AddThemeConstantOverride("separation", 8);
		_profileContainer.AddChild(buttonContainer);
		
		// Achievements button
		var achievementsButton = new Button();
		achievementsButton.CustomMinimumSize = new Vector2(400, 45);
		achievementsButton.Text = "Achievements";
		achievementsButton.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;

		var font = GD.Load<FontFile>("res://Resources/Fonts/VCR_OSD_MONO_1.001.ttf");
		achievementsButton.AddThemeFontOverride("font", font);
		achievementsButton.AddThemeFontSizeOverride("font_size", 45);
		achievementsButton.AddThemeColorOverride("font_color", new Color(0, 0, 0, 1));

		var normalStyle = GD.Load<StyleBox>("res://Resources/ButtonThemes/menubuttonnorm.tres");
		var hoverStyle = GD.Load<StyleBox>("res://Resources/ButtonThemes/menubuttonhover.tres");
		achievementsButton.AddThemeStyleboxOverride("normal", normalStyle);
		achievementsButton.AddThemeStyleboxOverride("hover", hoverStyle);
		achievementsButton.AddThemeStyleboxOverride("focus", normalStyle);

		achievementsButton.Pressed += () =>
		{
			var main = GetTree().CurrentScene as MainMenu;
			main._on_achievements_pressed();
		};
		buttonContainer.AddChild(achievementsButton);
		
		// EasterEgg button
		var easterEggsButton = new Button();
		easterEggsButton.CustomMinimumSize = new Vector2(400, 45);
		easterEggsButton.Text = "Easter Eggs";
		easterEggsButton.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;

		easterEggsButton.AddThemeFontOverride("font", font);
		easterEggsButton.AddThemeFontSizeOverride("font_size", 45);
		easterEggsButton.AddThemeColorOverride("font_color", new Color(0, 0, 0, 1));

		easterEggsButton.AddThemeStyleboxOverride("normal", normalStyle);
		easterEggsButton.AddThemeStyleboxOverride("hover", hoverStyle);
		easterEggsButton.AddThemeStyleboxOverride("focus", normalStyle);

		easterEggsButton.Pressed += () =>
		{
			var main = GetTree().CurrentScene as MainMenu;
			main._on_easter_eggs_pressed();
		};
		buttonContainer.AddChild(easterEggsButton);

		// Logout button
		var logoutButton = new Button();
		logoutButton.CustomMinimumSize = new Vector2(400, 80);
		logoutButton.Text = "Logout";
		logoutButton.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;

		//var font = GD.Load<FontFile>("res://Resources/Fonts/VCR_OSD_MONO_1.001.ttf");
		logoutButton.AddThemeFontOverride("font", font);
		logoutButton.AddThemeFontSizeOverride("font_size", 50);
		logoutButton.AddThemeColorOverride("font_color", new Color(0, 0, 0, 1));

		//var normalStyle = GD.Load<StyleBox>("res://Resources/ButtonThemes/menubuttonnorm.tres");
		//var hoverStyle = GD.Load<StyleBox>("res://Resources/ButtonThemes/menubuttonhover.tres");
		logoutButton.AddThemeStyleboxOverride("normal", normalStyle);
		logoutButton.AddThemeStyleboxOverride("hover", hoverStyle);
		logoutButton.AddThemeStyleboxOverride("focus", normalStyle);

		logoutButton.Pressed += () => _authManager.SignOut();
		buttonContainer.AddChild(logoutButton);
	}

	private async void ShowFriendsPanel()
	{
		// Search bar
		var searchContainer = new HBoxContainer();
		searchContainer.AddThemeConstantOverride("separation", 10);
		searchContainer.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;

		var searchInput = CreateLineEdit("Search username...", 320, 40);
		searchInput.Name = "SearchInput";

		var searchButton = CreateSmallButton("+");
		searchButton.CustomMinimumSize = new Vector2(50, 40);
		searchButton.Pressed += () => OnSearchPressed(searchInput);

		searchContainer.AddChild(searchInput);
		searchContainer.AddChild(searchButton);
		_friendsContainer.AddChild(searchContainer);

		// Search result area
		var searchResultContainer = new VBoxContainer();
		searchResultContainer.Name = "SearchResult";
		searchResultContainer.AddThemeConstantOverride("separation", 10);
		searchResultContainer.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		_friendsContainer.AddChild(searchResultContainer);

		// Friend Requests Section
		var requestsLabel = CreateLabel("Friend Requests:", 20);
		requestsLabel.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		_friendsContainer.AddChild(requestsLabel);

		var requestsScrollContainer = new ScrollContainer();
		requestsScrollContainer.Name = "RequestsScollContainer";
		requestsScrollContainer.CustomMinimumSize = new Vector2(450, 120);
		requestsScrollContainer.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
		requestsScrollContainer.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;

		var requestsList = new VBoxContainer();
		requestsList.Name = "RequestsList";
		requestsList.AddThemeConstantOverride("separation", 8);
		requestsList.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		requestsScrollContainer.AddChild(requestsList);
		_friendsContainer.AddChild(requestsScrollContainer);

		// Load incoming friend requests
		var incomingRequests = await _friendsService.GetIncomingFriendRequestsAsync(_authManager.UserId);
		if (incomingRequests.Count > 0)
		{
			foreach (var request in incomingRequests)
			{
				var requestContainer = new HBoxContainer();
				requestContainer.AddThemeConstantOverride("separation", 8);

				var fromLabel = CreateLabel($"{request.FromUsername}", 16);
				requestContainer.AddChild(fromLabel);

				var acceptButton = CreateSmallButton("✓");
				acceptButton.CustomMinimumSize = new Vector2(35, 30);
				acceptButton.AddThemeColorOverride("font_color", new Color(0, 0.6f, 0, 1));
				acceptButton.Pressed += async () => await OnAcceptFriendRequest(request);
				requestContainer.AddChild(acceptButton);

				var declineButton = CreateSmallButton("✗");
				declineButton.CustomMinimumSize = new Vector2(35, 30);
				declineButton.AddThemeColorOverride("font_color", new Color(0.8f, 0, 0, 1));
				declineButton.Pressed += async () => await OnDeclineFriendRequest(request);
				requestContainer.AddChild(declineButton);

				requestsList.AddChild(requestContainer);
			}
		}
		else
		{
			var noRequestsLabel = CreateLabel("No pending requests", 14);
			noRequestsLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
			requestsList.AddChild(noRequestsLabel);
		}

		// Friends list
		var friendsListLabel = CreateLabel("Your Friends:", 20);
		friendsListLabel.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		_friendsContainer.AddChild(friendsListLabel);

		var friendsScrollContainer = new ScrollContainer();
		friendsScrollContainer.CustomMinimumSize = new Vector2(450, 120);
		friendsScrollContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
		friendsScrollContainer.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;

		var friendsList = new VBoxContainer();
		friendsList.AddThemeConstantOverride("separation", 10);
		friendsList.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		friendsScrollContainer.AddChild(friendsList);
		_friendsContainer.AddChild(friendsScrollContainer);

		// Load friends
		if (_currentUserData.Friends.Count > 0)
		{
			var friendUsernames = await _friendsService.GetFriendUsernamesAsync(_currentUserData.Friends);
			foreach (var username in friendUsernames)
			{
				var friendLabel = CreateLabel($"• {username}", 18);
				friendsList.AddChild(friendLabel);
			}
		}
		else
		{
			var noFriendsLabel = CreateLabel("No friends yet", 16);
			noFriendsLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
			friendsList.AddChild(noFriendsLabel);
		}
	}

	private async System.Threading.Tasks.Task OnAcceptFriendRequest(FriendRequest request)
	{
		GD.Print($"Accepting friend request from {request.FromUsername}");

		// add to both 
		var currentFriends = new List<string>(_currentUserData.Friends);
		if (!currentFriends.Contains(request.FromUserId))
		{
			currentFriends.Add(request.FromUserId);
			await _firestoreService.UpdateUserFieldAsync(_authManager.UserId, "friends", currentFriends);
			_currentUserData.Friends = currentFriends;
		}

		// Add current user to their bum ass friends spot
		var otherUserData = await _firestoreService.GetUserAsync(request.FromUserId);
		if (otherUserData != null)
		{
			var otherFriends = new List<string>(otherUserData.Friends);
			if (!otherFriends.Contains(_authManager.UserId))
			{
				otherFriends.Add(_authManager.UserId);
				await _firestoreService.UpdateUserFieldAsync(request.FromUserId, "friends", otherFriends);
			}
		}

		// Update friend request status
		string requestId = $"{request.FromUserId}_{request.ToUserId}";
		await UpdateFriendRequestStatus(requestId, "accepted");

		// Refresh the UI
		UpdateProfileMenu(); // this is slow as shit and not good but fuck if i know
	}

	private async System.Threading.Tasks.Task OnDeclineFriendRequest(FriendRequest request)
	{
		GD.Print($"Declining friend request from {request.FromUsername}");

		// Update friend request status to declined
		string requestId = $"{request.FromUserId}_{request.ToUserId}";
		await UpdateFriendRequestStatus(requestId, "declined");

		// Refresh the UI
		UpdateProfileMenu();
	}

	private async System.Threading.Tasks.Task UpdateFriendRequestStatus(string requestId, string status)
	{
		await _friendsService.UpdateFriendRequestStatusAsync(requestId, status);
	}

	private async void OnSearchPressed(LineEdit searchInput)
	{
		string username = searchInput.Text.Trim();
		if (string.IsNullOrEmpty(username))
			return;

		var searchResultContainer = _friendsContainer.GetNode<VBoxContainer>("SearchResult");

		// Clear previous results
		foreach (Node child in searchResultContainer.GetChildren())
		{
			child.QueueFree();
		}

		// Search for user
		var foundUser = await _friendsService.SearchUserByUsernameAsync(username);

		if (foundUser == null)
		{
			var notFoundLabel = CreateLabel("User not found", 18);
			notFoundLabel.AddThemeColorOverride("font_color", new Color(1, 0.3f, 0.3f));
			searchResultContainer.AddChild(notFoundLabel);
		}
		else if (foundUser.Uuid == _authManager.UserId)
		{
			var selfLabel = CreateLabel("That's you!", 18);
			selfLabel.AddThemeColorOverride("font_color", new Color(1, 0.7f, 0.3f));
			searchResultContainer.AddChild(selfLabel);
		}
		else if (_currentUserData.Friends.Contains(foundUser.Uuid))
		{
			var alreadyFriendLabel = CreateLabel($"{foundUser.Username} is already your friend", 18);
			alreadyFriendLabel.AddThemeColorOverride("font_color", new Color(0.3f, 0.8f, 0.3f));
			searchResultContainer.AddChild(alreadyFriendLabel);
		}
		else
		{
			var resultContainer = new HBoxContainer();
			resultContainer.AddThemeConstantOverride("separation", 10);

			var usernameLabel = CreateLabel($"Found: {foundUser.Username}", 18);
			usernameLabel.AddThemeColorOverride("font_color", new Color(0.2f, 0.6f, 0.2f));

			var sendRequestButton = CreateSmallButton("Send Request");
			sendRequestButton.Pressed += async () =>
			{
				await _friendsService.SendFriendRequestAsync(foundUser.Uuid, foundUser.Username);

				// Show feedback
				foreach (Node child in searchResultContainer.GetChildren())
				{
					child.QueueFree();
				}
				var sentLabel = CreateLabel("Friend request sent!", 18);
				sentLabel.AddThemeColorOverride("font_color", new Color(0.2f, 0.8f, 0.2f));
				searchResultContainer.AddChild(sentLabel);

				// Clear search input
				searchInput.Text = "";
			};

			resultContainer.AddChild(usernameLabel);
			resultContainer.AddChild(sendRequestButton);
			searchResultContainer.AddChild(resultContainer);
		}
	}

	private void ShowLoginForm()
	{
		// Simple login form in profile section
		var formContainer = new VBoxContainer();
		formContainer.AddThemeConstantOverride("separation", 15);
		formContainer.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		formContainer.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;

		var emailLabel = CreateLabel("Email:", 25);
		var emailInput = CreateLineEdit("Enter email", 400, 50);

		var passwordLabel = CreateLabel("Password:", 25);
		var passwordInput = CreateLineEdit("Enter password", 400, 50);
		passwordInput.Secret = true;

		var errorLabel = CreateLabel("", 18);
		errorLabel.AddThemeColorOverride("font_color", new Color(1, 0.3f, 0.3f));
		errorLabel.Visible = false;
		errorLabel.HorizontalAlignment = HorizontalAlignment.Center;

		var buttonContainer = new HBoxContainer();
		buttonContainer.AddThemeConstantOverride("separation", 20);
		buttonContainer.Alignment = BoxContainer.AlignmentMode.Center;

		var loginButton = CreateAuthButton("Login");
		loginButton.Pressed += () => OnLoginPressed(emailInput, passwordInput, errorLabel);

		var registerButton = CreateAuthButton("Register");
		registerButton.Pressed += () => OnRegisterPressed(emailInput, passwordInput, errorLabel);

		buttonContainer.AddChild(loginButton);
		buttonContainer.AddChild(registerButton);

		formContainer.AddChild(emailLabel);
		formContainer.AddChild(emailInput);
		formContainer.AddChild(passwordLabel);
		formContainer.AddChild(passwordInput);
		formContainer.AddChild(errorLabel);
		formContainer.AddChild(buttonContainer);

		_profileContainer.AddChild(formContainer);
	}

	private async void OnLoginPressed(LineEdit emailInput, LineEdit passwordInput, Label errorLabel)
	{
		string email = emailInput.Text.Trim();
		string password = passwordInput.Text;

		if (!ValidateInput(email, password, errorLabel))
			return;

		emailInput.Editable = false;
		passwordInput.Editable = false;
		errorLabel.Text = "Logging in...";
		errorLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
		errorLabel.Visible = true;

		var result = await _authManager.SignInAsync(email, password);

		emailInput.Editable = true;
		passwordInput.Editable = true;

		if (!result.Success)
		{
			errorLabel.Text = result.ErrorMessage;
			errorLabel.AddThemeColorOverride("font_color", new Color(1, 0.3f, 0.3f));
			errorLabel.Visible = true;
		}
	}

	private async void OnRegisterPressed(LineEdit emailInput, LineEdit passwordInput, Label errorLabel)
	{
		string email = emailInput.Text.Trim();
		string password = passwordInput.Text;

		if (!ValidateInput(email, password, errorLabel))
			return;

		emailInput.Editable = false;
		passwordInput.Editable = false;
		errorLabel.Text = "Creating account...";
		errorLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
		errorLabel.Visible = true;

		var result = await _authManager.RegisterAsync(email, password);

		emailInput.Editable = true;
		passwordInput.Editable = true;

		if (!result.Success)
		{
			errorLabel.Text = result.ErrorMessage;
			errorLabel.AddThemeColorOverride("font_color", new Color(1, 0.3f, 0.3f));
			errorLabel.Visible = true;
		}
	}

	private bool ValidateInput(string email, string password, Label errorLabel)
	{
		if (string.IsNullOrWhiteSpace(email))
		{
			errorLabel.Text = "Please enter an email address";
			errorLabel.AddThemeColorOverride("font_color", new Color(1, 0.3f, 0.3f));
			errorLabel.Visible = true;
			return false;
		}

		if (string.IsNullOrWhiteSpace(password))
		{
			errorLabel.Text = "Please enter a password";
			errorLabel.AddThemeColorOverride("font_color", new Color(1, 0.3f, 0.3f));
			errorLabel.Visible = true;
			return false;
		}

		if (password.Length < 6)
		{
			errorLabel.Text = "Password must be at least 6 characters";
			errorLabel.AddThemeColorOverride("font_color", new Color(1, 0.3f, 0.3f));
			errorLabel.Visible = true;
			return false;
		}

		return true;
	}

	// UI Helper Methods

	private Label CreateLabel(string text, int fontSize)
	{
		var label = new Label();
		label.Text = text;
		label.AddThemeColorOverride("font_color", new Color(0, 0, 0, 1));

		var font = GD.Load<FontFile>("res://Resources/Fonts/VCR_OSD_MONO_1.001.ttf");
		label.AddThemeFontOverride("font", font);
		label.AddThemeFontSizeOverride("font_size", fontSize);

		return label;
	}

	private LineEdit CreateLineEdit(string placeholder, int width, int height)
	{
		var lineEdit = new LineEdit();
		lineEdit.PlaceholderText = placeholder;
		lineEdit.CustomMinimumSize = new Vector2(width, height);
		lineEdit.AddThemeColorOverride("font_color", new Color(0, 0, 0, 1));

		var font = GD.Load<FontFile>("res://Resources/Fonts/VCR_OSD_MONO_1.001.ttf");
		lineEdit.AddThemeFontOverride("font", font);
		lineEdit.AddThemeFontSizeOverride("font_size", 18);

		return lineEdit;
	}

	private Button CreateAuthButton(string text)
	{
		var button = new Button();
		button.Text = text;
		button.CustomMinimumSize = new Vector2(180, 60);

		var font = GD.Load<FontFile>("res://Resources/Fonts/VCR_OSD_MONO_1.001.ttf");
		button.AddThemeFontOverride("font", font);
		button.AddThemeFontSizeOverride("font_size", 35);
		button.AddThemeColorOverride("font_color", new Color(0, 0, 0, 1));

		var normalStyle = GD.Load<StyleBox>("res://Resources/ButtonThemes/menubuttonnorm.tres");
		var hoverStyle = GD.Load<StyleBox>("res://Resources/ButtonThemes/menubuttonhover.tres");
		button.AddThemeStyleboxOverride("normal", normalStyle);
		button.AddThemeStyleboxOverride("hover", hoverStyle);
		button.AddThemeStyleboxOverride("focus", normalStyle);

		return button;
	}

	private Button CreateSmallButton(string text)
	{
		var button = new Button();
		button.Text = text;
		button.CustomMinimumSize = new Vector2(120, 35);

		var font = GD.Load<FontFile>("res://Resources/Fonts/VCR_OSD_MONO_1.001.ttf");
		button.AddThemeFontOverride("font", font);
		button.AddThemeFontSizeOverride("font_size", 20);
		button.AddThemeColorOverride("font_color", new Color(0, 0, 0, 1));

		var normalStyle = GD.Load<StyleBox>("res://Resources/ButtonThemes/menubuttonnorm.tres");
		var hoverStyle = GD.Load<StyleBox>("res://Resources/ButtonThemes/menubuttonhover.tres");
		button.AddThemeStyleboxOverride("normal", normalStyle);
		button.AddThemeStyleboxOverride("hover", hoverStyle);
		button.AddThemeStyleboxOverride("focus", normalStyle);

		return button;
	}

	private void AddStatLabel(GridContainer container, string text)
	{
		var label = new Label();
		label.Text = text;
		label.AddThemeColorOverride("font_color", new Color(0, 0, 0, 1));
		label.HorizontalAlignment = HorizontalAlignment.Center;

		var font = GD.Load<FontFile>("res://Resources/Fonts/VCR_OSD_MONO_1.001.ttf");
		label.AddThemeFontOverride("font", font);
		label.AddThemeFontSizeOverride("font_size", 25);

		container.AddChild(label);
	}
}
