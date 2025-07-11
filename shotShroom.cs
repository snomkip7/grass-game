using Godot;
using System;

public partial class shotShroom : CharacterBody2D
{
	private float lifespan = 3f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		MoveAndSlide();
		
		if(GetSlideCollisionCount()>=1){
			die();
		}
		lifespan -= (float)delta;
		if(lifespan<=0){
			die();
		}
	}

	public void die(){
		CallDeferred("ded");
	}

	public void ded(){
		QueueFree();
	}
}
