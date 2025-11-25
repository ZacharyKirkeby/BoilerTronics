using Godot;
using System;
using System.Numerics;

public partial class NewLevelSelect : Node2D

{
	private int currLevel = 0;
	
	private void _on_button_pressed() {
		var cam = GetNode<Camera2D>("Camera2D");
		cam.PositionSmoothingEnabled = true;
		cam.PositionSmoothingSpeed = 1;
		cam.Position = new Vector2I(2880, 540);
		var but = GetNode<Button>("%Button");
		but.Position += new Godot.Vector2I(1920, 0);
		var ani = GetNode<AnimationPlayer>("%Animation");
		ani.Play("new_animation");
	}
}
