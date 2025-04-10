using Godot;
using System;

public partial class Player : CharacterBody2D
{
	private float speed = 250;
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
	private bool movementPaused = false;
	private float movementPauseTimer = 0;
	private RigidBody2D swingPoint;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta){
		if(dashing&&IsOnWall()){
			dashing = false;
		}

		if(Input.IsActionPressed("left")!=Input.IsActionPressed("right") && !dashing){
			if(Input.IsActionPressed("left")){
				direction = -1;
			} else{
				direction = 1;
			}
			if(swinging){
				Velocity = Velocity.Lerp(new Vector2(speed/10*direction,Velocity.Y), acceleration);
			} else{
				Velocity = Velocity.Lerp(new Vector2(speed*direction,Velocity.Y), acceleration);
			}
			
		} else if(!dashing && !movementPaused){
			Velocity = Velocity.Lerp(new Vector2(0,Velocity.Y), acceleration);
		}

		if(!IsOnFloor()){
			if(coyoteTime>0){
				coyoteTime -=1;
			}
			if(Input.IsActionPressed("jump")&&jumping&&jumpTimer>0&&!dashing&&!movementPaused){
				Velocity = new Vector2(Velocity.X,Velocity.Y+Velocity.Y*(float)delta*(jumpTimer/jumpLength));
			} else{
				if(!dashing){
					if(jumping&&!movementPaused){
						jumping = false;
						Velocity = new Vector2(Velocity.X,Velocity.Y/1.5f);
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
			}
		}

		if(Input.IsActionJustPressed("grapple")){
			swinging = true;
			jumping = false;
			Velocity = Vector2.Zero;
			swingTimer = .08f;
			// shoot out thing but thats a later problem
			swingPoint = GetNode<RigidBody2D>("../GrassRope/SwingPoint");
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
			swingTimer-=(float)delta;
			if(Input.IsActionJustPressed("jump")){ // dismount
				swinging = false;
				Velocity = new Vector2(swingPoint.LinearVelocity.X * 1.5f, swingPoint.LinearVelocity.Y * 1.17f);
				movementPaused = true;
				movementPauseTimer = .75f;
				acceleration = .05f;
				jumping = false;
			}
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
	}
}
