using Godot;
using System;
using System.Collections.Generic;

public partial class Character : RigidBody3D
{
	private float MouseSensitivity = 0.005f;
	private float zoomSensitivity = 0.0015f;
	private float Speed = 7.5f;
	private float Jump = 10f;
	private float accumulatedRotating = 0f;
	private float rotating = 0f; 
	private float pitch = 0f;
	private float MinPitch = Mathf.DegToRad(-60);
	private float MaxPitch = Mathf.DegToRad(65);
	private RayCast3D GroundCheck;
	private bool isCrouching = false;
	private bool wasCrouching = false;
	private float crouchSpeed = 0.5f;
	private Vector3 standing = new Vector3(1, 1, 1);
	private Vector3 crouching = new Vector3(1, 0.5f, 1);

	private MeshInstance3D HeadMesh;
	private MeshInstance3D TorsoMesh;
	private MeshInstance3D LeftArmMesh;
	private MeshInstance3D RightArmMesh;
	private MeshInstance3D LeftLegMesh;
	private MeshInstance3D RightLegMesh;

	private CollisionShape3D HeadCollision;
	private CollisionShape3D TorsoCollision; 
	private CollisionShape3D LeftArmCollision; 
	private CollisionShape3D RightArmCollision; 
	private CollisionShape3D LeftLegCollision;
	private CollisionShape3D RightLegCollision;

	private Vector3 crouchOffset = new Vector3(0, -0.6f, 0);
	private Vector3 leftcrouchLegRotation = new Vector3(Mathf.DegToRad(-15), 0, 0);
	private Vector3 rightcrouchLegRotation = new Vector3(Mathf.DegToRad(-90), 0, 0);

	private Vector3 HeadStartPosition;
	private Vector3 TorsoStartPosition;
	private Vector3 LeftLegStartPosition;
	private Vector3 RightLegStartPosition;

	private Vector3 HeadStartCollisionPosition;
	private Vector3 TorsoStartCollisionPosition;
	private Vector3 LeftLegStartCollisionPosition;
	private Vector3 RightLegStartCollisionPosition;

	private Node3D PlayerBody;
	public Vector3 ShooterPosition;
	private Node3D Cam;
	private Camera3D Camera;
	private bool isZoomed = false;
	private float zoomSpeed = 8f;
	private float normalFov = 75f;
	private float zoomFov = 40f;
	private Vector3 normalCamPos;
	private Vector3 aimCamPos;
	private Vector3 gunAimOffset = new Vector3(0, 0.2f, 0.05f);
	private Node3D gunHolder;
	private Node3D currentGun;
	private PackedScene BulletScene;
	int ammo = 0;
	bool Reloading = false;
	
	public float MaxHealth = 10000f;
	private float CurrentHealth;
	private ProgressBar HealthBar;
	
	private float gunSpinAngle = 0f;
	private float gunSpinSpeed = 90f;
	private float totalSpinAmount = 360f;
	private bool isSpinningGun = false;

	private Node3D ArmPivot;
	private Node3D ArmMovement;
	private Node3D GunPivot;
	private Vector3 ArmPivotStart;
	
	public int NumBullets = 0;
	private double ShootCooldown = 0;
	string selectedGun;

