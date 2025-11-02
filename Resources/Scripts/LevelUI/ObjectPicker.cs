using Godot;
using System;
using System.Reflection.Metadata;

public partial class ObjectPicker : HBoxContainer
{
	// sprite table id for each table
	static int FactorySpriteTable = 0;
	static int ClawSpriteTable = 1;
	static int MovementSpriteTable = 2;
	static String[] MovementSpriteNames = { "Vertical Conveyor", "Horizontal Conveyor", "Rotator", "Placeholder", "Placeholder", "Placeholder" };
	static String[] FactorySpriteNames = {"Input", "Output", "Floor", "Placeholder", "Placeholder", "Placeholder" };
	static String[] ClawSpriteNames = { "Claw", "Vertical Rail", "Horizontal Rail" };
	public static int[] MovementItemPrices = { 0, 0, 0, 0, 0, 0};
	public static int[] FactoryItemPrices = { 0, 0, 0, 0, 0, 0 };
	public static int[] ClawItemPrices = { 0, 0, 0 };
	public void Update(int selection) 
	{
		// This should be called when we change the type of object that we are wanting to select
		// The currSelect in the manager should be set beforehand as it will use that value to change teh sprites it contains
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		switch (selection) {
			case 1: // Movement
				SetMovement(selection);
				break;
			case 2: // Factory
				SetFactory(selection);
				break;
			case 3: // Claw
				SetClaw(selection);
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

	Node createBoilerObjectSelector(ImageTexture texture, Vector2I atlasCords, int posX, int posY, int selection, Label priceLabel, PanelContainer vboxPanel, int itemNumber) {
		DragableObjectControl objectController = new DragableObjectControl(texture, atlasCords, posX, posY, selection, priceLabel, vboxPanel, itemNumber);
		objectController.SetSize(new Vector2I(100, 100));
		objectController.Set(Control.PropertyName.CustomMinimumSize, new Vector2I(128, 128));
		return objectController;
	}

	void loadSprites(int source_idx, int posX, int posY, int selection) {
		var tileSet = GD.Load<TileSet>("res://Resources/objects.tres");
		int sourceid = tileSet.GetSourceId(source_idx);

		TileSetAtlasSource tileSetSource = tileSet.GetSource(sourceid) as TileSetAtlasSource;
		if (tileSetSource == null)
		{
			GD.Print(source_idx, " is not a valid id for the sprite tabel");
			return; // make sure it exists
		}
		AddChild(new Control()); // Creates left padding so its not smushed against container
		for (int i = 0; i < tileSetSource.GetTilesCount(); i++) {
			var atlasCords = tileSetSource.GetTileId(i);
			if (atlasCords == null) continue; // make sure that the cords exist

			// get the tile
			var tile = tileSetSource.GetTileTextureRegion(atlasCords);
			var fullTexture = tileSetSource.Texture.GetImage();
			var imageTexture = fullTexture.GetRegion(tile);
			var texture = new ImageTexture();
			texture.SetImage(imageTexture);

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
			if (source_idx == MovementSpriteTable)
			{
				nameLabel.Text = MovementSpriteNames[i];
				priceLabel.Text = "Price: $" + MovementItemPrices[i];
			}
			else if (source_idx == ClawSpriteTable)
			{
				nameLabel.Text = ClawSpriteNames[i];
				priceLabel.Text = "Price: $" + ClawItemPrices[i];
			}
			else if (source_idx == FactorySpriteTable)
			{
				nameLabel.Text = FactorySpriteNames[i];
				priceLabel.Text = "Price: $" + FactoryItemPrices[i];
			}
			else
			{
				nameLabel.Text = "Error";
				priceLabel.Text = "Error";
			}
			
			FontFile pixelFont = ResourceLoader.Load<FontFile>("Resources/Fonts/VCR_OSD_MONO_1.001.ttf");
			priceLabel.AddThemeFontOverride("font", pixelFont);
			priceLabel.AddThemeColorOverride("font_color", new Color(0, 0, 0, 1));
			priceLabel.HorizontalAlignment = HorizontalAlignment.Center;

			nameLabel.AddThemeFontOverride("font", pixelFont);
			nameLabel.AddThemeColorOverride("font_color", new Color(0, 0, 0, 1));
			nameLabel.HorizontalAlignment = HorizontalAlignment.Center;

			background.AddChild(createBoilerObjectSelector(texture, atlasCords, posX, posY, selection, priceLabel, vboxPanel, i));
			vbox.AddChild(background);
			vbox.AddChild(nameLabel);
			vbox.AddChild(priceLabel);
		}
	}

	void SetClaw(int selection)
	{
		// Kill the current children to ensure that only children that we want exist
		KillChildren();
		// This will spawn the children for all of the different kinds of claw layer elements
		loadSprites(ClawSpriteTable, 85, 131, selection); // 128, 196
	}

	void SetFactory(int selection)
	{
		// Kill the current children to ensure that only children that we want exist
		KillChildren();
		// This will spawn the children for all of the different kinds of factory layer elements
		loadSprites(FactorySpriteTable, 85, 85, selection); // 128, 128
	}

	void SetMovement(int selection)
	{
		// Kill the current children to ensure that only children that we want exist
		KillChildren();
		// This will spawn the children for all of the different kinds of movement layer elements
		loadSprites(MovementSpriteTable, 85, 85, selection);	// 128, 128
	}

	public override void _Ready()
	{
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		manager.picker = this;
		// This will allow us to have the default selection set here
		SetMovement(1);
	}
}
