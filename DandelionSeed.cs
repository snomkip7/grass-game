using Godot;
using System;

public partial class DandelionSeed : CharacterBody2D
{
	private float deleteTimer = 10f;
	public override void _Ready()
	{
		Velocity = new Vector2(50, 200);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		deleteTimer -= (float)delta;
		if (deleteTimer <= 0){
			CallDeferred("ded");
		}
		MoveAndSlide();
	}

	public void die(Node2D thing){
		CallDeferred("ded");
	}

	public void ded(){
		QueueFree();
	}
}
