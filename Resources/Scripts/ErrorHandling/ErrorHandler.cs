using Godot;
using System;
using Parsing;
using Microsoft.VisualBasic.FileIO;

public partial class ErrorHandler : Node2D
{
	// Types of errors
	// Negative means error
	// Digit in the 100s spot is the type of error
	// Other digits are the sub-category of error
	public enum ErrorType
	{
		ClawErrorGeneric = -100,
		ClawRail = -101,
		ClawOutOfBounds = -102,
		ClawCollision = -103,
		ClawInventory = -104,
		TerminalErrorGeneric = -200,
		TerminalInvalidCommand = -201,
		TerminalInvalidArg = -202,
		TerminalInvalidCommandUse = -203,
		// TODO: Make more error codes
	}

	// We need to track the scene so that we can remove it later
	private Node errorSceneInstance;
	private bool ErrorPresent;

	public ErrorHandler()
	{
	}

	public bool HasError()
	{
		return ErrorPresent;
	}

	public void ClearError()
	{
		ErrorPresent = false;
		ClearErrorNotice();
	}

	// Function to throw an error from the parser (runtime errors during execution)
	public void OnParserErrorRaised(int lineNumber, string message, string editorName)
	{
		var codeEditors = GetTree().GetNodesInGroup("CodeTerminals");
		GD.Print($"[ParserError] Looking for editor: {editorName}");

		foreach (CodeEdit editor in codeEditors)
		{
			if (editor.Name == editorName)
			{
				// Remove any existing runtime error label
				var existingLabels = editor.GetChildren();
				foreach (Node child in existingLabels)
				{
					if (child is Label label && label.Name == "RuntimeErrorLabel")
					{
						label.QueueFree();
						label.Free();
					}
				}

				// Create runtime error label (distinct from syntax error label)
				Label errorLabel = new Label
				{
					Name = "RuntimeErrorLabel",  // Different name to avoid conflict
					Text = $"Runtime Error - Line {lineNumber + 1}: {message}",
					HorizontalAlignment = HorizontalAlignment.Left,
					Modulate = new Color(1, 0.2f, 0.2f),  // Slightly different color
					AutowrapMode = TextServer.AutowrapMode.Word
				};

				// Add to bottom of CodeEdit (slightly offset from syntax errors)
				editor.AddChild(errorLabel);
				errorLabel.AnchorLeft = 0;
				errorLabel.AnchorRight = 1;
				errorLabel.AnchorBottom = 1;
				errorLabel.AnchorTop = 1;
				errorLabel.OffsetBottom = -24;  // Different offset than syntax errors

				errorLabel.Position = new Vector2(0, editor.Size.Y - 40);

				// Highlight error line (uses CodeEdit's built-in method)
				editor.HighlightLine(lineNumber, new Color(1, 0, 0, 0.25f), true);

				GD.Print($"[ParserError] {editorName}: Line {lineNumber + 1} -> {message}");
				break;
			}
		}

		ShowErrorNotice(new Vector2I(0, 0)); // Add the '!' icon

		ErrorPresent = true;
	}

	// Displays error (specific error popup, location of error on level ui, specific code terminal highlighted red)
	public void handleError(ErrorType type, CodeEdit E, Vector2 Pos)
	{
		if (errorSceneInstance != null) return; // Already displaying error

		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		manager.currLevel.HaultObjects();

		ShowErrorNotice(Pos);

		if (E != null)
		{
			E.HighlightLine(E.getLastHighlighted(), new Color(1, 0, 0, 0.3f));
		}

		PackedScene packedErrorScene = null;

		// Dynamic error popups based on type of error
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

		if (packedErrorScene == null) return;

		errorSceneInstance = packedErrorScene.Instantiate();

		// Actually display error notice
		if (errorSceneInstance is AcceptDialog dialog)
		{
			dialog.Connect("confirmed", new Callable(this, nameof(OnErrorDialogClosed)));
			dialog.Connect("canceled", new Callable(this, nameof(OnErrorDialogClosed)));
			dialog.Connect("close_requested", new Callable(this, nameof(OnErrorDialogClosed)));
		}

		GetTree().CurrentScene.AddChild(errorSceneInstance);

		ErrorPresent = true;
	}

	// Be able to call for error popup from this script
	public void ShowErrorNotice(Vector2 position)
	{
		var camera = GetTree().CurrentScene.GetNode<BoilerTronicsObjects.GameCamera.Camera2d>("MainVBox/TerminalLevelSplit/VBoxContainer/LevelContainer/SubViewport/Node2D/Camera2D");
		camera.SpawnErrorSprite(position);
	}

	// Be able to call for error popup removal from this script
	public void ClearErrorNotice()
	{
		var camera = GetTree().CurrentScene.GetNode<BoilerTronicsObjects.GameCamera.Camera2d>("MainVBox/TerminalLevelSplit/VBoxContainer/LevelContainer/SubViewport/Node2D/Camera2D");
		camera.RemoveErrorSprite();
	}

	public void OnErrorDialogClosed()
	{
		GD.Print("Error dialog closed — clearing reference");

		if (errorSceneInstance != null)
		{
			errorSceneInstance.QueueFree();
			errorSceneInstance = null;
		}

		ClearRuntimeErrors();
	}

	public void RemoveErrorScene()
	{
		if (IsInstanceValid(errorSceneInstance))
		{
			errorSceneInstance.QueueFree();
			errorSceneInstance = null;
			GD.Print("Error scene removed.");
		}
		else
		{
			GD.Print("Error scene failed to remove.");
		}

		ClearRuntimeErrors();
	}

	// Clear all runtime error labels from terminals
	public void ClearRuntimeErrors()
	{
		var codeEditors = GetTree().GetNodesInGroup("CodeTerminals");
		foreach (CodeEdit editor in codeEditors)
		{
			var existing = editor.GetNodeOrNull<Label>("RuntimeErrorLabel");
			if (existing != null)
			{
				existing.QueueFree();
			}

			// Clear line highlighting (but keep syntax error highlights)
			// This preserves the syntax validation highlights
		}

		ClearErrorNotice();
		ErrorPresent = false;
	}

	// Check if any terminal has syntax errors (prevents execution)
	public bool HasSyntaxErrors(Terminals terminals)
	{
		var editors = terminals.GetAllEditors();
		foreach (CodeEdit editor in editors)
		{
			if (editor.HasErrors())
			{
				GD.PrintErr($"Terminal '{editor.Name}' has syntax errors");
				return true;
			}
		}
		return false;
	}

	// Clear both syntax and runtime errors
	public void ClearAllErrors(Terminals terminals)
	{
		ClearRuntimeErrors();
		terminals.ClearAllErrors();
	}
}