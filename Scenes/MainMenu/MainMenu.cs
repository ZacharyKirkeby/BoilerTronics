using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class MainMenu : Node2D
{
	private int level = 1;
	private FirebaseAuthManager _authManager;
	private FirestoreService _firestoreService;
	
	public override void _Ready()
	{
		InitializeServices();

		// Set fullscreen toggle
		var fullscreenButton = GetNode<Button>("SettingsMenu/VBoxContainer/Panel/VBoxContainer/VBoxContainer2/Fullscreen");
		fullscreenButton.ButtonPressed = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.ExclusiveFullscreen
			|| DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen;

		// Set volume slider
		BoilerTronicsSoundManager soundManager = BoilerTronicsSoundManager.SoundManager;
		var volSlider = GetNode<HSlider>("SettingsMenu/VBoxContainer/Panel/VBoxContainer/VBoxContainer2/MainVolSlider");
		volSlider.Value = soundManager.GetCurrentVolume();
		
	}

	private void InitializeServices()
	{
		// Get or create auth manager
		_authManager = FirebaseAuthManager.Instance;
		if (_authManager == null)
		{
			_authManager = new FirebaseAuthManager();
			AddChild(_authManager);
		}

		// Get or create firestore service
		_firestoreService = FirestoreService.Instance;
		if (_firestoreService == null)
		{
			_firestoreService = new FirestoreService();
			AddChild(_firestoreService);
		}
	}

	private void _on_new_game_pressed()
	{
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		
		// load the default level for level 0
		// manager.SetTargetLevelSave(0, -1);
		
		// load autosave
		// manager.SetTargetLevelSave(0, -2);
		GetTree().ChangeSceneToFile("res://Scenes/LevelUI/level_ui.tscn");
	}

	private void _on_level_select_pressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/LevelSelect/level_select.tscn");
	}

	private async Task<List<LevelData>> _get_levels()
	{
		ProfileMenuController profMan = ProfileMenuController.GlobalManager;
		UserData userDat = null;
		if (profMan.IsAuthenticated()) {
			FirestoreService _instance = FirestoreService.Instance;
			userDat = profMan.GetUserData();
			List<LevelData> levels = await _instance.GetUserLevelsAsync(userDat.Uuid);
			foreach (LevelData level in levels)
			{
				GD.Print(level);
			}
			return levels;
		}
		return null;
	}

	private async Task _on_level_creator_pressed()
	{
		GetNode<Control>("MainMenu").Visible = false;
		GetNode<Control>("LevelCreatorMenu").Visible = true;
		var vbox = GetNode<VBoxContainer>("%LevelVBox");
		foreach (Node child in vbox.GetChildren())
		{
			child.QueueFree();
		}
		List<LevelData> levels = await _get_levels();
		foreach (LevelData level in levels)
        {
            CreateRow(level.LevelName, level.CreatorName, level);
        }
	}
	
	public PanelContainer CreateRow(String levelName, String authorName, LevelData dat)
	{
		// ---- PanelContainer ----
		var panel = new PanelContainer
		{
			CustomMinimumSize = new Vector2(0, 150)
		};

		var style = new StyleBoxFlat
		{
			BgColor = new Color("937e56"),   // background color
			BorderColor = Colors.Black
		};

		style.CornerRadiusTopLeft = 10;
		style.CornerRadiusTopRight = 10;
		style.CornerRadiusBottomLeft = 10;
		style.CornerRadiusBottomRight = 10;

		style.BorderWidthLeft = 4;
		style.BorderWidthTop = 4;
		style.BorderWidthRight = 4;
		style.BorderWidthBottom = 4;

		panel.AddThemeStyleboxOverride("panel", style);

		// ---- HBoxContainer ----
		var hbox = new HBoxContainer();
		panel.AddChild(hbox);

		// Common font for labels
		// (replace with your actual font path)
		var font = ResourceLoader.Load<Font>("res://Resources/Fonts/VCR_OSD_MONO_1.001.ttf");

		// ---- Left Control (100 x 0) ----
		var controlLeft = new Control
		{
			CustomMinimumSize = new Vector2(100, 0)
		};
		hbox.AddChild(controlLeft);

		// ---- Label ----
		var label1 = new Label
		{
			Text = levelName
		};
		label1.AddThemeFontOverride("font", font);
		label1.AddThemeColorOverride("font_color", Colors.Black);
		hbox.AddChild(label1);

		// ---- Label2 ----
		var label2 = new Label
		{
			Text = authorName
		};
		label2.AddThemeFontOverride("font", font);
		label2.AddThemeColorOverride("font_color", Colors.Black);
		hbox.AddChild(label2);

		// ---- Button ----
		var button = new Button
		{
			Text = "",
			// Horizontal: Shrink End + Expand
			SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd | Control.SizeFlags.Expand,
			// Vertical: Shrink Center
			SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
		};

		// Theme (replace with your theme path)
		button.Theme = ResourceLoader.Load<Theme>("res://Resources/ButtonThemes/buttontheme.tres");

		// Icon
		var icon = ResourceLoader.Load<Texture2D>("res://Resources/Icons/play.png");
		button.Icon = icon;
		button.AddThemeConstantOverride("icon_max_width", 100);
		button.Pressed += () => OnRowButtonPressed(dat);

		hbox.AddChild(button);

		// ---- Right Control (100 x 0) ----
		var controlRight = new Control
		{
			CustomMinimumSize = new Vector2(100, 0)
		};
		hbox.AddChild(controlRight);

		return panel;
	}
	
	private void OnRowButtonPressed(LevelData level)
	{
    GD.Print($"Button for level {level.LevelName} pressed");
    // TODO Open level creator with level loaded
	}

	private void _on_level_creator_button_pressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/LevelCreator/user_level_creator.tscn");
	}

	private void _on_profile_pressed() {
		GetNode<Control>("MainMenu").Visible = false;
		GetNode<Control>("ProfileMenu").Visible = true;
	}
	
	private void _on_leaderboard_pressed() {
		GetNode<Control>("MainMenu").Visible = false;
		GetNode<Control>("Leaderboard").Visible = true;
	}
	
	private void _on_back_pressed()
	{
		GetNode<Control>("SettingsMenu").Visible = false;
		GetNode<Control>("ProfileMenu").Visible = false;
		GetNode<Control>("Leaderboard").Visible = false;
		GetNode<Control>("LevelCreatorMenu").Visible = false;
		GetNode<Control>("MainMenu").Visible = true;
	}

	private void _on_quit_pressed()
	{
		GetTree().Quit();
	}
	
	private void _on_settings_pressed() {
		GetNode<Control>("MainMenu").Visible = false;
		GetNode<Control>("SettingsMenu").Visible = true;
	}

	private void _on_fullscreen_toggled(bool toggledOn)
	{
		if (toggledOn)
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
		else
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Maximized);
	}

	private void _on_main_vol_slider_value_changed(float val)
	{
		BoilerTronicsSoundManager soundManager = BoilerTronicsSoundManager.SoundManager;
		soundManager.SetCurrentVolume(val);
	}

	private void _on_mute_pressed()
	{
		//move slider to 0
		var slider = GetNode<HSlider>("SettingsMenu/VBoxContainer/Panel/VBoxContainer/VBoxContainer2/MainVolSlider");
		slider.Value = 0;

		//actually make volume 0
		_on_main_vol_slider_value_changed(0);
	}
	private void _on_open_pdf_pressed()
	{
		string pdfPath = "res://docs/AssemblyManual.pdf";
		if (FileAccess.FileExists(pdfPath))
			OS.ShellOpen(ProjectSettings.GlobalizePath(pdfPath));
		else
			GD.PrintErr($"PDF not found: {pdfPath}");
	}
}
