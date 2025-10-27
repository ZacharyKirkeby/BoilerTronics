using Godot;
using System;
using Parsing;

public partial class ErrorHandler : Node2D {

	// Types of errors
	// Negative means error
	// Digit in the 100s spot is the type of error
	// Other digits are the sub-category of error
	public enum ErrorType {
		ClawErrorGeneric = -100,
		ClawRail = -101,
		ClawOutOfBounds = -102,
		ClawCollision = -103,
		ClawInventory = -104,
		TerminalErrorGneric = -200,
		TerminalInvalidCommand = -201,
		TerminalInvalidArg = -202,
		TerminalInvalidCommandUse = -203,
		// TODO: Make more error codes
	}

	// We need to track the scene so that we can remove it later
	private Node errorSceneInstance;
	private bool ErrorPresent;

	public ErrorHandler() {

	}

	public bool HasError() {
		return ErrorPresent;
	}

	public void ClearError() {
		ErrorPresent = false;
	}

	// Function to throw an error from the parser
	public void OnParserErrorRaised(int lineNumber, string message, string editorName)
	{
		// TODO: Rework this to work with the new error handling system ? (see if this is doable)
		var codeEditors = GetTree().GetNodesInGroup("CodeTerminals");
		GD.Print(codeEditors);

		foreach (CodeEdit editor in codeEditors)
		{
			if (editor.Name == editorName)
			{
				var existing = editor.GetNodeOrNull<Label>("ErrorLabel");
				if (existing != null)
				{
					existing.QueueFree();
				}

				// Create error label
				Label errorLabel = new Label
				{
					Name = "ErrorLabel",
					Text = $"Line {lineNumber}: {message}",
					HorizontalAlignment = HorizontalAlignment.Left,
					Modulate = new Color(1, 0.3f, 0.3f),
					AutowrapMode = TextServer.AutowrapMode.Word
				};

				// Add to bottom of CodeEdit
				editor.AddChild(errorLabel);
				errorLabel.AnchorLeft = 0;
				errorLabel.AnchorRight = 1;
				errorLabel.AnchorBottom = 1;
				errorLabel.AnchorTop = 1;
				errorLabel.OffsetBottom = -4;

				errorLabel.Position = new Vector2(0, editor.Size.Y - 20);

				// Highlight error line
				editor.HighlightLine(lineNumber, new Color(1, 0, 0, 0.25f));

				//TODO - delete
				GD.Print($"[ParserError] {editorName}: Line {lineNumber} -> {message}");
				break;
			}
		}

		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		manager.PlaySound("error", 10);

		ShowErrorNotice(new Vector2I(0,0)); // Add the '!' icon | TODO: throw a handle error based on the actual error

		ErrorPresent = true;
	}

	//displays error (specific error popup, location of error on level ui, specific code terminal highlighted red)
	public void handleError(ErrorType type, CodeEdit E, Vector2 Pos) {
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		manager.StopSound();
		manager.PlaySound("error", 10);

		if (errorSceneInstance != null) return; // Already displaying error
		manager.currLevel.HaultObjects();

		//open error notice (exclamation mark) at coords of error
		//TODO: add this to camera2D in actual level window
		/*if(errorNoticeIcon == null) {
		  var scene = (PackedScene)ResourceLoader.Load("res://Resources/ErrorNotice.tscn");
		  errorNoticeIcon = scene.Instantiate<Node2D>();
		  AddChild(errorNoticeIcon);
		  }
		//TODO: replace example coords with actual (make dynamic)
		errorNoticeIcon.Position = errorCoords;*/

		ShowErrorNotice(Pos);

		if(E != null) {
			E.HighlightLine(E.getLastHighlighted(), new Color(1, 0, 0, 0.3f));
		}

		PackedScene packedErrorScene = null;

		//dynamic error popups based on type of error
		switch (type)
		{
			case ErrorType.ClawRail:
				packedErrorScene = ResourceLoader.Load<PackedScene>("res://Scenes/ErrorWindows/ClawRailError.tscn");
				break;
			case ErrorType.ClawOutOfBounds:
				packedErrorScene = ResourceLoader.Load<PackedScene>("res://Scenes/ErrorWindows/ClawOutOfBoundsError.tscn");
				break;
			case ErrorType.ClawCollision:
				packedErrorScene = ResourceLoader.Load<PackedScene>("res://Scenes/ErrorWindows/ClawCollisionError.tscn");
				break;
			case ErrorType.ClawInventory:
				packedErrorScene = ResourceLoader.Load<PackedScene>("res://Scenes/ErrorWindow/ClawInventoryError.tscn");
				break;
			default:
				return; // Invalid error code
		}

		errorSceneInstance = packedErrorScene.Instantiate();

		//actually display error notice
		if(packedErrorScene != null) {
			if (errorSceneInstance is AcceptDialog dialog) {
				dialog.Connect("confirmed", new Callable(this, nameof(OnErrorDialogClosed)));
				dialog.Connect("canceled", new Callable(this, nameof(OnErrorDialogClosed)));
				dialog.Connect("close_requested", new Callable(this, nameof(OnErrorDialogClosed)));
			}

			GetTree().CurrentScene.AddChild(errorSceneInstance);
		}

		ErrorPresent = true;
		manager.PlaySound("error", 10);
	}


	//be able to call for error popup from this script
	public void ShowErrorNotice(Vector2 position) {
		var camera = GetTree().CurrentScene.GetNode<BoilerTronicsObjects.GameCamera.Camera2d>("MainVBox/TerminalLevelSplit/VBoxContainer/LevelContainer/SubViewport/Node2D/Camera2D");
		camera.SpawnErrorSprite(position);
	}

	//be able to call for error popup removal from this script
	public void ClearErrorNotice() {
		var camera = GetTree().CurrentScene.GetNode<BoilerTronicsObjects.GameCamera.Camera2d>("MainVBox/TerminalLevelSplit/VBoxContainer/LevelContainer/SubViewport/Node2D/Camera2D");
		camera.RemoveErrorSprite();
	}

	public void OnErrorDialogClosed() {
		GD.Print("Error dialog closed — clearing reference");

		if (errorSceneInstance != null)
		{
			errorSceneInstance.QueueFree();
			errorSceneInstance = null;
		}
	}

	public void RemoveErrorScene() {
		if(IsInstanceValid(errorSceneInstance)) {
			errorSceneInstance.QueueFree();
			errorSceneInstance = null;
			GD.Print("Error scene removed.");
		}
		else {
			GD.Print("Error scene failed to removed.");
		}
	}
}
