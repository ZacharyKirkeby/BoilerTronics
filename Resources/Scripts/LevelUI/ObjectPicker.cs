using Godot;
using System;
using System.Numerics;
using System.Collections.Generic;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Objects;
using BoilerTronicsObjects.Objects.MovementLayerObjects;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
using BoilerTronicsObjects.Objects.ClawLayerObjects;
using System.Reflection.Metadata;
using BoilerTronicsObjects.Data;

// Doc string smth smth Fuck you Zach (No one touch this line)
public partial class ObjectPicker : HBoxContainer
{
	private class ItemInfo {
		public String name; // Name of the item that we want in the picker
		public int price; // Price of the item in the picker
		public int table; // ID of the table that contains the sprite
		public Vector2I atPos; // Atlas position of the sprite (this will be the root if it's a big object)
		public Vector2I offSet; // Amount we need to offset the sprite
		public bool big; // Bool to tell us if this is a big object or not (this will affect the way the texture is rendered)
		public bool quality; // Bool to tell us if this item has quality asocuiated with it
		public int sel;
		public String tooltip;

		public ItemInfo(string name, int price, int sel, int table, Vector2I atPos, Vector2I offSet, bool big, bool quality, String tooltip) {
			this.name = name;
			this.price = price;
			this.table = table;
			this.atPos = atPos;
			this.offSet = offSet;
			this.big = big;
			this.quality = quality;
			this.sel = sel;
			this.tooltip = tooltip;
		}
	}

	// sprite table id for each table
	static int MovementIndex = 1;
	static int FactoryIndex = 2;
	static int ClawIndex = 3;

	static ItemInfo[] MovementSection = {
		new ItemInfo("Vertical Conveyor", ConveyorObject.GetCostStatic(), 1, 2, new Vector2I(0,0), new Vector2I(85, 85), false, false, "Allow for you to move a rail (and whatever is on said rail) up or down with the ‘mov u’ and ‘mov d’ commands."),
		new ItemInfo("Horizontal Conveyor", ConveyorObject.GetCostStatic(), 1, 2, new Vector2I(0,1), new Vector2I(85, 85), false, false, "Allow for you to move a rail (and whatever is on said rail) left or right with the ‘mov l’ and ‘mov r’ commands."),
		new ItemInfo("Rotator", ConveyorRotatorObject.GetCostStatic(), 1, 2, new Vector2I(0,2), new Vector2I(85, 85), false, false, "Allows for you to rotate a rail (and whatever is on said rail) by usingthe ‘rot l’ or ‘rot r’ commands."),
		new ItemInfo("Switch", SwitchObject.GetCostStatic(), 1, 2, new Vector2I(2,0), new Vector2I(130, 90), true, false, "Allows you to switch to position of a rail (and whatever is on said rail) by using the ‘swt’ command."),
	};

	static ItemInfo[] ClawSection = {
		new ItemInfo("Claw", ClawObject.GetCostStatic(), 3, 1, new Vector2I(0,0), new Vector2I(85, 131), false, true, "Claws can be moved using the ‘mov’ command, they must be on a rail and move onto a rail of the correct direction. You can pick up materials via the ‘grb’ command or drop material via the ‘drp’ command."),
		new ItemInfo("Vertical Rail", TrackObject.GetCostStatic(), 3, 1, new Vector2I(0,1), new Vector2I(85, 131), false, false, "This rail allows a claw to move up and down"),
		new ItemInfo("Horizontal Rail", TrackObject.GetCostStatic(), 3, 1, new Vector2I(0,2), new Vector2I(85, 131), false, false, "This rail allows a claw to move left and right"),
	};

	static ItemInfo[] FactorySection = {
		new ItemInfo("Furnace", FactoryFurnace.GetCostStatic(), 2, 3, new Vector2I(0,0), new Vector2I(85, 85), true, false, "Allows for you to smelt raw material into a bar of said material. The furnace needs a fuel source in the form of coal and takes two time steps to smelt a given object."),
		new ItemInfo("Roller", FactoryRoller.GetCostStatic(), 2, 3, new Vector2I(0,2), new Vector2I(85, 85), true, false, "The roller allows you to turn a sheet of material into a pipe of that given material. It takes 1 time step to accomplish this and does not require any fuel."),
		new ItemInfo("Press", FactoryPress.GetCostStatic(), 2, 3, new Vector2I(0,3), new Vector2I(130, 90), true, false, "The press allows you to turn a bar of a given material into a sheet of that given material. It takes 1 time step to accomplish this and does not require any fuel."),
		new ItemInfo("Pipe", FactoryPress.GetCostStatic(), 2, 13, new Vector2I(0,0), new Vector2I(85, 85), false, false, "This pipe allows for the flow of liquid, it'll attatch to any adjacent pipes, liquid inputs, and liquid outputs"),
	};

