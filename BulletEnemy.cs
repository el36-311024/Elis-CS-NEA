using Godot;
using System;

public partial class BulletEnemy : CharacterBody3D
{
	private float MoveSpeed = 100f;
	private float MaxDistance = 500f;
	private float distanceTraveled = 0f;
	public float Damage = 10f;
	public Vector3 Direction = Vector3.Zero;
	public Node3D EnemyShooter;
	
	public override void _Ready()
	{
		AddToGroup("BulletEnemy");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 motion = Direction * MoveSpeed * (float)delta;
		KinematicCollision3D collision = MoveAndCollide(motion);
		distanceTraveled += motion.Length();

		if (collision != null)
		{
			if (collision.GetCollider() is Character character)
			{
				character.TakeDamage(Damage);
			}
			else if (collision.GetCollider() is team teamTarget)
			{
				teamTarget.TakeDamage((int)Damage);
			}
			else if (collision.GetCollider() is enemy enemyTarget)
			{
				enemyTarget.TakeDamage((int)Damage); 
			}
			Despawn();
		}

		if (distanceTraveled >= MaxDistance)
		{
			Despawn();
		}
		}
		
		public void SetGunType(string InputGun)
		{
			switch(InputGun)
			{
				case "Pistol":
					MoveSpeed = 100f;
					MaxDistance = 200f;
					Damage = 10f;
					break;
				case "Heavy":
					MoveSpeed = 500f;
					MaxDistance = 200f;
					Damage = 5f;
					break;
				case "Sniper":
					MoveSpeed = 750f;
					MaxDistance = 500f;
					Damage = 50f;
					break;
				case "Rifle1":
					MoveSpeed = 175f;
					MaxDistance = 250f;
					Damage = 15f;
					break;
				case "Rifle2":
					MoveSpeed = 200f;
					MaxDistance = 250f;
					Damage = 20f;
					break;
			}
		}
		
		private void Despawn()
		{
			QueueFree();
		}
}
