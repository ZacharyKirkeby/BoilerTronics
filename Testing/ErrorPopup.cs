using Godot;
using System;

public partial class ErrorPopup : Node
{
	private Node2D _errorIcon;

	public override void _Ready()
	{
		//will point out location of issue on ui
		var scene = (PackedScene)ResourceLoader.Load("res://Resources/ErrorNotice.tscn");
		_errorIcon = scene.Instantiate<Node2D>();

		AddChild(_errorIcon);
		_errorIcon.Position = new Vector2(1000, 600);

		//will open specific error popup, currently an example
		var packedErrorScene = ResourceLoader.Load<PackedScene>("Resources/ClawRailError.tscn");
		var instance = packedErrorScene.Instantiate();
		GetTree().CurrentScene.AddChild(instance);
	}
}