	static ItemInfo[] DeveloperSection =
	{
		// Factory Input Objects
		new ItemInfo("Coal In", FactoryInputObject.GetCostStatic(), 2, 0, new Vector2I(1,0), new Vector2I(85, 85), false, false, "Input for coal"),
		new ItemInfo("Iron Ore In", FactoryInputObject.GetCostStatic(), 2, 0, new Vector2I(2,0), new Vector2I(85, 85), false, false, "Input for iron ore"),
		new ItemInfo("Iron Bar In", FactoryInputObject.GetCostStatic(), 2, 0, new Vector2I(3,0), new Vector2I(85, 85), false, false, "Input for iron bar"),
		new ItemInfo("Iron Plate In", FactoryInputObject.GetCostStatic(), 2, 0, new Vector2I(0,2), new Vector2I(85, 85), false, false, "Input for iron plate"),
		new ItemInfo("Iron Rod In", FactoryInputObject.GetCostStatic(), 2, 0, new Vector2I(1,2), new Vector2I(85, 85), false, false, "Input for iron rod"),
		
		// Factory Output Objects
		new ItemInfo("Coal Out", FactoryOutputObject.GetCostStatic(), 2, 0, new Vector2I(1,1), new Vector2I(85, 85), false, false, "Output for coal"),
		new ItemInfo("Iron Ore Out", FactoryOutputObject.GetCostStatic(), 2, 0, new Vector2I(2,1), new Vector2I(85, 85), false, false, "Output for iron ore"),
		new ItemInfo("Iron Bar Out", FactoryOutputObject.GetCostStatic(), 2, 0, new Vector2I(3,1), new Vector2I(85, 85), false, false, "Output for iron bar"),
		new ItemInfo("Iron Plate Out", FactoryOutputObject.GetCostStatic(), 2, 0, new Vector2I(0,3), new Vector2I(85, 85), false, false, "Output for iron plate"),
		new ItemInfo("Iron Rod Out", FactoryOutputObject.GetCostStatic(), 2, 0, new Vector2I(1,3), new Vector2I(85, 85), false, false, "Output for iron rod"),
		
		// Floor Tiles
		new ItemInfo("Floor Tile 1", FloorTileObject.GetCostStatic(), 2, 4, new Vector2I(0,0), new Vector2I(85, 85), false, false, "Tile for the floor"),
		
		// Floor Obstructions
		new ItemInfo("Floor Cracked Tile Object", FloorCrackedTileObject.GetCostStatic(), 2, 5, new Vector2I(0,0), new Vector2I(85, 85), false, false, "Broken floor tile"),
		new ItemInfo("Broken Pipe: Floor Left", PipeBrokenFloorObject.GetCostStatic(), 2, 5, new Vector2I(0,3), new Vector2I(85, 85), false, false, "Broken pipe on the floor"),
		new ItemInfo("Broken Pipe: Floor Right", PipeBrokenFloorObject.GetCostStatic(), 2, 5, new Vector2I(1,3), new Vector2I(85, 85), false, false, "Broken pipe on the floor"),
		
		// Ceiling (Rail Layer) Obstructions
		new ItemInfo("StalagmiteObject", StalagmiteObject.GetCostStatic(), 3, 5, new Vector2I(0,1), new Vector2I(85, 85), false, false, "It might tough the celing"),
		new ItemInfo("StalagmiteObjects", StalagmitesObject.GetCostStatic(), 3, 5, new Vector2I(1,1), new Vector2I(85, 85), false, false, "It might tough the celing"),
		new ItemInfo("Broken Pipe: Ceiling Left", PipeBrokenCeilingObject.GetCostStatic(), 3, 5, new Vector2I(0,2), new Vector2I(85, 85), false, false, "Is there water dripping from up there? Watch out, someone might sue!!"),
		new ItemInfo("Broken Pipe: Ceiling Right", PipeBrokenCeilingObject.GetCostStatic(), 3, 5, new Vector2I(1,2), new Vector2I(85, 85), false, false, "Is there water dripping from up there? Watch out, someone might sue!!"),
	};

	public void Update(int selection)
	{
		// This should be called when we change the type of object that we are wanting to select
		// The currSelect in the manager should be set beforehand as it will use that value to change teh sprites it contains
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		manager.currSlection = selection;

		switch (selection)
		{
			case 1: // Movement
				SetMovement();
				break;
			case 2: // Factory
				SetFactory();
				break;
			case 3: // Claw
				SetClaw();
				break;
			case 4: // Dev
				SetDeveloper();
				break;
			default:
				break;
		}
	}

	void KillChildren()
	{
		// This will kill all the children that are sprites
		// This also need to be modular because the children may need to be different
		foreach (Node child in GetChildren()) {
			if (child is Control) { // replace this with different class if we need to wrap the child class in something
				child.QueueFree(); // This kills the child
				RemoveChild(child);
				// GD.Print("Child has been killed");
			} else {
				// GD.Print("This is not the right child");
			}
		}
	}

	Control createBoilerObjectSelector(ImageTexture texture, Vector2I atlasCords, int sel, int sourceID, int posX, int posY, Label priceLabel, PanelContainer vboxPanel) {
		DragableObjectControl objectController = new DragableObjectControl(texture, atlasCords, sel, sourceID, posX, posY, priceLabel, vboxPanel);
		objectController.SetSize(new Vector2I(100, 100));
		objectController.Set(Control.PropertyName.CustomMinimumSize, new Vector2I(128, 128));
		return objectController;
	}

