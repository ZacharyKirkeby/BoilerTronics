using Godot;
using System;

public partial class ButtonTray : Godot.CanvasLayer
{
		public override void _Ready()
	{
	}
	private void _on_toggle_button_pressed() {
		GetNode<AnimationPlayer>("VerticalButtonTray/AnimationPlayer").Play("tray_close");
		GetNode<Button>("OpenButton").Visible = true;
	}
	private void _on_open_button_pressed() {
		GetNode<AnimationPlayer>("VerticalButtonTray/AnimationPlayer").Play("tray_open");
		GetNode<Button>("OpenButton").Visible = false;
	}
}
