using Godot;
using System;

public partial class Player : CharacterBody2D
{
	private float speed = 400;
	private float direction = 1;
	private float acceleration = .2f;
	private float gravity = 1200;
	private float jumpSpeed = -400;
	private float jumInitialSpeed = -750;
	private bool jumping = false;
	private float jumpTimer = 0;
	private float jumpLength = .4f;
	private bool dashing = false;
	private float dashTimer = 0;
	private float dashCooldown = 1.3f;
	private float dashSpeed = 450;
	private float pushStrength = 100;
	private float coyoteTime = 10;
	private bool swinging = false;
	private float swingTimer = .3f;
	private float swingSpeed = 250;
	private bool movementPaused = false;
	private float movementPauseTimer = 0;
	private RigidBody2D swingPoint;
	private AnimatedSprite2D mainSprite;
	private AnimatedSprite2D partSprite;
	private bool attacking = false;
	private float attackTimer = .4f;
	private Area2D sword;
	private int health = 100;
	private bool animation = false;
	private CanvasModulate screenColor;
	private PackedScene ropeScene;
	private grassRope currentRope = null;
	private PackedScene ropeShoot;
	public bool shotRope = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		mainSprite = GetNode<AnimatedSprite2D>("PlayerMainSprite");
		mainSprite.Play("default");

		partSprite = GetNode<AnimatedSprite2D>("PlayerPartSprite");
		partSprite.Play("default");

