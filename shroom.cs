using Godot;
using System;

public partial class shroom : CharacterBody2D
{
	private int health = 10;
	private PackedScene shootShroom;
	private float shotTimer = 2f;
	private float shotCooldown = 2f;
	private Player player;
	private bool targeted = false;
	private float shotSpeed = 600;
	private Area2D hitbox;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		shootShroom = ResourceLoader.Load<PackedScene>("shootShroom.tscn");
		player = GetNode<Player>("../Player");
		hitbox = GetNode<Area2D>("ShroomHitbox");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		shotTimer -= (float)delta;
		if(shotTimer <= 0){
			shotTimer = shotCooldown;
			if(Mathf.Abs(player.Position.X-Position.X)< 800){
				shotShroom shot = shootShroom.Instantiate<shotShroom>();
				if(targeted){
					shot.Rotation = GetAngleTo(player.Position+new Vector2(player.Velocity.X, player.Velocity.Y/5));
					shot.Velocity = new Vector2(shotSpeed, 0).Rotated(shot.Rotation);
					shot.GetNode<AnimatedSprite2D>("ShotSprite").Play("orange");
					AddChild(shot);
				} else{
					shot.Rotation = GetAngleTo(player.Position);
					shot.Velocity = new Vector2(shotSpeed, 0).Rotated(shot.Rotation);
					shot.GetNode<AnimatedSprite2D>("ShotSprite").Play("red");
					AddChild(shot);
				}
				targeted = !targeted;
			}
		}
	}

	public void hit(Area2D area){
		health -= 5;
		GD.Print("shroom hurt");
		GD.Print(area.Name);
		area.SetDeferred("monitorable", false);
		if(health<=0){
			QueueFree();
		}
	}
}
