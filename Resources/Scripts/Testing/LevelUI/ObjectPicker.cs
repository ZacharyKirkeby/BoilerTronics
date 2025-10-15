using Godot;
using System;

public partial class ObjectPicker : HBoxContainer
{
	// sprite table id for each table
	static int FactorySpriteTable = 0;
	static int ClawSpriteTable = 1;
	static int MovementSpriteTable = 2;

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

	Node createBoilerObjectSelector(ImageTexture texture, Vector2I atlasCords, int posX, int posY, int selection) {
		DragableObjectControl objectController = new DragableObjectControl(texture, atlasCords, posX, posY, selection);
		objectController.SetSize(new Vector2I(128, 128));
		objectController.Set(Control.PropertyName.CustomMinimumSize, new Vector2I(128, 128));
		return objectController;
	}

	void loadSprites(int source_idx, int posX, int posY, int selection) {
		var tileSet = GD.Load<TileSet>("res://Resources/objects.tres");
		int sourceid = tileSet.GetSourceId(source_idx);

		TileSetAtlasSource tileSetSource = tileSet.GetSource(sourceid) as TileSetAtlasSource;
		if (tileSetSource == null) {
			GD.Print(source_idx, " is not a valid id for the sprite tabel");
			return; // make sure it exists
		}
		for (int i = 0; i < tileSetSource.GetTilesCount(); i++) {
			var atlasCords = tileSetSource.GetTileId(i);
			if (atlasCords == null) continue; // make sure that the cords exist

			// get the tile
			var tile = tileSetSource.GetTileTextureRegion(atlasCords);
			var fullTexture = tileSetSource.Texture.GetImage();
			var imageTexture = fullTexture.GetRegion(tile);
			var texture = new ImageTexture();
			texture.SetImage(imageTexture);

			AddChild(createBoilerObjectSelector(texture, atlasCords, posX, posY, selection));

			// GD.Print(GetChildren());
		}
	}

	void SetClaw(int selection)
	{
		// Kill the current children to ensure that only children that we want exist
		KillChildren();
		// This will spawn the children for all of the different kinds of claw layer elements
		loadSprites(ClawSpriteTable, 128, 196, selection);
	}

	void SetFactory(int selection)
	{
		// Kill the current children to ensure that only children that we want exist
		KillChildren();
		// This will spawn the children for all of the different kinds of factory layer elements
		loadSprites(FactorySpriteTable, 128, 128, selection);
	}

	void SetMovement(int selection)
	{
		// Kill the current children to ensure that only children that we want exist
		KillChildren();
		// This will spawn the children for all of the different kinds of movement layer elements
		loadSprites(MovementSpriteTable, 128, 128, selection);		
	}

	public override void _Ready()
	{
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		manager.picker = this;
		// This will allow us to have the default selection set here
		SetMovement(1);
	}
}
