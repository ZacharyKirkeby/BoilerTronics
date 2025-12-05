using Godot;
using System;

public partial class MainMenu : Node2D
{
	private int level = 11;
	private static FirebaseAuthManager _authManager;
	private static FirestoreService _firestoreService;
	
	public static MainMenu mainMainMenu;
	
	public static FirebaseAuthManager GetFirebaseAuthManager() {
		return _authManager;
	}
	public static FirestoreService GetFirestoreService() {
		return _firestoreService;
	}
	
	public override void _Ready()
	{
		mainMainMenu = this;
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

	public static void InitializeServices()
	{
		// Get or create auth manager
		_authManager = FirebaseAuthManager.Instance;
		if (_authManager == null)
		{
			_authManager = new FirebaseAuthManager();
			mainMainMenu.AddChild(_authManager);
		}

		// Get or create firestore service
		_firestoreService = FirestoreService.Instance;
		if (_firestoreService == null)
		{
			_firestoreService = new FirestoreService();
			mainMainMenu.AddChild(_firestoreService);
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
		GetTree().ChangeSceneToFile("res://Scenes/LevelSelect/new_level_select.tscn");
	}

	private void _on_settings_pressed()
	{
		GetNode<Control>("MainMenu").Visible = false;
		GetNode<Control>("SettingsMenu").Visible = true;
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
		GetNode<Control>("MainMenu").Visible = true;
		GetNode<Window>("ProfileMenu/VBoxContainer/Achievements/Achievements Menu").Visible = false;
		GetNode<Window>("ProfileMenu/VBoxContainer/Achievements/Easter Egg Menu").Visible = false;
	}

	private void _on_quit_pressed()
	{
		GetTree().Quit();
	}
	
	private void _on_documentation_pressed() {
		GetTree().ChangeSceneToFile("res://Scenes/LevelCreator/level_creator.tscn");
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
	
	private void _on_open_pdf_2_pressed()
	{
		string pdfPath = "res://docs/tempUserGuide.pdf";
		if (FileAccess.FileExists(pdfPath))
			OS.ShellOpen(ProjectSettings.GlobalizePath(pdfPath));
		else
			GD.PrintErr($"PDF not found: {pdfPath}");
	}

	public void _on_achievements_pressed() {
		GetNode<Window>("Achievements Menu").Visible = true;
	}
	
	private void _on_achievements_menu_close_requested() {
		GetNode<Window>("Achievements Menu").Visible = false;
	}
	
	public void _on_easter_eggs_pressed() {
		GetNode<Window>("Easter Egg Menu").Visible = true;
	}
	
	private void _on_easter_egg_menu_close_requested() {
		GetNode<Window>("Easter Egg Menu").Visible = false;
	}
}
