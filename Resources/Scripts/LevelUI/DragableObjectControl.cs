using Godot;
using System;
using System.Linq;

// this script will be 
public partial class DragableObjectControl : Control {
	
	// if false, then should block all drag attempts
	public static bool allowDrag = true;
	public static Window objectControlWindow;
	Sprite2D sprite;
	Vector2I atlasCords;
	static Vector2I visibleObjectScaling = new Vector2I(3, 3);
	Label priceLabel;
	PanelContainer vboxPanel;
	Button submitButton;
	Window priceChangeWindow;
	LineEdit priceBox;
	// int itemNumber;

	public DragableObjectControl(ImageTexture texture, Vector2I atlasCords, int posX, int posY, Label priceLabel, PanelContainer vboxPanel)
	{
		sprite = new Sprite2D();
		// get texture
		sprite.Texture = texture;
		sprite.Scale = visibleObjectScaling;
		sprite.Set(Sprite2D.PropertyName.Position, new Vector2I(posX, posY));
		AddChild(sprite);
		this.atlasCords = atlasCords;
		this.vboxPanel = vboxPanel;
		this.priceLabel = priceLabel;
		// this.itemNumber = itemNumber;
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
		
		Vector2 mousePos = buttonEvent2.GlobalPosition;
		popup.Position = new Vector2I((int)mousePos.X, (int)mousePos.Y);
		popup.IdPressed += (id) =>
		{
			string itemText = popup.GetItemText((int)id);
			switch (itemText)
			{
				case "Change Price":
					GD.Print("Change Price Selected");
					priceChangeWindow = GetTree().Root.GetNode<Window>("/root/Node2D/PriceChangeWindow");
					priceChangeWindow.Visible = true;
					submitButton = new Button();
					submitButton.Text = "Submit";
					submitButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
					submitButton.SizeFlagsVertical = SizeFlags.ShrinkCenter;
					priceChangeWindow.GetNode<VBoxContainer>("PriceChangeVbox").AddChild(submitButton);
					priceBox = priceChangeWindow.GetNode<LineEdit>("PriceChangeVbox/PriceBox");
					priceBox.GrabFocus();

					submitButton.Pressed += () => OnSubmitPrice();
					break;
			}
		};
		popup.Popup();
	}

	private void OnSubmitPrice() 
	{
		/*
		 * Not sure if we need this, prices should be staic and defined in code
		vboxPanel.GetChild<VBoxContainer>(0).GetChild<Label>(2).Text = "Price: $" + priceBox.Text;
		GD.Print(selection);
		if (selection == 1)
		{
			ObjectPicker.MovementItemPrices[itemNumber] = Int32.Parse(priceBox.Text);
			GD.Print(ObjectPicker.MovementItemPrices[itemNumber]);
		}
		else if (selection == 2)
		{
			ObjectPicker.FactoryItemPrices[itemNumber] = Int32.Parse(priceBox.Text);
			GD.Print(ObjectPicker.FactoryItemPrices[itemNumber]);
		}
		else if (selection == 3)
		{
			ObjectPicker.ClawItemPrices[itemNumber] = Int32.Parse(priceBox.Text);
			GD.Print(ObjectPicker.ClawItemPrices[itemNumber]);
		}
		priceBox.Text = "";
		priceChangeWindow.Visible = false;
		*/
		
		submitButton.QueueFree();
	}
}
