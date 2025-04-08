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

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if(dashing&&IsOnWall()){
			dashing = false;
		}

		if(Input.IsActionPressed("left")!=Input.IsActionPressed("right") && !dashing){
			if(Input.IsActionPressed("left")){
				direction = -1;
			} else{
				direction = 1;
			}
			Velocity = Velocity.Lerp(new Vector2(speed*direction,Velocity.Y), acceleration);
		} else if(!dashing){
			Velocity = Velocity.Lerp(new Vector2(0,Velocity.Y), acceleration);
		}

		if(!IsOnFloor()){
			if(coyoteTime>0){
				coyoteTime -=1;
			}
			if(Input.IsActionPressed("jump")&&jumping&&jumpTimer>0&&!dashing){
				Velocity = new Vector2(Velocity.X,Velocity.Y+Velocity.Y*(float)delta*(jumpTimer/jumpLength));
			} else{
				if(!dashing){
					if(jumping){
						jumping = false;
						Velocity = new Vector2(Velocity.X,Velocity.Y/1.5f);
					}
					Velocity = new Vector2(Velocity.X,Velocity.Y+gravity*(float)delta);
				}
				if(coyoteTime >0 && !jumping){
					if(Input.IsActionJustPressed("jump")){
						Velocity = new Vector2(Velocity.X,Velocity.Y+jumpSpeed);
						jumping = true;
						jumpTimer = jumpLength;
					}
				}
			} 
		} else if(!dashing){
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

		if(Input.IsActionJustPressed("dash") && dashTimer==0){
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

		MoveAndSlide();

		for (int i = GetSlideCollisionCount()-1; i>=0; i--){
			GodotObject collision = GetSlideCollision(i).GetCollider();
			if(collision.GetType() == typeof(RigidBody2D)){
				((RigidBody2D) collision).ApplyCentralImpulse(-GetSlideCollision(i).GetNormal() * pushStrength);
			}
		} 
	}
}
