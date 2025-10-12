using Godot;
using System;
using BoilerTronicsObjects.Layers;

public partial class LevelConstructor : Node2D
{
	int x;
	int y;
	MovementLayer mLayer;
	// RailLayer rLayer;
	ClawLayer cLayer;
	FactoryLayer fLayer;
	// FloorLayer flLayer;
	
	private void CreateMovement() {

	}

	private void CreateRail() {

	}
	
	private void CreateClaw() {

	}

	private void CreateFactory() {

	}

	private void CreateFloor() {

	}

	public override void _Ready()
	{
		// Create all of the diffrent layers and read in the corresponding data from the manager
		// Temp, this will be replaced by a read from the global manager's game state
		x = 20;
		y = 20;

		CreateMovement();
		CreateRail();
		CreateClaw();
		CreateFactory();
		CreateFloor();

		base._Ready();
	}
}
