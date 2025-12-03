
using Godot;


public partial class NewLevelSelect : Node2D

{
	[Export] public Vector2 ScreenSize {get; set;} = new Godot.Vector2(1920, 1152);
	[Export] public Vector2I GridSize {get; set;} = new Vector2I(6, 5);
	[Export] public Vector2 TrainOffset {get; set;} = Vector2.Zero;

	private Sprite2D _train;
	private Camera2D _camera;
	private Button _btnUp;
	private Button _btnDown;
	private Button _btnLeft;
	private Button _btnRight;
	private Vector2I _gridPos = new Vector2I(0, 0);
	private Tween _tween;
	private bool _isMoving = false;
	public override void _Ready()
	{
		_train = GetNode<Sprite2D>("%TrainSprite");
		_camera = GetNode<Camera2D>("%Camera2D");
		_btnUp = GetNode<Button>("%UpButton");
		_btnRight = GetNode<Button>("%RightButton");
		_btnDown = GetNode<Button>("%DownButton");
		_btnLeft = GetNode<Button>("%LeftButton");
		_camera.Enabled = true;
		_camera.MakeCurrent();
		_btnUp.Pressed += OnUpPressed;
		_btnDown.Pressed += OnDownPressed;
		_btnLeft.Pressed += OnLeftPressed;
		_btnRight.Pressed += OnRightPressed;
		SnapToGrid(_gridPos);
		UpdateDirectionButtons();


	}
	
	private void OnUpPressed() {
		if (!_isMoving)
		{
			SetSpriteDirection(new Vector2I(0, 1));
		}
		MoveInDirection(new Vector2I(0, 1));
		
	}
	private void OnDownPressed() {
		
		if (!_isMoving)
		{
			SetSpriteDirection(new Vector2I(0, -1));
		}
		MoveInDirection(new Vector2I(0, -1));
	}
	private void OnLeftPressed()
	{
		
		if (!_isMoving)
		{
			SetSpriteDirection(new Vector2I(-1, 0));
		}
		MoveInDirection(new Vector2I(-1, 0));
	} 
	private void OnRightPressed()
	{
		
		if (!_isMoving)
		{
			SetSpriteDirection(new Vector2I(1, 0));
		}
		MoveInDirection(new Vector2I(1, 0));
	} 

	private void MoveInDirection(Vector2I dir)
	{
		if (_isMoving)
		{
			return;
		}
		var newGrid = _gridPos + dir;
		newGrid.X = Mathf.Clamp(newGrid.X, 0, GridSize.X - 1);
		newGrid.Y = Mathf.Clamp(newGrid.Y, 0, GridSize.Y - 1);
		if (newGrid == _gridPos)
		{
			return;
		}
		_gridPos = newGrid;
		GD.Print(_gridPos);
		AnimateToGrid(_gridPos);
		UpdateDirectionButtons();
		
		
	}

	private void SnapToGrid(Vector2I grid)
	{
		Vector2 camTarget = GridToCameraPosition(grid);
		Vector2 trainTarget = camTarget + TrainOffset;
		_camera.Position = camTarget;
		_train.Position = trainTarget;
	}

	private void AnimateToGrid(Vector2I grid)
	{
		Vector2 camTarget = GridToCameraPosition(grid);
		Vector2 trainTarget = camTarget + TrainOffset;
		_tween?.Kill();
		_isMoving = true;
		_tween = CreateTween();
		_tween.SetTrans(Tween.TransitionType.Sine);
		_tween.SetEase(Tween.EaseType.Out);
		_tween.TweenProperty(_camera, "position", camTarget, 2f);
		_tween.Parallel().TweenProperty(_train, "position", trainTarget, 2f);
		_tween.Finished += () =>
		{
			_isMoving = false;
		};
	}

	private Vector2 GridToCameraPosition(Vector2I grid)
	{
		return new Vector2(
			grid.X * ScreenSize.X + ScreenSize.X / 2f,
			-grid.Y * ScreenSize.Y + ScreenSize.Y / 2f
		);
	}

	private void SetSpriteDirection(Vector2I dir)
{

	if (dir == new Vector2I(1, 0))
		_train.Texture = ResourceLoader.Load<Texture2D>("res://Resources/Sprites/32x32_trainr.png");

	else if (dir == new Vector2I(-1, 0))
		_train.Texture = ResourceLoader.Load<Texture2D>("res://Resources/Sprites/32x32_trainl.png");

	else if (dir == new Vector2I(0, 1))
		_train.Texture = ResourceLoader.Load<Texture2D>("res://Resources/Sprites/32x32_trainu.png");

	else if (dir == new Vector2I(0, -1))
		_train.Texture = ResourceLoader.Load<Texture2D>("res://Resources/Sprites/32x32_traind.png");
}

	private void UpdateDirectionButtons()
	{
		_btnLeft.Visible = _gridPos.X > 0;
		_btnRight.Visible = _gridPos.X < GridSize.X - 1;
		_btnUp.Visible = _gridPos.Y < GridSize.Y - 1 && _gridPos.X == 0;
		_btnDown.Visible = _gridPos.Y > 0 && _gridPos.X == 0;
	}

}
