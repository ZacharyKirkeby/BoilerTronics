using Godot;
using System;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Layers;

public partial class MovingObject : Area2D {

	Vector2 PosDelta; // amount we need to move (relative to global position)
	double TotalDelta; // Total time that has elapsed
	double TargetDelta; // Time that we want the movement to take
	Vector2I TargetPos; // This is the target grid position
	Vector2 CurrGlobalPos;
	Vector2 TargetGlobalPos;
	PlaceableObject obj; // Object that is moving
	Layer layer; // Layer that object belongs to
	bool collided = false;

	bool move = true;

	public MovingObject(PlaceableObject obj, Vector2I pos, Vector2I mov, Layer layer, float time) {
		float xd = mov.X;
		float yd = mov.Y;
		// We may also need to set th position of the object globally

		this.TargetPos = pos + mov;
		this.TargetDelta = time;

		Vector2 localCurrPos = layer.ToLocal(pos);
		Vector2 globalCurrPos = layer.ToLocal(localCurrPos);

		this.CurrGlobalPos = globalCurrPos;

		Vector2 localTargetPos = layer.ToLocal(this.TargetPos);
		Vector2 globalTargetPos = layer.ToLocal(localTargetPos);

		this.TargetGlobalPos = globalTargetPos;

		this.PosDelta = this.TargetGlobalPos - this.CurrGlobalPos; // Calculate the amout we need to move

		this.Position = CurrGlobalPos;
	}

	public override void _Ready() {
		// Create collison object 2d
		CollisionShape2D shape = new CollisionShape2D();
		shape.Position = this.CurrGlobalPos;

		// Create collison circle 2d
		CircleShape2D circle = new CircleShape2D();
		circle.Radius = 32; // 32 pixels (height of the objects)

		shape.Shape = circle;

		this.AddChild(shape);

		// Create sprite
		Sprite2D sprite = new Sprite2D();
		sprite.Texture = this.obj.GetTexture() as Texture2D;
		sprite.Position = this.CurrGlobalPos;

		this.AddChild(sprite);

		// Register with the GameState (For resets and errors and such)

		// add event for when we detect a collision
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
		if (collided) return; // Don't do anything if this is already handled
		collided = true; // Set flag for this collision
		// We have collided with something else, this is a problem and shouldn't happen :(
		// This will trigger an error and then halt all movement
		// Send something to the game state (TBD)

	}

	public void Halt() {
		// make this flag false so that stop moving
		move = false;
	}
}
