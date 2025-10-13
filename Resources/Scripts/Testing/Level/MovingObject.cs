using Godot;
using System;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Layers;

public partial class MovingObject : Area2D {

	Vector2 PosDelta;
	double TotalDelta;
	double TargetDelta;
	Vector2I TargetPos;
	PlaceableObject obj;
	Layer layer;

	bool move = true;

	public MovingObject(PlaceableObject obj, Vector2I pos, Vector2I mov, Layer layer, float time) {
		float xd = mov.X;
		float yd = mov.Y;
		// We may also need to set th position of the object globally

		this.TargetPos = pos + mov;
		this.TargetDelta = time;

		this.PosDelta = new Vector2((float) ((Math.Sqrt(3.0) / 2) * (xd - yd)), ((-1 / 2) * (xd + yd))); // this will convert layer coord to movement
		// We also will need to multiply this by some lenght, unsure what that is atm
	}

	public override void _Ready() {
		// Create collison object 2d
		CollisionShape2D shape = new CollisionShape2D();

		// Create collison circle 2d
		CircleShape2D circle = new CircleShape2D();
		circle.Radius = 1; // This will be some set value (TBD)

		shape.Shape = circle;

		this.AddChild(shape);

		// Create sprite
		Sprite2D sprite = new Sprite2D();
		sprite.Texture = this.obj.GetTexture() as Texture2D;

		// Add some other configs her for the sprite

		this.AddChild(sprite);

		// Register with the GameState (For resets and errors and such)

		// add event for when we detect a collison
		AreaEntered += Collison;
	}

	public override void _Process(double delta)
	{
		if (!move) {
			base._Process(delta);
			return;
		}

		this.TotalDelta += delta;

		if (this.TotalDelta >= this.TargetDelta) {
			// We done
			// Move the object internally
			obj.MoveObject(TargetPos.X, TargetPos.Y);
			// Place the object back on the layer
			layer.AddObject(obj);
			// De-register object from the game state

			// Destroy this object
			this.QueueFree();
		}

		// We need to move
		Vector2 CurrPos = this.Position;
		CurrPos += this.PosDelta * ((float) (delta/TargetDelta)); // Calculate new pos for this step
		this.Position = CurrPos; // set new pos

		base._Process(delta);
	}

	public void Collison(Node2D body) {
		// We have collided with somthing else, this is a problem and shouldn't happen :(
		// Somthing bad need to happen
		// Send somthing to the game state
	}

	public void Hault() {
		// make this flag false so that stop moving
		move = false;
	}
}
