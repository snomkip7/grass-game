using Godot;
using System;

public partial class ropeSpawner : Area2D
{
	private Vector2 velocity = new Vector2(800,0);
	private bool doStuffs = false;
	private float timerThing = .4f;
	private bool thing = true;

	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(thing){
			LookAt(GetGlobalMousePosition());
			thing = false;
		}
		Position += (velocity*(float)delta).Rotated(Rotation);
		GD.Print("velocity"+velocity);
		if(doStuffs){
			doStuff();
		}
		timerThing -= (float) delta;
		if(timerThing < 0){
			GetNode<Player>("../Player").shotRope = false;
			QueueFree();
		}
	}

	public void stop(Node2D area){
		velocity = Vector2.Zero;
		doStuffs = true;
		GD.Print("hit smth");
	}

	public void doStuff(){
		GD.Print("doin stuff");
		GetNode<Player>("../Player").spawnRope(Position, Rotation+Mathf.Pi/2);
		QueueFree();
	}
}
