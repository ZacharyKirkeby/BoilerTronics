using Godot;
using System;

namespace BoilerTronicsObjects.GameCamera {
	public partial class Camera2d : Camera2D
	{
		Vector2 OldMousePosition;
		Vector2 NewMousePosition;
		
		public override void _Input(InputEvent @event)
			{
				// Note: these inputs will still "pass through" and Layer.cs could theoretically receive this
				// Be advised!
				// TODO: design a system for panning the view screen using right mouse button
				if (@event is InputEventMouseButton buttonEvent && buttonEvent.ButtonIndex == MouseButton.Right && buttonEvent.Pressed)
				{
					NewMousePosition = GetLocalMousePosition();
					
					this.Position += new Vector2(5, 5);
					
					OldMousePosition = GetLocalMousePosition();//GetViewport().GetMousePosition();
				}
				// base._Input(@event); // Calling this will pass down the input, we want to absorbe it
			}
	}
}