		sword = GetNode<Area2D>("SwordCollision");
		screenColor = GetNode<CanvasModulate>("ScreenColor");
		sword.Position = new Vector2(0,-10000);
		ropeScene = ResourceLoader.Load<PackedScene>("grassRope.tscn");
		ropeShoot = ResourceLoader.Load<PackedScene>("ropeSpawner.tscn");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta){
		if(dashing&&IsOnWall()){
			dashing = false;
		}

		if(Input.IsActionPressed("left")!=Input.IsActionPressed("right") && !dashing){
			if(Input.IsActionPressed("left")){
				direction = -1;
				if(!attacking){
					mainSprite.FlipH = false;
					partSprite.FlipH = false;
					sword.Scale = new Vector2(-1,1);
				}
				
			} else{
				direction = 1;
				if(!attacking){
					mainSprite.FlipH = true;
					partSprite.FlipH = true;
					sword.Scale = new Vector2(1,1);
				}
			}
			if(swinging){
				Velocity = Velocity.Lerp(new Vector2(swingSpeed/10*direction,Velocity.Y), acceleration);
			} else{
				Velocity = Velocity.Lerp(new Vector2(swingSpeed*direction,Velocity.Y), acceleration);
			}
			
		} else if(!dashing && !movementPaused){
			Velocity = Velocity.Lerp(new Vector2(0,Velocity.Y), acceleration);
		} else if(!dashing){
			Velocity = Velocity.Lerp(new Vector2(Velocity.X/1.5f,Velocity.Y), acceleration);
		}

		if(!IsOnFloor()){
			if(coyoteTime>0){
				coyoteTime -=1;
			}
			if(Input.IsActionPressed("jump")&&jumping&&jumpTimer>0&&!dashing&&!movementPaused&&!swinging){
				Velocity = new Vector2(Velocity.X,Velocity.Y+Velocity.Y*(float)delta*(jumpTimer/jumpLength));
			} else{
				if(!dashing){
					if(jumping&&!movementPaused){
						jumping = false;
						Velocity = new Vector2(Velocity.X,Velocity.Y/1.7f);
					}

					Velocity = new Vector2(Velocity.X,Velocity.Y+gravity*(float)delta);
					
					if(Velocity.Y>10 && swinging){
						Velocity = new Vector2(Velocity.X,10);
					}
					
				}
				if(coyoteTime >0 && !jumping && Input.IsActionJustPressed("jump")){
					Velocity = new Vector2(Velocity.X,Velocity.Y+jumpSpeed);
					jumping = true;
					jumpTimer = jumpLength;
				}
			} 
		} else if(!dashing && !movementPaused){
			Velocity = new Vector2(Velocity.X,0);
			coyoteTime = 12;
			if(Input.IsActionJustPressed("jump")){
				Velocity = new Vector2(Velocity.X,Velocity.Y+jumpSpeed);
				jumping = true;
				jumpTimer = jumpLength;
			}
		}

		if(IsOnCeiling()){
			jumping = false;
			Velocity = new Vector2(Velocity.X,Velocity.Y/1.5f);
		}

		if(Input.IsActionJustPressed("dash") && dashTimer==0 && !swinging){
			Velocity = new Vector2((speed+dashSpeed)*direction,0);
			dashing = true;
			dashTimer = dashCooldown;
		}
		
		if(jumping){
			jumpTimer-=(float)delta;
		}

		if(dashTimer!=0){
			dashTimer -= (float)delta;
			if(dashing && dashTimer <= 1.06f){
				dashing = false;
				Velocity = new Vector2(speed*direction,0);
			}
			else if(dashTimer < 0){
				dashTimer = 0;
			}
		}

		if(movementPaused){
			movementPauseTimer -= (float)delta;
			if(movementPauseTimer<=0){
				movementPaused = false;
				acceleration = .2f;
				if(currentRope!=null && IsInstanceValid(currentRope)){
					currentRope.QueueFree();
				}
			}
		}

		if(Input.IsActionJustPressed("grapple") && !swinging && !shotRope){
			if(movementPauseTimer>0){
				movementPaused = false;
				acceleration = .2f;
				if(currentRope!=null && IsInstanceValid(currentRope)){
					currentRope.QueueFree();
				}
			}
			shotRope = true;
			ropeSpawner ropeSpawn = ropeShoot.Instantiate<ropeSpawner>();
			GetParent().AddChild(ropeSpawn);
			ropeSpawn.GlobalPosition = GlobalPosition;
		}
		
		if(Input.IsActionJustPressed("attack") && !attacking){
			sword.Monitorable = true;
			sword.Position = new Vector2(0,0);
			mainSprite.Play("swing");
			attacking = true;
			attackTimer = .3f;
		}
		attackTimer -= (float)delta;
		if(attackTimer<=0){
			attacking = false;
			sword.Monitorable = false;
			sword.Position = new Vector2(0,-10000);
		}
		

		if(swinging){
			GlobalPosition = swingPoint.GlobalPosition;
			if(swingTimer<=0){
				Vector2 newVelocity = Velocity;
				if(Velocity.Y > 200){
					newVelocity = new Vector2(100*direction, 100);
				}
				swingPoint.LinearVelocity += newVelocity;
				swingTimer = .08f;
			}
			if(Input.IsActionJustPressed("jump") || (Input.IsActionJustPressed("grapple") && swingTimer != .081f)){ // dismount
				swinging = false;
				Velocity = new Vector2(swingPoint.LinearVelocity.X * 1.5f, swingPoint.LinearVelocity.Y * 1.17f);
				movementPaused = true;
				movementPauseTimer = .75f;
				acceleration = .05f;
				jumping = false;
			}
			swingTimer-=(float)delta;
			coyoteTime = 0;
		}
		else{
			MoveAndSlide();
		}
		

		for (int i = GetSlideCollisionCount()-1; i>=0; i--){
			GodotObject collision = GetSlideCollision(i).GetCollider();
			if(collision.GetType() == typeof(RigidBody2D)){
				((RigidBody2D) collision).ApplyCentralImpulse(-GetSlideCollision(i).GetNormal() * pushStrength);
			}
		} 

		if(animation){
			screenColor.Color = new Color(screenColor.Color.R-.01f, screenColor.Color.G+.01f, screenColor.Color.B+.01f); 
			if(screenColor.Color.R <=1){
				screenColor.Color = new Color(1,1,1);
				animation = false;
			}
		}
	}

	public void animationFinish(){
		mainSprite.Play("default");
	}

	public void takeDmg(int amount){
		health -= amount;
		screenColor.Color = new Color(2,0,0);
		GD.Print(health);
		animation = true;
		if(health<=0){
			CallDeferred("restart");
		}
	}

	public void ouch(Node2D mean){
		if(mean.GetType() == typeof(shotShroom)){
			takeDmg(25);
			mean.QueueFree();
		} else if(mean.GetParent().GetType() == typeof(shroom)){
			takeDmg(25);
		} else if(mean.GetType() == typeof(dandelion)){
			takeDmg(25);
		} else if(mean.GetType() == typeof(DandelionSeed)){
			takeDmg(25);
		}
	}

	public void spawnRope(Vector2 pos, float rot){
		GD.Print("spawn stuff");
		swinging = true;
		jumping = false;
		Velocity = Vector2.Zero;
		swingTimer = .081f;
		grassRope rope = ropeScene.Instantiate<grassRope>();
		GetParent().AddChild(rope);
		rope.Position = pos;
		rope.Rotation = rot;
		currentRope = rope;
		swingPoint = rope.GetNode<RigidBody2D>("SwingPoint");
		shotRope = false;
		
	}

	public void restart(){
		GetTree().ReloadCurrentScene();
	}
	
	public void startGame(Node2D nodething){
		GetTree().ChangeSceneToFile("finalBoss.tscn");
	}
	
}
