using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages the profile menu UI with friends system
/// File location: res://Scenes/MainMenu/ProfileMenuController.cs
/// </summary>
public partial class ProfileMenuController : Control
{
	private FirebaseAuthManager _authManager;
	private FirestoreService _firestoreService;
	private FriendsService _friendsService;
	private UserData _currentUserData;
	
	private HBoxContainer _mainContainer;
	private VBoxContainer _profileContainer;
	private VBoxContainer _friendsContainer;

	public override void _Ready()
	{
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

	public override void _ExitTree()
	{
		if (_authManager != null)
		{
			_authManager.AuthenticationChanged -= OnAuthenticationChanged;
		}
	}

	private void BuildLayout()
	{
		// Clear existing children
		foreach (Node child in GetChildren())
		{
			child.QueueFree();
		}

		// Create main horizontal container
		_mainContainer = new HBoxContainer();
		_mainContainer.AnchorRight = 1;
		_mainContainer.AnchorBottom = 1;
		_mainContainer.AddThemeConstantOverride("separation", 30);
		AddChild(_mainContainer);

		// Left side - Profile section
		var profilePanel = CreateStyledPanel(new Vector2(900, 650));
		_mainContainer.AddChild(profilePanel);

		_profileContainer = new VBoxContainer();
		_profileContainer.AnchorRight = 1;
		_profileContainer.AnchorBottom = 1;
		_profileContainer.GrowHorizontal = Control.GrowDirection.Both;
		_profileContainer.GrowVertical = Control.GrowDirection.Both;
		_profileContainer.AddThemeConstantOverride("separation", 80);
		profilePanel.AddChild(_profileContainer);

		// Add profile title
		var profileTitle = CreateTitleLabel("Profile");
		_profileContainer.AddChild(profileTitle);

		// Right side - Friends section
		var friendsPanel = CreateStyledPanel(new Vector2(500, 650));
		_mainContainer.AddChild(friendsPanel);

		_friendsContainer = new VBoxContainer();
		_friendsContainer.AnchorRight = 1;
		_friendsContainer.AnchorBottom = 1;
		_friendsContainer.GrowHorizontal = Control.GrowDirection.Both;
		_friendsContainer.GrowVertical = Control.GrowDirection.Both;
		_friendsContainer.AddThemeConstantOverride("separation", 20);
		friendsPanel.AddChild(_friendsContainer);

		// Add friends title
		var friendsTitle = CreateTitleLabel("Friends");
		_friendsContainer.AddChild(friendsTitle);

		// Back button at bottom
		var backButton = CreateBackButton();
		AddChild(backButton);
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

	private Button CreateBackButton()
	{
		var backButton = new Button();
		backButton.Text = "Back";
		backButton.CustomMinimumSize = new Vector2(600, 100);
		backButton.Position = new Vector2(660, 200);
		
		var font = GD.Load<FontFile>("res://Resources/Fonts/VCR_OSD_MONO_1.001.ttf");
		backButton.AddThemeFontOverride("font", font);
		backButton.AddThemeFontSizeOverride("font_size", 76);
		backButton.AddThemeColorOverride("font_color", new Color(0, 0, 0, 1));
		
		var normalStyle = GD.Load<StyleBox>("res://Resources/ButtonThemes/menubuttonnorm.tres");
		var hoverStyle = GD.Load<StyleBox>("res://Resources/ButtonThemes/menubuttonhover.tres");
		backButton.AddThemeStyleboxOverride("normal", normalStyle);
		backButton.AddThemeStyleboxOverride("hover", hoverStyle);
		backButton.AddThemeStyleboxOverride("focus", normalStyle);
		
		backButton.Pressed += () => {
			GetParent().GetNode<Control>("SettingsMenu").Visible = false;
			GetParent().GetNode<Control>("ProfileMenu").Visible = false;
			GetParent().GetNode<Control>("Leaderboard").Visible = false;
			GetParent().GetNode<Control>("MainMenu").Visible = true;
		};
		
		return backButton;
	}

	private async void LoadUserDataDeferred()
	{
		await LoadUserData();
		UpdateProfileMenu();
	}

	private async void OnAuthenticationChanged(bool isAuthenticated)
	{
		if (isAuthenticated)
		{
			await LoadUserData();
		}
		else
		{
			_currentUserData = null;
		}

		if (Visible)
		{
			UpdateProfileMenu();
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
		// Clear profile content (keep title)
		var profileChildren = _profileContainer.GetChildren();
		for (int i = profileChildren.Count - 1; i >= 1; i--)
		{
			profileChildren[i].QueueFree();
		}

		// Clear friends content (keep title)
		var friendsChildren = _friendsContainer.GetChildren();
		for (int i = friendsChildren.Count - 1; i >= 1; i--)
		{
			friendsChildren[i].QueueFree();
		}

		if (_authManager.IsAuthenticated && _currentUserData != null)
		{
			ShowAuthenticatedProfile();
			ShowFriendsPanel();
		}
		else
		{
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
		AddStatLabel(gridContainer, $"Achievements: {_currentUserData.AchievementsUnlocked.Count}");
		AddStatLabel(gridContainer, $"Easter Eggs: {_currentUserData.EasterEggsFound.Count}");

		_profileContainer.AddChild(gridContainer);

		// Logout button
		var logoutButton = new Button();
		logoutButton.CustomMinimumSize = new Vector2(400, 80);
		logoutButton.Text = "Logout";
		logoutButton.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		
		var font = GD.Load<FontFile>("res://Resources/Fonts/VCR_OSD_MONO_1.001.ttf");
		logoutButton.AddThemeFontOverride("font", font);
		logoutButton.AddThemeFontSizeOverride("font_size", 50);
		logoutButton.AddThemeColorOverride("font_color", new Color(0, 0, 0, 1));
		
		var normalStyle = GD.Load<StyleBox>("res://Resources/ButtonThemes/menubuttonnorm.tres");
		var hoverStyle = GD.Load<StyleBox>("res://Resources/ButtonThemes/menubuttonhover.tres");
		logoutButton.AddThemeStyleboxOverride("normal", normalStyle);
		logoutButton.AddThemeStyleboxOverride("hover", hoverStyle);
		logoutButton.AddThemeStyleboxOverride("focus", normalStyle);
		
		logoutButton.Pressed += () => _authManager.SignOut();
		_profileContainer.AddChild(logoutButton);
	}

	private async void ShowFriendsPanel()
	{
		// Search bar
		var searchContainer = new HBoxContainer();
		searchContainer.AddThemeConstantOverride("separation", 10);
		searchContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

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
		_friendsContainer.AddChild(searchResultContainer);

		// Friends list
		var friendsListLabel = CreateLabel("Your Friends:", 22);
		_friendsContainer.AddChild(friendsListLabel);

		var friendsScrollContainer = new ScrollContainer();
		friendsScrollContainer.CustomMinimumSize = new Vector2(0, 400);
		friendsScrollContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;

		var friendsList = new VBoxContainer();
		friendsList.AddThemeConstantOverride("separation", 10);
		friendsScrollContainer.AddChild(friendsList);
		_friendsContainer.AddChild(friendsScrollContainer);

		// Load friends
		if (_currentUserData.Friends.Count > 0)
		{
			var friendUsernames = await _friendsService.GetFriendUsernamesAsync(_currentUserData.Friends);
			foreach (var username in friendUsernames)
			{
				var friendLabel = CreateLabel($"• {username}", 20);
				friendsList.AddChild(friendLabel);
			}
		}
		else
		{
			var noFriendsLabel = CreateLabel("No friends yet", 18);
			noFriendsLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
			friendsList.AddChild(noFriendsLabel);
		}
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
			sendRequestButton.Pressed += async () => {
				await _friendsService.SendFriendRequestAsync(foundUser.Uuid, foundUser.Username);
				OnSearchPressed(searchInput); // Refresh
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