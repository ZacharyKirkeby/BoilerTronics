using Godot;
using System;

public partial class LevelSelect : Node2D
{
	private void _on_play_pressed() {
		GetNode<Window>("Window").Visible = true;
	}
	private void _on_back_pressed() {
		GetTree().ChangeSceneToFile("res://Scenes/main_menu.tscn");
	}
	private void _on_window_close_requested() {
		GetNode<Window>("Window").Visible = false;
	}
}
