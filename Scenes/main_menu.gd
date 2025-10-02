extends Node2D

var level: int = 1

func _ready() -> void:
	$SettingsMenu/VBoxContainer/VBoxContainer2/Fullscreen.button_pressed = true if DisplayServer.window_get_mode() == DisplayServer.WINDOW_MODE_EXCLUSIVE_FULLSCREEN else false
	$SettingsMenu/VBoxContainer/VBoxContainer2/MainVolSlider.value = db_to_linear(AudioServer.get_bus_index("Master"))
	

func _on_new_game_pressed() -> void:
	get_tree().change_scene_to_file("res://Testing/test_level_ui.tscn")


func _on_level_select_pressed() -> void:
	pass # Replace with function body.


func _on_settings_pressed() -> void:
	$MainMenu.visible = false
	$SettingsMenu.visible = true


func _on_quit_pressed() -> void:
		get_tree().quit()


func _on_back_pressed() -> void:
	$SettingsMenu.visible = false
	$MainMenu.visible = true


func _on_fullscreen_toggled(toggled_on: bool) -> void:
	if toggled_on:
		DisplayServer.window_set_mode(DisplayServer.WINDOW_MODE_EXCLUSIVE_FULLSCREEN)
	else:
		DisplayServer.window_set_mode(DisplayServer.WINDOW_MODE_MAXIMIZED)

func _on_main_vol_slider_value_changed(value: float) -> void:
	AudioServer.set_bus_volume_linear(AudioServer.get_bus_index("Master"), value)
