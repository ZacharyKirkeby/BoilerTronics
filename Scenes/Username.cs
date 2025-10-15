using Godot;
using System;

public partial class Username : Label
{
	public override void _Ready() {
		string username = System.Environment.UserName;
		Text = $"{username}";
	}
}
