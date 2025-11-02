using Godot;
using System;

// this script will be 
public partial class DragableObjectControl : Control {
	
	// if false, then should block all drag attempts
	public static bool allowDrag = true;
	public static Window objectControlWindow;
	Sprite2D sprite;
	Vector2I atlasCords;
	int selection;
	static Vector2I visibleObjectScaling = new Vector2I(3, 3);
	Label priceLabel;
	PanelContainer vboxPanel;
	public DragableObjectControl(ImageTexture texture, Vector2I atlasCords, int posX, int posY, int selection, Label priceLabel, PanelContainer vboxPanel)
	{


		sprite = new Sprite2D();
		// get texture
		sprite.Texture = texture;
		sprite.Scale = visibleObjectScaling;
		sprite.Set(Sprite2D.PropertyName.Position, new Vector2I(posX, posY));
		AddChild(sprite);
		this.atlasCords = atlasCords;
		this.selection = selection;
		this.vboxPanel = vboxPanel;
		this.priceLabel = priceLabel;
	}

	public override void _Ready() {
		CustomMinimumSize = new Vector2(171, 171); // 256,256
		
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton buttonEvent && buttonEvent.ButtonIndex == MouseButton.Left && buttonEvent.Pressed
			&& allowDrag)
		{
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			if (manager.currLevel.StepCount != 0) return; // Don't allow placement while we are stepping

			// We want to spawn a new draggable object and pass in all the correct values
			var draggable = new DraggableObject(Position - GetGlobalMousePosition(), sprite, atlasCords);
			SubViewport subView = GetTree().Root.GetNode("/root/Node2D/MainVBox/TerminalLevelSplit/VBoxContainer/LevelContainer/SubViewport") as SubViewport;
			subView.AddChild(draggable);
			// spawn terminal perhap?

			GD.Print("Created new dragable:", draggable);
			manager.objectToPlace = atlasCords;
			manager.placingObject = 1;
			manager.currSlection = selection;
		}
		else if (@event is InputEventMouseButton buttonEvent2 && buttonEvent2.ButtonIndex == MouseButton.Right && GetTree().CurrentScene.SceneFilePath == "res://Scenes/LevelCreator/level_creator.tscn")
		{
			DragableObjectMenu(buttonEvent2);
		}
		else
		{
			base._Input(@event); // pass downward
		}
	}
	public void DragableObjectMenu(InputEventMouseButton buttonEvent2)
	{
        PopupMenu popup = new PopupMenu();
		AddChild(popup);
		popup.AddItem("Change Price");
		popup.AddItem("Remove Item");
		Vector2 mousePos = buttonEvent2.GlobalPosition;
		popup.Position = new Vector2I((int)mousePos.X, (int)mousePos.Y);
		popup.IdPressed += (id) => {
			string itemText = popup.GetItemText((int) id);
			switch (itemText)
			{
				case "Change Price":
					GD.Print("Change Price Selected");
					Window priceChangeWindow = new Window();
					priceChangeWindow.Size = new Vector2I(400, 300);
				

					// Get the viewport size (the visible game window)
					Vector2 viewportSize = GetViewport().GetVisibleRect().Size;

					// Center = viewport midpoint minus half of the window size
					priceChangeWindow.Position = new Vector2I(
						(int)((viewportSize.X - priceChangeWindow.Size.X) / 2),
						(int)((viewportSize.Y - priceChangeWindow.Size.Y) / 2)
					);
					VBoxContainer vbox = new VBoxContainer();
					vbox.CustomMinimumSize = new Vector2I(300, 200);
					vbox.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
					vbox.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
					TextEdit priceBox = new TextEdit();
					priceBox.CustomMinimumSize = new Vector2I(200, 200);
					AddChild(priceChangeWindow);
					priceChangeWindow.AddChild(vbox);
					vbox.AddChild(priceBox);
					Button changeButton = new Button();
					changeButton.Pressed += () =>
					{
						priceLabel.Text = priceBox.Text;
					};
					vbox.AddChild(changeButton);
					priceChangeWindow.Visible = true;
					
					break;

				case "Remove Item":
					GD.Print("Remove option selected");
					var picker = GetTree().Root.GetNode<ObjectPicker>("/root/Node2D/MainVBox/PanelContainer/HBoxContainer/PanelContainer/ScrollContainer/ObjectPicker");
					picker.RemoveChild(vboxPanel);
					break;

				case "Delete":
					GD.Print("Delete option selected");
					// Delete logic here
					break;
			}
		};
		popup.Popup();
    }
}
