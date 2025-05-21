using Godot;
using System;

public partial class dandelion : CharacterBody2D
{
	private int health = 10;
	private AnimatedSprite2D sprite;
	private float attackTimer;
	private float attackCooldown = 3f;
	private bool attacking = false;
	private Player player;
	private RandomNumberGenerator rng;
	private float speed = 200;
	private float runSpeed = 600;
	private float currentTimer = 0;
	private PackedScene shroomScene;
	private PackedScene seeds;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		sprite = GetNode<AnimatedSprite2D>("DandelionSprite");
		player = GetNode<Player>("../Player");
		attackTimer = 10f;
		rng = new RandomNumberGenerator();
		shroomScene = ResourceLoader.Load<PackedScene>("shroom.tscn");
		seeds = ResourceLoader.Load<PackedScene>("DandelionSeed.tscn");
		sprite.Play("walk");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		attackTimer -= (float)delta;
		if (attackTimer <= 0 && !attacking)
		{
			int num = rng.RandiRange(1, 3);
			GD.Print(num);
			if (num == 1)
			{
				// dandelion spam
				int count = rng.RandiRange(4, 8);
				for (int i = 0; i < count; i++)
				{
					DandelionSeed seed = seeds.Instantiate<DandelionSeed>();
					seed.Position = Position + new Vector2(rng.RandiRange(-700, 700), -1000);
					GetParent().AddChild(seed);
				}
			}
			else if (num == 2)
			{
				// zoom at em
				if (player.Position.X - Position.X > 0)
				{
					Velocity = new Vector2(runSpeed, 0);
					sprite.Play("run");
					sprite.FlipH = true;
				}
				else
				{
					Velocity = new Vector2(-runSpeed, 0);
					sprite.Play("run");
					sprite.FlipH = false;
				}
				attacking = true;
				currentTimer = 3f;
			}
			else
			{
				// spawn the shrooms
				if (rng.RandiRange(1, 2) == 1){
					shroom shroom1 = shroomScene.Instantiate<shroom>();
					shroom1.Position = Position + Vector2.Left * 400 + Vector2.Up * 100;
					GetParent().AddChild(shroom1);
				}
				else{
					shroom shroom2 = shroomScene.Instantiate<shroom>();
					shroom2.Position = Position + Vector2.Right * 400+Vector2.Up * 100;
					GetParent().AddChild(shroom2);
				}
			}

			attackTimer = rng.Randfn(5f, 1f);
			if (!attacking)
			{
				Velocity *= -1;
				if (Velocity.X > 0)
				{
					sprite.FlipH = true;
				}
				else
				{	
					sprite.FlipH = false;
				}
				if (sprite.Animation != "walk")
				{
					sprite.Play("walk");
				}
			}
		}
		if (attacking)
		{
			currentTimer -= (float)delta;
			if (currentTimer <= 0)
			{
				attacking = false;
			}
		}
		else
		{
			if (Mathf.Abs(Velocity.X) != speed)
			{
				Velocity = new Vector2(-speed, 0);
			}
		}

		MoveAndSlide();
	}

	public void hit(Area2D area)
	{
		health -= 1;
		GD.Print("hit lion");
		area.SetDeferred("monitorable", false);
		if (health <= 0)
		{
			QueueFree();
		}
	}

	public void turn(Node2D body)
	{
		if (attacking)
		{
			Velocity = new Vector2(speed, 0);
		}
		Velocity *= -1;
		if (Velocity.X > 0)
		{
			sprite.FlipH = true;
		}
		else
		{	
			sprite.FlipH = false;
		}
		if (sprite.Animation != "walk")
		{
			sprite.Play("walk");
		}
	}
}