	public override void _Ready()
	{
		Cam = GetNode<Node3D>("Cam");
		Camera = GetNode<Camera3D>("Cam/Camera");
		Camera.Current = true;
		rotating = Rotation.Y;
		if (Cam != null)
		{
			pitch = Cam.Rotation.X;
		}
		else
		{
			pitch = 0f;
		}
		
		Input.MouseMode = Input.MouseModeEnum.Captured;
		GroundCheck = GetNode<RayCast3D>("GroundCheck");
		normalCamPos = Camera.Position;
		CurrentHealth = MaxHealth;
		HealthBar = GetNode<ProgressBar>("Control/Health");
		HealthBar.MaxValue = MaxHealth;
		HealthBar.Value = CurrentHealth;
		HeadMesh = GetNode<MeshInstance3D>("HeadMesh");
		TorsoMesh = GetNode<MeshInstance3D>("TorsoMesh");
		LeftArmMesh = GetNode<MeshInstance3D>("ArmPivot/ArmMovement/LeftArmMesh");
		RightArmMesh = GetNode<MeshInstance3D>("ArmPivot/ArmMovement/RightArmMesh");
		LeftLegMesh = GetNode<MeshInstance3D>("LeftLegMesh");
		RightLegMesh = GetNode<MeshInstance3D>("RightLegMesh");
		HeadCollision = GetNode<CollisionShape3D>("HeadCollision");
		TorsoCollision = GetNode<CollisionShape3D>("TorsoCollision");
		LeftArmCollision = GetNode<CollisionShape3D>("ArmPivot/ArmMovement/LeftArmMesh/LeftArmCollision");
		RightArmCollision = GetNode<CollisionShape3D>("ArmPivot/ArmMovement/RightArmMesh/RightArmCollision");
		LeftLegCollision = GetNode<CollisionShape3D>("LeftLegCollision");
		RightLegCollision = GetNode<CollisionShape3D>("RightLegCollision");
		HeadStartPosition = HeadMesh.Position;
		TorsoStartPosition = TorsoMesh.Position;
		LeftLegStartPosition = LeftLegMesh.Position;
		RightLegStartPosition = RightLegMesh.Position;
		HeadStartCollisionPosition = HeadCollision.Position;
		TorsoStartCollisionPosition = TorsoCollision.Position;
		LeftLegStartCollisionPosition = LeftLegCollision.Position;
		RightLegStartCollisionPosition = RightLegCollision.Position;
		ArmMovement = GetNode<Node3D>("ArmPivot/ArmMovement");
		ArmPivot = GetNode<Node3D>("ArmPivot");
		ArmPivotStart = ArmPivot.Position;
		GunPivot = GetNode<Node3D>("ArmPivot/ArmMovement/GunPivot");
		gunHolder = GetNode<Node3D>("ArmPivot/ArmMovement/GunPivot/GunHolder");
		
		AxisLockAngularX = true;
		AxisLockAngularY = false;
		AxisLockAngularZ = true;
		
		var globalData = GetNode<GlobalData>("/root/GlobalData");
		selectedGun = globalData.SelectedGunName;
		SetZoomValuesByGun(selectedGun);
		AddToGroup("Team");
		
		foreach (Node child in gunHolder.GetChildren())
		{
			child.QueueFree();
		}
		
		PackedScene gunScene = GD.Load<PackedScene>($"res://{selectedGun}.tscn");
		Node3D gunInstance = gunScene.Instantiate<Node3D>();
		gunHolder.AddChild(gunInstance);
		currentGun = gunInstance;
		BulletScene = GD.Load<PackedScene>("res://BulletTeam.tscn");
		
		switch (selectedGun)
		{
			case "Pistol":
				ammo = 12;
				break;
			case "Heavy":
				ammo = 100;
				break;
			case "Sniper":
				ammo = 2;
				break;
			case "Rifle1":
				ammo = 50;
				break;
			case "Rifle2":
				ammo = 36;
				break;
		}	
	}

	public override void _Input(InputEvent @event)
	{
		if (Input.IsActionPressed("Click") || @event is InputEventKey eventKey && eventKey.Pressed)
		{
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}
		if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			float sens = 0f;
			
			if (isZoomed == true)
			{
				sens = zoomSensitivity;
			}
			else if (isZoomed == false)
			{
				sens = MouseSensitivity;
			}

			accumulatedRotating -= mouseMotion.Relative.X * sens;
			pitch -= mouseMotion.Relative.Y * sens;
			pitch = Mathf.Clamp(pitch, MinPitch, MaxPitch);
			Vector3 camRot = Camera.Rotation;
			camRot.X = pitch;
			Camera.Rotation = camRot;
		}
		if (@event.IsActionPressed("Zoom"))
		{
			isZoomed = true;
		}

