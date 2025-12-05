using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Objects;
using BoilerTronicsObjects.Data;
using BoilerTronicsObjects.Interfaces;

// this script will be 
public partial class DragableObjectControl : Control {
	
	// if false, then should block all drag attempts
	public static bool allowDrag = true;
	public static Window objectControlWindow;
	Sprite2D sprite;
	Vector2I atlasCords;
	int sourceID;
	static Vector2I visibleObjectScaling = new Vector2I(3, 3);
	Label priceLabel;
	PanelContainer vboxPanel;
	Button submitButton;
	Window priceChangeWindow;
	LineEdit priceBox;
	PlaceableBig.Direction big_dir;
	Direction dir;
	Quality Q;

	int sel;
	// int itemNumber;

	public DragableObjectControl(ImageTexture texture, Vector2I atlasCords, int sel, int sourceID, int posX, int posY, Label priceLabel, PanelContainer vboxPanel)
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
		this.sourceID = sourceID;
		this.sel = sel;
		big_dir = PlaceableBig.Direction.UP; // Up by default
		dir = Direction.UP;
		// this.itemNumber = itemNumber;
	}

	public override void _Ready() {
		CustomMinimumSize = new Vector2(171, 171); // 256,256
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton buttonEvent && buttonEvent.ButtonIndex == MouseButton.Left && buttonEvent.Pressed && allowDrag)
		{
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

			if (manager.currLevel.StepCount != 0) return; // Don't allow placement while we are stepping

			// clear selection status
			manager.ClearSelectingObject();
			
			// start placing object
			manager.placingObject = 1;

			// We now need to make the object so that we place it :D
			PlaceableObject obj = ObjectFactory.CreateObject(new Vector2I(-1, -1), sourceID, atlasCords, Q, dir);
			GD.Print(obj);

			if (obj is PlaceableBig bObj) bObj.SetDir(big_dir);

			// We want to spawn a new draggable object and pass in all the correct values
			var draggable = new DraggableObject(Position - GetGlobalMousePosition(), sprite, obj);

			SubViewport subView = GetTree().Root.GetNode("/root/Node2D/MainVBox/TerminalLevelSplit/VBoxContainer/LevelContainer/SubViewport") as SubViewport;
			subView.AddChild(draggable);

			manager.currSlection = sel;
			manager.ClearSelectingObject();

			GD.Print("Curr Sel: ", manager.currSlection);

			// spawn terminal perhap?
			// set layer to be semi transparent if not being placed on
			if (manager.currSlection == 1)
			{
				manager.layerClaw.Modulate = new Color(1, 1, 1, 0.3f);
				manager.layerFactory.Modulate = new Color(1, 1, 1, 0.3f);
				manager.layerFloor.Modulate = new Color(1, 1, 1, 0.3f);
				manager.layerRail.Modulate = new Color(1, 1, 1, 0.3f);
			} else if (manager.currSlection == 3)
			{
				manager.layerMovement.Modulate = new Color(1, 1, 1, 0.3f);
				manager.layerFactory.Modulate = new Color(1, 1, 1, 0.3f);
				manager.layerFloor.Modulate = new Color(1, 1, 1, 0.3f);
			} else if (manager.currSlection == 2)
			{
				manager.layerMovement.Modulate = new Color(1, 1, 1, 0.3f);
				manager.layerClaw.Modulate = new Color(1, 1, 1, 0.3f);
				manager.layerRail.Modulate = new Color(1, 1, 1, 0.3f);
			}

			GD.Print("Created new dragable:", draggable);
		}
		// else if (@event is InputEventMouseButton buttonEvent2 && buttonEvent2.ButtonIndex == MouseButton.Right && GetTree().CurrentScene.SceneFilePath == "res://Scenes/LevelCreator/level_creator.tscn")
		else if (@event is InputEventMouseButton buttonEvent2 && buttonEvent2.ButtonIndex == MouseButton.Right && buttonEvent2.Pressed)
		{
			// TODO: make the placable big work with the direction enum for the general palcable object as well

			// Change our direction for big objects
			switch (big_dir) {
				case PlaceableBig.Direction.UP:
					big_dir = PlaceableBig.Direction.DOWN;
					break;
				case PlaceableBig.Direction.DOWN:
					big_dir = PlaceableBig.Direction.LEFT;
					break;
				case PlaceableBig.Direction.LEFT:
					big_dir = PlaceableBig.Direction.RIGHT;
					break;
				case PlaceableBig.Direction.RIGHT:
					big_dir = PlaceableBig.Direction.UP;
					break;
			}

			// Try to get data
			List<PlaceableBigData> data = ObjectFactory.GetBigObjectTileMap(BoilerTronicsData.objectMap[BoilerTronicsData.hashCoords(this.sourceID, this.atlasCords)], big_dir);

			// If we can get data (it's a big object)
			if (data != null) {
				// Then we can update the texture
				ImageTexture texture = PlaceableBig.GetBigTexture(data) as ImageTexture;
				sprite.Texture = texture;
				return;
			}

			// Deal with normal direction
			switch(dir) {
				case Direction.UP:
					dir = Direction.RIGHT;
					break;
				case Direction.RIGHT:
					dir = Direction.DOWN;
					break;
				case Direction.DOWN:
					dir = Direction.LEFT;
					break;
				case Direction.LEFT:
					dir = Direction.UP;
					break;
			}

			Texture2D T = ObjectFactory.GetTexture(BoilerTronicsData.objectMap[BoilerTronicsData.hashCoords(this.sourceID, this.atlasCords)], Q, dir) as Texture2D;

			if (T != null) {
				sprite.Texture = T;
			}
		}
		else if (@event is InputEventMouseButton buttonEvent3 && buttonEvent3.ButtonIndex == MouseButton.Middle && buttonEvent3.Pressed) {
			// This case will be how the user changes the quality of the sprite, allowing them to press space and rotate through the quality of the object
			// We should make sure the ite mwe are tracking has quality
			// If so then we need to rotate the quality var internally (this will be later passed into the object factory)
			// And then we need to update the sprite to the new spite for the given quality
			GD.Print("Try to rotate quality");

			switch(Q) {
				case Quality.LOW_QUALITY:
					Q = Quality.MID_QUALITY;
					break;
				case Quality.MID_QUALITY:
					Q = Quality.HIGH_QUALITY;
					break;
				case Quality.HIGH_QUALITY:
					Q = Quality.LOW_QUALITY;
					break;
			}

			Texture2D T = ObjectFactory.GetTexture(BoilerTronicsData.objectMap[BoilerTronicsData.hashCoords(this.sourceID, this.atlasCords)], Q, dir) as Texture2D;

			GD.Print("Quality: ", Q, " Texture: ", T);

			if (T != null) {
				sprite.Texture = T;
			}
		}
		else
		{
			GD.Print(@event);
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
