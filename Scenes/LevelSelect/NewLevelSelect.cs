
using System;
using System.Buffers;
using Godot;


public partial class NewLevelSelect : Node2D

{
	[Export] public Vector2 ScreenSize {get; set;} = new Godot.Vector2(1920, 1152);
	[Export] public Vector2I GridSize {get; set;} = new Vector2I(6, 5);
	[Export] public Vector2 TrainOffset {get; set;} = Vector2.Zero;

	private int levelnum;
	private Sprite2D _train;
	private Camera2D _camera;
	private Button _btnUp;
	private Button _btnDown;
	private Button _btnLeft;
	private Button _btnRight;
	private Vector2I _gridPos = new Vector2I(0, 0);
	private Tween _tween;
	private bool _isMoving = false;
	/* Save Box ? (Ethan Change name for clarification) */

	private StyleBoxFlat FullSaveButtonTheme = new StyleBoxFlat();
	private StyleBoxFlat EmptySaveButtonTheme = new StyleBoxFlat();
	private StyleBoxFlat EmptySaveButtonHoverTheme = new StyleBoxFlat();
	private StyleBoxFlat FullSaveButtonHoverTheme = new StyleBoxFlat();

	/* Buttons */

	private Button saveZero;
	private Button saveOne;
	private Button saveTwo;
	private Button clearZero;
	private Button clearOne;
	private Button clearTwo;
	private Button pauseButton;
	private Button playButton;
	public override void _Ready()
	{
		saveZero = GetNode<Button>("CanvasLayer/Window/SaveContainer/Save0Cont/Save 0");
		saveOne = GetNode<Button>("CanvasLayer/Window/SaveContainer/Save1Cont/Save 1");
		saveTwo = GetNode<Button>("CanvasLayer/Window/SaveContainer/Save2Cont/Save 2");
		clearZero = GetNode<Button>("CanvasLayer/Window/SaveContainer/Save0Cont/Clear 0");
		clearOne = GetNode<Button>("CanvasLayer/Window/SaveContainer/Save1Cont/Clear 1");
		clearTwo = GetNode<Button>("CanvasLayer/Window/SaveContainer/Save2Cont/Clear 2");
		FullSaveButtonTheme.BgColor = new Color(1, 0, 0);
		FullSaveButtonTheme.BorderColor = new Color(0, 0, 0);
		FullSaveButtonTheme.SetBorderWidthAll(3);
		FullSaveButtonTheme.SetCornerRadiusAll(20);
		FullSaveButtonHoverTheme = FullSaveButtonTheme.Duplicate() as StyleBoxFlat;
		FullSaveButtonHoverTheme.BorderColor = new Color(1, 1, 1);
		EmptySaveButtonTheme.BgColor = new Color(0, 0.7f, 0);
		EmptySaveButtonTheme.BorderColor = new Color(0, 0, 0);
		EmptySaveButtonTheme.SetBorderWidthAll(3);
		EmptySaveButtonTheme.SetCornerRadiusAll(20);
		EmptySaveButtonHoverTheme = EmptySaveButtonTheme.Duplicate() as StyleBoxFlat;
		EmptySaveButtonHoverTheme.BorderColor = new Color(1, 1, 1);
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
	private void full_theme(Button button)
	{
		button.AddThemeStyleboxOverride("normal", FullSaveButtonTheme);
		button.AddThemeStyleboxOverride("hover", FullSaveButtonHoverTheme);
		button.AddThemeStyleboxOverride("focus", FullSaveButtonTheme);
	}

	private void empty_theme(Button button)
	{
		button.AddThemeStyleboxOverride("normal", EmptySaveButtonTheme);
		button.AddThemeStyleboxOverride("hover", EmptySaveButtonHoverTheme);
		button.AddThemeStyleboxOverride("focus", EmptySaveButtonTheme);
	}
	private void _on_save_button_pressed() {

		// Don't allow saving while stepping!
		// TODO: visually indicate that system cannot save	
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

		if (manager.currLevel.StepCount != 0) {
			return;
		}

		saveZero.AddThemeColorOverride("font_color_hover", new Color(0.8f, 0.8f, 0.8f));
		saveOne.AddThemeColorOverride("font_color_hover", new Color(0.8f, 0.8f, 0.8f));
		saveTwo.AddThemeColorOverride("font_color_hover", new Color(0.8f, 0.8f, 0.8f));

		if (manager.CheckSaveData(manager.GetLevelID(), 0))
		{
			full_theme(saveZero);
			clearZero.Visible = true;
		}
		else
		{
			empty_theme(saveZero);
			clearZero.Visible = false;
		}

		if (manager.CheckSaveData(manager.GetLevelID(), 1))
		{
			full_theme(saveOne);
			clearOne.Visible = true;
		}
		else
		{
			empty_theme(saveOne);
			clearOne.Visible = false;
		}

		if (manager.CheckSaveData(manager.GetLevelID(), 2))
		{
			full_theme(saveTwo);
			clearTwo.Visible = true;
		}
		else
		{
			empty_theme(saveTwo);
			clearTwo.Visible = false;
		}

		GetNode<Window>("Window").Visible = true;
	}

	private void _on_save_0_pressed()
	{
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager ;
		if (manager.CheckSaveData(levelnum, 0))
		{
			LoadLevel(levelnum, 0);
		} else
		{
			LoadLevel(levelnum, -1);
		}
	}

	private void _on_save_1_pressed()
	{
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager ;
		if (manager.CheckSaveData(levelnum, 1))
		{
			LoadLevel(levelnum, 1);
		} else
		{
			LoadLevel(levelnum, -1);
		}
	}

	private void _on_save_2_pressed()
	{
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager ;
		if (manager.CheckSaveData(levelnum, 2))
		{
			LoadLevel(levelnum, 2);
		} else
		{
			LoadLevel(levelnum, -1);
		}
	}

	// TODO: Ethen should update these to use 'SaveManager' specific functions for consistency and etc
	private void _on_clear_0_pressed()
	{
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		DirAccess.RemoveAbsolute("user://level" + levelnum + "/save0.save");
		empty_theme(saveZero);
		clearZero.Visible = false;
	}

	private void _on_clear_1_pressed()
	{
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		DirAccess.RemoveAbsolute("user://level" + levelnum + "/save1.save");
		empty_theme(saveOne);
		clearOne.Visible = false;
	}

	private void _on_clear_2_pressed()
	{
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		DirAccess.RemoveAbsolute("user://level" + levelnum + "/save2.save");
		empty_theme(saveTwo);
		clearTwo.Visible = false;
	}
	private void _on_window_close_requested()
	{
		GetNode<Window>("CanvasLayer/Window").Visible = false;
	}
	private void _on_play_pressed()
{
	// The button that called this
	var button = GetNode<Button>("%Play" +  (_gridPos.Y+1).ToString() + (_gridPos.X+1).ToString());
	BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;


	int section = _gridPos.Y+1; 
	int level = _gridPos.X+1;
	String levelstr = section.ToString() + level.ToString();
	levelnum = levelstr.ToInt();
	if (!manager.CheckSaveData(levelnum, 0) && !manager.CheckSaveData(levelnum, 1) && !manager.CheckSaveData(levelnum, 2))
		{
			LoadLevel(levelnum, -1);
		} else
		{
			saveZero.AddThemeColorOverride("font_color_hover", new Color(0.8f, 0.8f, 0.8f));
			saveOne.AddThemeColorOverride("font_color_hover", new Color(0.8f, 0.8f, 0.8f));
			saveTwo.AddThemeColorOverride("font_color_hover", new Color(0.8f, 0.8f, 0.8f));

			if (manager.CheckSaveData(manager.GetLevelID(), 0))
			{
				full_theme(saveZero);
				clearZero.Visible = true;
			}
			else
			{
				empty_theme(saveZero);
				clearZero.Visible = false;
			}

			if (manager.CheckSaveData(manager.GetLevelID(), 1))
			{
				full_theme(saveOne);
				clearOne.Visible = true;
			}
			else
			{
				empty_theme(saveOne);
				clearOne.Visible = false;
			}

			if (manager.CheckSaveData(manager.GetLevelID(), 2))
			{
				full_theme(saveTwo);
				clearTwo.Visible = true;
			}
			else
			{
				empty_theme(saveTwo);
				clearTwo.Visible = false;
			}
			GetNode<Window>("CanvasLayer/Window").Visible = true;
		}
	GD.Print($"Pressed section {section} level {level}");

	//LoadLevel(section, level);
	}

	private void LoadLevel(int levelnum, int savenum)
{
	BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
	manager.SetTargetLevelSave(levelnum, savenum);
	CallDeferred(nameof(changescenes));
}
private void changescenes()
	{
		GetTree().ChangeSceneToFile("res://Scenes/LevelUI/level_ui.tscn");
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