		if (@event.IsActionReleased("Zoom"))
		{
			isZoomed = false;
		}
		
		if (Input.IsActionPressed("esc"))
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}
		
		isCrouching = Input.IsActionPressed("Crouch");
	}
		
	public override void _PhysicsProcess(double delta)
	{
		isCrouching = Input.IsActionPressed("Crouch");
		
		if (Input.IsActionPressed("Click") && !Reloading)
		{
			Shoot();
		}
		
		AngularVelocity = Vector3.Zero;
		ShootCooldown -= delta;
		
		if(ShootCooldown<0)
		{
			ShootCooldown = 0;
		}
		
		if(ShootCooldown==0 && Reloading)
		{
			
			Reloading = false;
			gunSpinAngle = 0f;
			
			switch(selectedGun)
			{
				case "Pistol":
					ammo = 12;
					break;
				case "Heavy":
					ammo = 100;
					break;
				case "Sniper":
					ammo = 2;
					break;
				case "Rifle1":
					ammo = 50;
					break;
				case "Rifle2":
					ammo = 36;
					break;
			}
		}
		
		if (isSpinningGun && gunHolder != null)
		{
			float spinStep = gunSpinSpeed * (float)delta;
			gunSpinAngle += spinStep;

			if (gunSpinAngle >= totalSpinAmount)
			{
				isSpinningGun = false;
				GunPivot.Rotation = Vector3.Zero;
			}
			else
			{
				float angleRad = Mathf.DegToRad(spinStep);
				Vector3 spinAxis = Vector3.Forward;
				Basis spin = new Basis(spinAxis, angleRad);
				GunPivot.Basis = spin * GunPivot.Basis;
			}
		}
		
		if (LinearVelocity.Y > 0)
		{
			GravityScale = 1.5f;
		}
		else
		{
			GravityScale = 2f;
		}

		Node3D gun = gunHolder.GetChildOrNull<Node3D>(0);
		var aimPoint = gun.GetNodeOrNull<Marker3D>("AimPosition");
		
		if (aimPoint != null)
		{
			Vector3 worldTargetPos = aimPoint.GlobalPosition;
			Vector3 localTargetPos = Camera.GetParent<Node3D>().ToLocal(worldTargetPos);
			aimCamPos = localTargetPos;
		}

		Vector3 targetPos;

		if (isZoomed)
		{
			targetPos = aimCamPos;
		}
		else
		{
			targetPos = normalCamPos;
		}
		
		float currentZoomSpeed;

		if (isZoomed)
		{
			currentZoomSpeed = zoomSpeed * 0.4f;
		}
		else
		{
			currentZoomSpeed = zoomSpeed;
		}

		Camera.Position = Camera.Position.Lerp(targetPos, currentZoomSpeed * (float)delta);
		float targetFov;

		if (isZoomed)
		{
			targetFov = zoomFov;
		}
		else
		{
			targetFov = normalFov;
		}
		
		Camera.Fov = Mathf.Lerp(Camera.Fov, targetFov, zoomSpeed * (float)delta);
	}
	
	public override void _IntegrateForces(PhysicsDirectBodyState3D state)
	{
		rotating += accumulatedRotating;
		accumulatedRotating = 0f;
		Vector3 rotation = Rotation;
		rotation.Y = rotating;
		Rotation = rotation;
		Vector3 velocity = Vector3.Zero;

		if (Input.IsActionPressed("MoveRight"))
		{
			velocity.X += 1;
		}
		if (Input.IsActionPressed("MoveLeft"))
		{
			velocity.X -= 1;
		}
		if (Input.IsActionPressed("MoveBack"))
		{
			velocity.Z += 1;
		}
		if (Input.IsActionPressed("MoveForward"))
		{
			velocity.Z -= 1;
		}

		velocity = velocity.Normalized();
		Basis basis = GlobalTransform.Basis;
		Vector3 forward = basis.Z;
		Vector3 right = basis.X;
		Vector3 moveDirection = (right * velocity.X + forward * velocity.Z).Normalized();
		float currentSpeed;

		if (isCrouching)
		{
			currentSpeed = Speed * crouchSpeed;
		}
		else
		{
			currentSpeed = Speed;
		}
		
		Vector3 finalVelocity = new Vector3(moveDirection.X * currentSpeed, LinearVelocity.Y, moveDirection.Z * currentSpeed);
		
		if (Input.IsActionJustPressed("Jump") && GroundCheck.IsColliding())
		{
			finalVelocity.Y = Jump;
		}

		if (Cam != null)
		{
			Vector3 targetCamPos;

			if (isCrouching)
			{
				targetCamPos = new Vector3(0, 0.5f, 0);
			}
			else
			{
				targetCamPos = new Vector3(0, 1f, 0);
			}
			
			Cam.Position = Cam.Position.Lerp(targetCamPos, 10 * state.Step);
		}
		
		Vector3 offset;
		Vector3 lefttargetRotation;
		Vector3 righttargetRotation;

		if (isCrouching)
		{
			offset = crouchOffset;
			lefttargetRotation = leftcrouchLegRotation;
			righttargetRotation = rightcrouchLegRotation;
		}
		else
		{
			offset = Vector3.Zero;
			lefttargetRotation = Vector3.Zero;
			righttargetRotation = Vector3.Zero;
		}


		if (ArmPivot != null)
		{
			Vector3 targetArmRotation = new Vector3(pitch, 0, 0);
			ArmPivot.Rotation = ArmPivot.Rotation.Lerp(targetArmRotation, 10f * (float)state.Step);
			ArmPivot.Position = ArmPivot.Position.Lerp(ArmPivotStart + offset, 10f * (float)state.Step);
		}

		if (HeadMesh != null)
		{
			HeadMesh.Position = HeadMesh.Position.Lerp(HeadStartPosition + offset, 10f * (float)state.Step);
		}
		if (HeadCollision != null)
		{
			HeadCollision.Position = HeadCollision.Position.Lerp(HeadStartCollisionPosition + offset, 10f * (float)state.Step);
		}
		if (TorsoMesh != null)
		{
			TorsoMesh.Position = TorsoMesh.Position.Lerp(TorsoStartPosition + offset, 10f * (float)state.Step);
		}
		if (TorsoCollision != null)
		{
			TorsoCollision.Position = TorsoCollision.Position.Lerp(TorsoStartCollisionPosition + offset, 10f * (float)state.Step);
		}
		if (LeftLegMesh != null)
		{
			LeftLegMesh.Position = LeftLegMesh.Position.Lerp(LeftLegStartPosition + (isCrouching ? (new Vector3(0, 0.1f, -0.3f)) : Vector3.Zero), 10f * (float)state.Step);
		}
		if (LeftLegCollision != null)
		{
			LeftLegCollision.Position = LeftLegCollision.Position.Lerp(LeftLegStartCollisionPosition + (isCrouching ? (new Vector3(0, 0.1f, -0.3f)) : Vector3.Zero), 10f * (float)state.Step);
		}
		if (RightLegMesh != null)
		{
			RightLegMesh.Position = RightLegMesh.Position.Lerp(RightLegStartPosition + (isCrouching ? (new Vector3(0, -0.3f, 0.3f)) : Vector3.Zero), 10f * (float)state.Step);
		}
		if (RightLegCollision != null)
		{
			RightLegCollision.Position = RightLegCollision.Position.Lerp(RightLegStartCollisionPosition + (isCrouching ? (new Vector3(0, -0.3f, 0.3f)) : Vector3.Zero), 10f * (float)state.Step);
		}
		if (LeftLegMesh != null)
		{
			LeftLegMesh.Rotation = LeftLegMesh.Rotation.Lerp(lefttargetRotation, 10f * (float)state.Step);
		}
		if (LeftLegCollision != null)
		{
		LeftLegCollision.Rotation = LeftLegCollision.Rotation.Lerp(lefttargetRotation, 10f * (float)state.Step);
		}
		if (RightLegMesh != null)
		{
			RightLegMesh.Rotation = RightLegMesh.Rotation.Lerp(righttargetRotation, 10f * (float)state.Step);
		}
		if (RightLegCollision != null)
		{
		RightLegCollision.Rotation = RightLegCollision.Rotation.Lerp(righttargetRotation, 10f * (float)state.Step);
		}
		
		LinearVelocity = finalVelocity;
	}
		
	private void SetZoomValuesByGun(string gun)
	{
		switch (gun)
		{
			case "Sniper":
				zoomFov = 5f;
				break;
			case "Rifle1":
				zoomFov = 40f;
				break;
			case "Rifle2":
				zoomFov = 40f;
				break;
			case "Heavy":
				zoomFov = 60f;
				break;
			case "Pistol":
				zoomFov = 50f;
				break;
			default:
				zoomFov = 50f;
				break;
		}
	}
	
	private void Shoot()
	{
		if (ShootCooldown <= 0 && ammo > 0)
		{
			ammo--;
			var bulletHole = gunHolder.GetChild(0).GetNode<Marker3D>("BulletHole");
			var aimRay = Camera.GetNode<RayCast3D>("aimRay");
			Vector3 targetPos;
			
			if (aimRay.IsColliding())
			{
				targetPos = aimRay.GetCollisionPoint();
			}
			else
			{
				targetPos = Camera.GlobalPosition + -Camera.GlobalTransform.Basis.Z * 1000f;
			}

			bulletHole.LookAt(targetPos, Vector3.Up);
			BulletTeam bulletInstance = BulletScene.Instantiate<BulletTeam>();
			GetNode("Bullets").AddChild(bulletInstance);
			bulletInstance.GlobalTransform = bulletHole.GlobalTransform;
			bulletInstance.SetGunType(selectedGun);
			bulletInstance.Direction = (targetPos - bulletHole.GlobalPosition).Normalized();
			bulletInstance.TeamShooter = this; 

			switch(selectedGun)
			{
				case "Pistol":
					ShootCooldown = 0.2;
					break;
				case "Heavy":
					ShootCooldown = 0.15;
					break;
				case "Sniper":
					ShootCooldown = 1;
					break;
				case "Rifle1":
					ShootCooldown = 0.3;
					break;
				case "Rifle2":
					ShootCooldown =0.25;
					break;
			}
		}
		else if (ShootCooldown <= 0)
		{
			Reload();
		}
	}
		
	private void Reload()
	{
		isZoomed = false;
		SetZoomValuesByGun(selectedGun);
		Reloading = true;
		isSpinningGun = true;
		gunSpinAngle = 0f;
		
		switch (selectedGun)
		{
			case "Pistol":
				ShootCooldown=2;
				break;
			case "Heavy":
				ShootCooldown=4;
				break;
			case "Sniper":
				ShootCooldown=4;
				break;
			case "Rifle1":
				ShootCooldown=3;
				break;
			case "Rifle2":
				ShootCooldown=2;
				break;	
		}
		
		totalSpinAmount = 360f;
		gunSpinSpeed = totalSpinAmount / (float)ShootCooldown;
	}
	
	public void TakeDamage(float amount)
	{
		CurrentHealth = Mathf.Max(CurrentHealth - (int)amount, 0);
		HealthBar.Value = CurrentHealth;

		if (CurrentHealth <= 0)
		{
			Die();
			KillManager.Instance.AddEnemyKill();
		}
	}

	private void Die()
	{
		Input.MouseMode = Input.MouseModeEnum.Visible;
		SetProcess(false);
		SetPhysicsProcess(false);
		var respawnScene = GD.Load<PackedScene>("res://respawn.tscn");
		Control respawnUI = respawnScene.Instantiate<Control>();
		Map.Instance.AddChild(respawnUI);
		Map.Instance.ShowKillManagerUI(false);
		Map.Instance.ShowCaptureManagerUI(false);
		CallDeferred(nameof(DeferredDie));
	}

	private void DeferredDie()
	{
		QueueFree();
	}
}
