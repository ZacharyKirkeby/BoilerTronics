using Godot;
using System;
using System.Threading.Tasks;

public partial class ProfileMenuController : Control
{
	private FirebaseAuthManager _authManager;
	private FirestoreService _firestoreService;
	private UserData _currentUserData;
	
	private VBoxContainer _contentContainer;

	public override void _Ready()
	{
		_authManager = FirebaseAuthManager.Instance;
		_firestoreService = FirestoreService.Instance;

		try
		{
			_contentContainer = GetNode<VBoxContainer>("VBoxContainer/Panel/VBoxContainer");
		}
		catch (Exception ex)
		{
			return;
		}

		// Subscribe to auth changes
		if (_authManager != null)
		{
			_authManager.AuthenticationChanged += OnAuthenticationChanged;
			
			// If already authenticated, load user data immediately
			if (_authManager.IsAuthenticated)
			{
			    CallDeferred(nameof(LoadUserDataDeferred));
			}
		}

		// Initial update
		UpdateProfileMenu();
	}

    private async void LoadUserDataDeferred()
	{
		await LoadUserData();
		UpdateProfileMenu();
	}

	public override void _ExitTree()
	{
		if (_authManager != null)
		{
			_authManager.AuthenticationChanged -= OnAuthenticationChanged;
		}
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

		// Update UI if visible
		if (Visible)
		{
			UpdateProfileMenu();
		}
	}
	private async System.Threading.Tasks.Task LoadUserData()
	{
		// Load user data from Firestore
		_currentUserData = await _firestoreService.GetUserAsync(_authManager.UserId);
		
		// If user data doesn't exist, create it
		if (_currentUserData == null)
		{
			_currentUserData = UserData.CreateDefault(_authManager.UserId, _authManager.Email);
			await _firestoreService.CreateUserAsync(_authManager.UserId, _currentUserData);
		}
	}

	public void UpdateProfileMenu()
	{	
		// Clear existing content except the title
		var children = _contentContainer.GetChildren();
		
		for (int i = children.Count - 1; i >= 1; i--)
		{
			children[i].QueueFree();
		}

		if (_authManager.IsAuthenticated && _currentUserData != null)
		{
			ShowAuthenticatedProfile();
		}
		else
		{
			ShowLoginForm();
		}
	}

	private void ShowAuthenticatedProfile()
	{
		// Update title to show username
		var titleLabel = _contentContainer.GetChild<Label>(0);
		titleLabel.Text = _currentUserData.Username;

		// Add the stats grid
		var gridContainer = new GridContainer();
		gridContainer.Columns = 2;
		gridContainer.AddThemeConstantOverride("h_separation", 200);
		gridContainer.AddThemeConstantOverride("v_separation", 30);
		gridContainer.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		gridContainer.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
		
		// Add stat labels from Firestore data
		AddStatLabel(gridContainer, $"Hours Played: {_currentUserData.HoursPlayed:F1}");
		AddStatLabel(gridContainer, $"Friends: {_currentUserData.Friends.Count}");
		AddStatLabel(gridContainer, $"Achievements: {_currentUserData.AchievementsUnlocked.Count}");
		AddStatLabel(gridContainer, $"Easter Eggs: {_currentUserData.EasterEggsFound.Count}");

		_contentContainer.AddChild(gridContainer);

		// Add logout button
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
		
		logoutButton.Pressed += OnLogoutPressed;
		_contentContainer.AddChild(logoutButton);
	}

	private void ShowLoginForm()
	{
		var titleLabel = _contentContainer.GetChild<Label>(0);
		titleLabel.Text = "Login / Register";

		var formContainer = new VBoxContainer();
		formContainer.AddThemeConstantOverride("separation", 15);
		formContainer.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		formContainer.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;

		var emailLabel = CreateLabel("Email:", 25);
		var emailInput = CreateLineEdit("Enter email", 400, 50);
		emailInput.Name = "EmailInput";

		var passwordLabel = CreateLabel("Password:", 25);
		var passwordInput = CreateLineEdit("Enter password", 400, 50);
		passwordInput.Secret = true;
		passwordInput.Name = "PasswordInput";

		// Error message label
		var errorLabel = CreateLabel("", 18);
		errorLabel.Name = "ErrorLabel";
		errorLabel.AddThemeColorOverride("font_color", new Color(1, 0.3f, 0.3f));
		errorLabel.Visible = false;
		errorLabel.HorizontalAlignment = HorizontalAlignment.Center;

		// Button container
		var buttonContainer = new HBoxContainer();
		buttonContainer.AddThemeConstantOverride("separation", 20);
		buttonContainer.Alignment = BoxContainer.AlignmentMode.Center;

		// Login button
		var loginButton = CreateAuthButton("Login");
		loginButton.Pressed += () => OnLoginPressed(emailInput, passwordInput, errorLabel);

		// Register button
		var registerButton = CreateAuthButton("Register");
		registerButton.Pressed += () => OnRegisterPressed(emailInput, passwordInput, errorLabel);

		buttonContainer.AddChild(loginButton);
		buttonContainer.AddChild(registerButton);

		// Add all elements to form
		formContainer.AddChild(emailLabel);
		formContainer.AddChild(emailInput);
		formContainer.AddChild(passwordLabel);
		formContainer.AddChild(passwordInput);
		formContainer.AddChild(errorLabel);
		formContainer.AddChild(buttonContainer);

		_contentContainer.AddChild(formContainer);
	}

	private async void OnLoginPressed(LineEdit emailInput, LineEdit passwordInput, Label errorLabel)
	{
		string email = emailInput.Text.Trim();
		string password = passwordInput.Text;

		if (!ValidateInput(email, password, errorLabel))
			return;

		// Disable inputs during request
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

	private void OnLogoutPressed()
	{
		_authManager.SignOut();
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
		lineEdit.AddThemeFontSizeOverride("font_size", 20);
		
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