	void loadSprite(ItemInfo item) {
		var tileSet = GD.Load<TileSet>("res://Resources/objects.tres");
		int sourceid = tileSet.GetSourceId(item.table);

		TileSetAtlasSource tileSetSource = tileSet.GetSource(sourceid) as TileSetAtlasSource;
		if (tileSetSource == null)
		{
			GD.Print(item.table, " is not a valid id for the sprite tabel");
			return; // make sure it exists
		}
		AddChild(new Control()); // Creates left padding so its not smushed against container
		ImageTexture texture = new ImageTexture();

		if (item.big) {
			List<PlaceableBigData> data = ObjectFactory.GetBigObjectTileMap(BoilerTronicsData.objectMap[BoilerTronicsData.hashCoords(item.table, item.atPos)], PlaceableBig.Direction.UP);
			GD.Print(data);
			texture = PlaceableBig.GetBigTexture(data) as ImageTexture;
			// Used for scaling later ? (Unsure exactly how we would do this and preserve th scale when dragging)
			double hScale = 32 / texture.GetHeight();
			double wScale = 32 / texture.GetWidth();
		} else {
			// get the tile
			var tile = tileSetSource.GetTileTextureRegion(item.atPos);
			var fullTexture = tileSetSource.Texture.GetImage();
			var imageTexture = fullTexture.GetRegion(tile);
			texture.SetImage(imageTexture);
		}

		// Initialize vbox with styling for Name Label and Sprite
		PanelContainer vboxPanel = new PanelContainer();
		StyleBoxFlat vboxStyle = new StyleBoxFlat();
		vboxPanel.AddThemeStyleboxOverride("panel", vboxStyle);
		vboxPanel.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;

		VBoxContainer vbox = new VBoxContainer();
		vbox.AddThemeConstantOverride("separation", 20);
		
		AddChild(vboxPanel);
		vboxPanel.AddChild(vbox);
		PanelContainer background = new PanelContainer();

		// Initialize stylebox for the panel behind sprite
		StyleBoxFlat backgroundStyle = new StyleBoxFlat();
		backgroundStyle.ExpandMarginTop = 10;
		backgroundStyle.BgColor = new Color(0.35f, 0.35f, 0.35f, 1);
		backgroundStyle.BorderColor = new Color(0, 0, 0, 1);
		backgroundStyle.SetBorderWidthAll(2);
		backgroundStyle.SetCornerRadiusAll(10);
		background.AddThemeStyleboxOverride("panel", backgroundStyle);

		// Set name label to the name of the object.
		Label nameLabel = new Label();
		Label priceLabel = new Label();
		nameLabel.Text = item.name;
		priceLabel.Text = "Price: $" + item.price;

		FontFile pixelFont = ResourceLoader.Load<FontFile>("Resources/Fonts/VCR_OSD_MONO_1.001.ttf");
		priceLabel.AddThemeFontOverride("font", pixelFont);
		priceLabel.AddThemeColorOverride("font_color", new Color(0, 0, 0, 1));
		priceLabel.HorizontalAlignment = HorizontalAlignment.Center;

		nameLabel.AddThemeFontOverride("font", pixelFont);
		nameLabel.AddThemeColorOverride("font_color", new Color(0, 0, 0, 1));
		nameLabel.HorizontalAlignment = HorizontalAlignment.Center;

		GD.Print("Sel: ", item.sel);
		var spriteholder = createBoilerObjectSelector(texture, item.atPos, item.sel, item.table, item.offSet.X, item.offSet.Y, priceLabel, vboxPanel);
		spriteholder.TooltipText = item.tooltip;
		background.AddChild(spriteholder);
		
		vbox.AddChild(background);
		
		vbox.AddChild(nameLabel);
		vbox.AddChild(priceLabel);
	}

	void loadSprites(ItemInfo[] selection) {

		foreach (ItemInfo item in selection) {
			loadSprite(item);
		}
	}

	void SetClaw()
	{
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		// Kill the current children to ensure that only children that we want exist
		KillChildren();
		// This will spawn the children for all of the different kinds of claw layer elements
		loadSprites(ClawSection);
		manager.currSlection = 3;
	}

	void SetFactory()
	{
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		// Kill the current children to ensure that only children that we want exist
		KillChildren();
		// This will spawn the children for all of the different kinds of factory layer elements
		loadSprites(FactorySection);
		manager.currSlection = 2;
	}

	void SetMovement()
	{
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		// Kill the current children to ensure that only children that we want exist
		KillChildren();
		// This will spawn the children for all of the different kinds of movement layer elements
		loadSprites(MovementSection);
		manager.currSlection = 1;
	}

	void SetDeveloper()
	{
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		KillChildren();
		loadSprites(DeveloperSection);
		manager.currSlection = 4;
	}

	public override void _Ready()
	{
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		manager.picker = this;
		// This will allow us to have the default selection set here
		SetMovement();
	}
}
