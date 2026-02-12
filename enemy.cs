using Godot;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public partial class enemy : RigidBody3D
{
	//enum for each enemy state during the game
	private enum EnemyState
	{
		GoingToCapturePoint,
		InCombat,
		HuntingTeam,
		GuardingCapturePoint
	}

	private EnemyState currentState;
	
	private int MaxHealth = 100;
	private int CurrentHealth;
	private Marker3D spawnMarker;
	private string gunName;
	private NavigationAgent3D MovementEnemy;
	private float moveSpeed = 7.5f;
	private Area3D EnemyDetection;
	private Node3D currentTarget;
	private float detectionRange = 1f;
	private float rotationSpeed = 6.5f;
	private float lookRotationSpeed = 5f;
	private const float ClosingSpeedMultiplier = 0.5f;  
	private const float RandomMoveSpeedMultiplier = 1.0f;
	private const float Circling = 0.6f;
	private const float CirclingBackwards = 0.8f;
	private float DirectionSign = 1f;
	private float CirclingSwitchTimer = 0f;
	private const float CirclingSwitchInterval = 1.5f;
	private int ammoAmount;
	public int currentAmmo;
	private float reloadTime;
	private float fireRate;
	private bool isReloading = false;
	private bool isShooting = false;
	private Node3D gunHolder;
	private PackedScene bulletEnemyScene;
	private float minDistance;
	private const float DistanceAway = 2.0f;
	private bool reactingToBullet = false;
	private float bulletLookTimer = 0f;
	private const float BulletLookDuration = 2f;
	private Node3D bulletLookTarget = null;
	private float bulletDodgeTimer = 0f;
	private const float BulletDodgeDuration = 1.2f;
	private float ragdollTime = 3.0f;
	private bool isDead = false;
	public event Action<Marker3D, string> EnemyDied;
	private Node3D assignedCapturePoint;
	private Node3D lastCapturePoint;
	private bool captureCompleted = false;
	private Vector3 captureOffset;  
	private RandomNumberGenerator rng = new RandomNumberGenerator();
	private Vector3 combatMoveDirection = Vector3.Zero;
	private float combatMoveTimer = 0f;
	private float combatMoveInterval = 0.3f;
	private RayCast3D EnemySight;
	private float huntTimer = 0f;
	private const float MaxHuntTime = 10f;
	private bool sightTemporarilyDisabled = false;
	private float sightDisableTimer = 0f;
	private const float SightDisableDuration = 11f;
	private float guardStayTimer = 0f;
	private const float GuardStayRequired = 15f;
	private bool decidedToStay = false;
	private bool decidedToLeave = false; 
	private Vector3 guardTarget = Vector3.Zero;
	private float guardRepathTimer = 0f;
	private const float GuardRepathInterval = 2.5f; 
	
	public override void _Ready()
	{
		Sleeping = false;
		CanSleep = false;
		CurrentHealth = MaxHealth;
		AxisLockAngularX = true;
   		AxisLockAngularZ = true;
		MovementEnemy = GetNode<NavigationAgent3D>("MovementEnemy");
		EnemyDetection = GetNode<Area3D>("EnemyDetection"); 
		gunHolder = GetNode<Node3D>("ArmPivot/ArmMovement/GunPivot/GunHolder");
		bulletEnemyScene = GD.Load<PackedScene>("res://BulletEnemy.tscn");
		EnemySight = GetNode<RayCast3D>("ArmPivot/ArmMovement/GunPivot/EnemySight");
		AddToGroup("Enemy");
		rng.Randomize();
		PickRandomCapturePoint();
		currentState = EnemyState.GoingToCapturePoint; 
		EnemyDetection.BodyEntered += OnEnemyDetectionBodyEntered;
	}
	//changes the attributes of the gun 
	private void SetGunStats(string gun)
	{
		switch (gun)
		{
			case "Pistol":
				detectionRange = 30f;
				minDistance = 20f;
				ammoAmount = 12;
				reloadTime = 2f;
				fireRate = 0.3f;
				break;

			case "Rifle1":
				detectionRange = 40f;
				minDistance = 30f;
				ammoAmount = 50;
				reloadTime = 3f;
				fireRate = 0.5f;
				break;

			case "Rifle2":
				detectionRange = 40f;
				minDistance = 35f;
				ammoAmount = 36;
				reloadTime = 2f;
				fireRate = 0.3f;
				break;

			case "Heavy":
				detectionRange = 30f;
				minDistance = 25f;
				ammoAmount = 100;
				reloadTime = 4f;
				fireRate = 0.2f;
				break;

			case "Sniper":
				detectionRange = 65f;
				minDistance = 60f;
				ammoAmount = 2;
				reloadTime = 4f;
				fireRate = 2.5f;
				break;
			default:
				detectionRange = 1f;
				minDistance = 1f;
				ammoAmount = 0;
				reloadTime = 0f;
				fireRate = 0f;
				break;
		}
		currentAmmo = ammoAmount;
	}
	
	//if team is inside detection range, they are going to combat state
	private bool IsCloseCombat()
	{
		return IsTeamInDetectionRange();
	}
	
	//checks whether team is inside the raycast of enemy sight
	private void UpdateEnemySight()
	{ 
		if (sightTemporarilyDisabled)
		{
			EnemySight.Enabled = false;
			return;
		}

		if (reactingToBullet)
		{
			EnemySight.Enabled = true;
			return;
		}
		
		EnemySight.Enabled = !IsCloseCombat();
	}
	
	//disable raycast temporarily
	private void DisableSightTemporarily()
	{
		sightTemporarilyDisabled = true;
		sightDisableTimer = SightDisableDuration;
		EnemySight.Enabled = false;
	}
	
	//when spawned, the detection range is changed depending on gun type
	private void UpdateDetectionRange()
	{
		var shapeNode = EnemyDetection.GetNode<CollisionShape3D>("CollisionShape3D");

		if (shapeNode.Shape is SphereShape3D sphere)
		{
			SphereShape3D newSphere = new SphereShape3D();
			newSphere.Radius = detectionRange;
			shapeNode.Shape = newSphere;
		}
	}
	
	//used to move enemy towards their target
	private Vector3 GetCombatMoveDirection(Vector3 targetPosition, double delta)
	{
		Vector3 toTarget = (targetPosition - GlobalPosition).Normalized();
		Vector3 awayFromTarget = -toTarget;
		Vector3 randomDirection = GetSmoothCombatDirection(delta).Normalized();
		float distance = GlobalPosition.DistanceTo(targetPosition);
		Vector3 finalDirection;

		if (distance < minDistance)
		{ 
			finalDirection = (awayFromTarget * 0.7f) + (randomDirection * 0.6f);
		}
		else if (distance > minDistance + DistanceAway)
		{ 
			finalDirection = (toTarget * 0.6f) + (randomDirection * 0.7f);
		}
		else
		{ 
			finalDirection = randomDirection;
		}

		finalDirection.Y = 0;
		return finalDirection.Normalized();
	}
	
	//when team is too close, use this method to randomly move left/right and move out the way
	private float GetStrafeSign(double delta)
	{
		CirclingSwitchTimer -= (float)delta;

		if (CirclingSwitchTimer <= 0f)
		{
			CirclingSwitchTimer = CirclingSwitchInterval;
			if (rng.Randf() < 0.5f)
			{
				DirectionSign = -1f;  
			}
			else
			{
				DirectionSign = 1f; 
			}
		}

		return DirectionSign;
	}
	
	//used to randomly select capture point and return it to enemy, which can then be stored to them
	private void PickRandomCapturePoint()
	{
		var cps = GetTree().GetNodesInGroup("CapturePoint").OfType<Node3D>().ToList();

		if (cps.Count == 0)
		{
			return;
		}

		if (lastCapturePoint != null && cps.Count > 1)
		{
			cps.Remove(lastCapturePoint);
		}

		int index = rng.RandiRange(0, cps.Count - 1);
		assignedCapturePoint = cps[index];
		lastCapturePoint = assignedCapturePoint;
		captureOffset = new Vector3(rng.RandfRange(-4f, 4f), 0, rng.RandfRange(-4f, 4f));
		captureCompleted = false;
	}
	
	//checks if capture is owned by enemy
	private bool IsCaptureOwnedByEnemy()
	{
		if (assignedCapturePoint is CapturePoint cp)
		{
			return cp.Owner == CapturePoint.OwnerType.Enemy;
		}

		return false;
	}
	
	public override void _PhysicsProcess(double delta)
	{	
		//change behaviour of enemy when health is low
		if (CurrentHealth < 26)
		{
			ForceLowHealthBehavior(delta);
			return;
		}
		
		//if disabled, this is countdown to when it can be re enabled 
		if (sightTemporarilyDisabled)
		{
			sightDisableTimer -= (float)delta;
			if (sightDisableTimer <= 0f)
			{
				sightTemporarilyDisabled = false;
			}
		}
		
		//handles the behaviour of enemy
		HandleRotation(delta);   
		HandleMovement(delta);   
		UpdateEnemySight();
		Shooting();
		
		//changes state when inside capture point
		if (currentState == EnemyState.GuardingCapturePoint && IsTeamInDetectionRange())
		{
			UpdateTarget();
			ChangeState(EnemyState.InCombat);
		}
	}
	
	//force them to behave different when health is low
	private void ForceLowHealthBehavior(double delta)
	{ 
		if (currentState != EnemyState.GoingToCapturePoint)
		{
			ChangeState(EnemyState.GoingToCapturePoint); 
		}
 
		HandleRotation(delta);
		State_GoingToCapturePoint(delta);
		UpdateEnemySight();
		Shooting();
	}
	
	//handles how they look during the game
	private void HandleRotation(double delta)
	{  
		//low health
		if (CurrentHealth<26)
		{
			if (IsTeamInDetectionRange())
			{
				UpdateTarget();
				LookAtTargetOrDirection(delta);
			}
			else
			{
				LookMovement(delta);
			}
			return;
		}
 		
		//if bullet enters detection range, they look at team who shot it
		if (reactingToBullet)
		{
			if (CanSeeTeam(out Node3D seenTarget))
			{
				reactingToBullet = false;
				bulletLookTarget = null;
				bulletLookTimer = 0f;
				currentTarget = seenTarget;
				ChangeState(EnemyState.InCombat);
				LookAtTargetOrDirection(delta);
				return;
			}

			if (bulletLookTarget != null && IsInstanceValid(bulletLookTarget))
			{
				Node3D previousTarget = currentTarget;
				currentTarget = bulletLookTarget;
				LookAtTargetOrDirection(delta);
				currentTarget = previousTarget;
			}

			bulletLookTimer -= (float)delta;
			
			if (bulletLookTimer <= 0f)
			{
				reactingToBullet = false;
				bulletLookTarget = null;
			}
			return;
		}
 	
		//if raycast hits team, they look in their direction
		if (CanSeeTeam(out Node3D seen))
		{
			if (currentTarget == null)
			{
				currentTarget = seen;
			}
			
			LookAtTargetOrDirection(delta);
			return;
		}
 		
		//behaviour for different health
		if (CurrentHealth>25 && CurrentHealth<=50)
		{
			if (IsTeamInDetectionRange())
			{ 
				UpdateTarget();
				LookAtTargetOrDirection(delta);
			}
			else
			{
				LookMovement(delta);
			}
			return;
		}
 
		if (currentTarget != null && IsInstanceValid(currentTarget))
		{
			LookAtTargetOrDirection(delta);
			return;
		}

		LookMovement(delta);
	}
	
	//handles where they move depending on current state
	private void HandleMovement(double delta)
	{  
		switch (currentState)
		{
			case EnemyState.GoingToCapturePoint:
				State_GoingToCapturePoint(delta);
				break;

			case EnemyState.InCombat:
				State_InCombat(delta);
				break;

			case EnemyState.HuntingTeam:
				State_HuntingTeam(delta);
				break;

			case EnemyState.GuardingCapturePoint:
				State_GuardingCapturePoint(delta);
				break;
		}
	}
	
	//changes state depending on what has happened inside the game
	private void ChangeState(EnemyState newState)
	{
		if (currentState == newState) 
		{
			return;
		}
		
		if (CurrentHealth<26 && newState != EnemyState.GoingToCapturePoint)
		{
			return;
		}
			
		if (newState == EnemyState.GoingToCapturePoint)
		{
			currentTarget = null;
		}
 		
		if (newState == EnemyState.InCombat)
		{
			sightTemporarilyDisabled = false;
			sightDisableTimer = 0f;
		}
		
		guardStayTimer = 0f;
		decidedToStay = false;
		decidedToLeave = false;
		guardTarget = Vector3.Zero;
		guardRepathTimer = 0f;

		if (newState == EnemyState.HuntingTeam)
		{
			huntTimer = MaxHuntTime;
		}

		currentState = newState;
	}
	
	//used to move enemy
	private void MoveUsingNavigation(Vector3 target, double delta)
	{  
		if (bulletDodgeTimer > 0f && IsChasingViaSight())
		{
			bulletDodgeTimer -= (float)delta;
			Vector3 dodgeDirection = GetSmoothCombatDirection(delta);
			dodgeDirection.Y = 0;
			ApplyMovement(dodgeDirection.Normalized(), moveSpeed);
			return;
		}
 
		if (MovementEnemy == null)
		{
			return;
		}

		MovementEnemy.TargetPosition = target;
		Vector3 next = MovementEnemy.GetNextPathPosition();
		Vector3 direction = (next - GlobalPosition);
		direction.Y = 0;
		
		//apply to the direction where they are going
		if (direction.LengthSquared() > 0.0001f)
		{
			ApplyMovement(direction.Normalized(), moveSpeed);
		}
	}
	
	//adds velocity to the direction
	private void ApplyMovement(Vector3 desiredDirection, float speed)
	{
		Vector3 velocity = LinearVelocity;
		Vector3 desiredVelocity = desiredDirection * speed;
		velocity.X = desiredVelocity.X;
		velocity.Z = desiredVelocity.Z; 
		LinearVelocity = velocity;
	}
	
	//move in a random direction for a short time, then switch directions smoothly
	private Vector3 GetSmoothCombatDirection(double delta)
	{
		combatMoveTimer -= (float)delta;

		if (combatMoveTimer <= 0f || combatMoveDirection == Vector3.Zero)
		{
			combatMoveTimer = combatMoveInterval;
			Vector3[] Directions = {Transform.Basis.X, -Transform.Basis.X, -Transform.Basis.Z, Transform.Basis.Z};
			combatMoveDirection = Directions[rng.RandiRange(0, Directions.Length - 1)];
		}

		return combatMoveDirection;
	}
	
	//they are fighting
	private void State_InCombat(double delta)
	{
		UpdateTarget(); 
		//checks which state they are meant to be in
		if (currentTarget == null || !IsInstanceValid(currentTarget))
		{
			if (CurrentHealth > 50)
			{
				ChangeState(EnemyState.HuntingTeam);
			}
			else
			{
				ChangeState(EnemyState.GoingToCapturePoint);
			}
			return;
		}
		//checks how far they are away from team
		float distance = GlobalPosition.DistanceTo(currentTarget.GlobalPosition);

		if (distance > detectionRange)
		{
			if (CurrentHealth > 50)
			{
				ChangeState(EnemyState.HuntingTeam);
			}
			else
			{
				ChangeState(EnemyState.GoingToCapturePoint);
			}
			return;
		}
  
		Vector3 combatDirection = GetCombatMoveDirection(currentTarget.GlobalPosition, delta);
		float speedMultiplier = 1f;
		//changes speed of movement depending on gun
		if (gunName == "Sniper")
		{
			speedMultiplier = 0.75f;
		}
		else if (gunName == "Heavy")
		{
			speedMultiplier = 0.5f;
		}
 
		float idealMin = minDistance;
		float idealMax = minDistance + DistanceAway;
		Vector3 finalMoveDirection = combatDirection;
 		//make them go to the ideal minimum distance
		if (distance < idealMin)
		{
			Vector3 toTarget = (currentTarget.GlobalPosition - GlobalPosition).Normalized();
			Vector3 away = -toTarget;
			Vector3 strafe = toTarget.Cross(Vector3.Up).Normalized();
			strafe *= GetStrafeSign(delta);
			finalMoveDirection = (away * CirclingBackwards) + (strafe * Circling);
			finalMoveDirection = finalMoveDirection.Normalized();
		}
		//make sure they are not too far away
		else if (distance > idealMax)
		{
			Vector3 desiredPosition = currentTarget.GlobalPosition + (GlobalPosition - currentTarget.GlobalPosition).Normalized() * minDistance;
			Vector3 toDesired = desiredPosition - GlobalPosition;
			
			if (toDesired.LengthSquared() > 0.0001f)
			{
				Vector3 toDesiredDirection = toDesired.Normalized();
				finalMoveDirection = (toDesiredDirection * 0.7f) + (combatDirection * 0.6f);
			}
		}
 
		finalMoveDirection.Y = 0;
		if (finalMoveDirection.LengthSquared() < 0.0001f)
		{
			finalMoveDirection = -GlobalTransform.Basis.Z;
		}

		float finalSpeed = moveSpeed;
 		
		//if team not within detection range, go towards them
		if (!IsTeamInDetectionRange())
		{ 
			finalSpeed = moveSpeed;
		}
		else
		{
			Vector3 toTargetDirection = (currentTarget.GlobalPosition - GlobalPosition).Normalized();
			float towardTarget = finalMoveDirection.Dot(toTargetDirection);

			if (towardTarget > 0.5f)
			{
				finalSpeed *= ClosingSpeedMultiplier;
			}
			else
			{
				finalSpeed *= RandomMoveSpeedMultiplier;
			}
		}

		ApplyMovement(finalMoveDirection.Normalized(), finalSpeed);
	}

	//when assigned to capture point, they move towards it
	private void State_GoingToCapturePoint(double delta)
	{
		if (assignedCapturePoint == null)
		{
			return;
		}
 		
		//if raycast hits team, change state to combat
		if (ShootDecider(out Node3D target))
		{
			currentTarget = target;
 
			if (IsTeamInDetectionRange() && CurrentHealth >= 26)
			{
				ChangeState(EnemyState.InCombat);
				return;
			}

			if (CurrentHealth >= 26)
			{
				MoveUsingNavigation(currentTarget.GlobalPosition, delta);
			}
			else
			{
				MoveUsingNavigation(assignedCapturePoint.GlobalPosition + captureOffset, delta);
			}
			
			Shooting();
			return;
		}
 		//used to move towards capture point
		Vector3 targetPosition = assignedCapturePoint.GlobalPosition + captureOffset;
		MoveUsingNavigation(targetPosition, delta);

		if (IsInsideCapturePoint())
		{
			ChangeState(EnemyState.GuardingCapturePoint);
		}
	}
	
	//when team has left detection range, what state they are meant to be in
	private void State_HuntingTeam(double delta)
	{ 	
		//goes back to capture point
		if (currentTarget == null || !IsInstanceValid(currentTarget))
		{
			ChangeState(EnemyState.GoingToCapturePoint);
			return;
		}
 		
		//if team re enters detection range, back to combat state
		if (IsTeamInDetectionRange())
		{
			ChangeState(EnemyState.InCombat);
			return;
		}
 		
		//hunt team if within certain health
		MoveUsingNavigation(currentTarget.GlobalPosition, delta);
		huntTimer -= (float)delta;
		
		if (huntTimer <= 0f)
		{
			DisableSightTemporarily();
			currentTarget = null;
			ChangeState(EnemyState.GoingToCapturePoint);
		}
	}
	
	//when enemy is insde capture point, what they do inside
	private void State_GuardingCapturePoint(double delta)
	{ 
		//change state to combat if team is nearby
		if (IsTeamInDetectionRange())
		{
			ChangeState(EnemyState.InCombat);
			return;
		}
 		
		//if raycast hits team, shoot, but dont move
		if (ShootDecider(out Node3D target))
		{
			currentTarget = target;
			ApplyMovement(Vector3.Zero, 0f);  
			return;
		}
 		
		if (IsCloseCombat())
		{
			UpdateTarget();
			Shooting();
		}
 		
		//get a new point inside capture point to randomly move towards
		guardRepathTimer -= (float)delta;
		bool needNewPoint = guardTarget == Vector3.Zero || guardRepathTimer <= 0f || GlobalPosition.DistanceTo(guardTarget) < 1f;

		if (needNewPoint)
		{
			guardTarget = PickRandomPointInCapturePoint();
			guardRepathTimer = GuardRepathInterval;
		}
 
		MoveUsingNavigation(guardTarget, delta);
 
		if (!IsCaptureOwnedByEnemy())
		{
			return;
		}
		//randomly decide if they should stay or leave
		if (!decidedToStay && !decidedToLeave)
		{
			if (rng.Randf() < 0.5f)
			{
				decidedToStay = true;
				guardStayTimer = 0f;
			}
			else
			{
				decidedToLeave = true;
				ForceLeaveCapturePoint();
				return;
			}
		}
		
		//must wait 15seconds before leaving
		if (decidedToStay)
		{
			guardStayTimer += (float)delta;

			if (guardStayTimer >= GuardStayRequired)
			{
				ForceLeaveCapturePoint();
				return;
			}
		}
	} 
	
	//forced to leave capture point
	private void ForceLeaveCapturePoint()
	{
		PickRandomCapturePoint();
		captureCompleted = false;
		ChangeState(EnemyState.GoingToCapturePoint);
	}
	
	//checks whether capture point is owned by them
	private bool IsAssignedCaptureAlreadyOwned()
	{
		if (assignedCapturePoint is CapturePoint cp)
		{
			return cp.Owner == CapturePoint.OwnerType.Enemy;
		}

		return false;
	}
	
	//what to do when team is inside detection range
	private bool IsTeamInDetectionRange()
	{
		//checks all the team inside in order
		foreach (var body in EnemyDetection.GetOverlappingBodies())
		{
			if (body is Node node && node.IsInGroup("Team"))
			{
				return true;
			}
		}
		return false;
	}
	
	private void OnEnemyDetectionBodyEntered(Node body)
	{
		//dont react to bullet if another bullet is inside
		if (reactingToBullet)
		{
			return;
		}
 		
		//makes enemy look at the shooter
		if (body is BulletTeam bullet)
		{
			if (bullet.TeamShooter is Node3D shooter && IsInstanceValid(shooter))
			{
				reactingToBullet = true;
				bulletLookTimer = BulletLookDuration;
				bulletLookTarget = shooter;
				bulletDodgeTimer = BulletDodgeDuration;
			}
		}
	}
	
	//if raycast hits team, they temporarily move towards team
	private bool IsChasingViaSight()
	{
		return currentTarget != null && IsInstanceValid(currentTarget) && !IsTeamInDetectionRange()	&& CanSeeTeam(out _);
	}
	 
	//checks if they are inside capture point
	private bool IsInsideCapturePoint()
	{
		if (assignedCapturePoint == null)
		{
			return false;
		}

		Area3D area = assignedCapturePoint.GetNodeOrNull<Area3D>("CaptureArea");
		if (area == null)
		{
			return false;
		}
		//checks if they are inside
		return area.GetOverlappingBodies().Contains(this);
	}

	//look in the direction they are moving at
	private void LookMovement(double delta) 
	{ 
		Vector3 lookDirection = Vector3.Zero; 
		Vector3 velocity = LinearVelocity; 
		velocity.Y = 0; 
		
		if (velocity.LengthSquared() > 0.5f) 
		{ 
			lookDirection = velocity.Normalized(); 
		} 
		else if (MovementEnemy != null && MovementEnemy.IsNavigationFinished() == false) 
		{ 
			Vector3 next = MovementEnemy.GetNextPathPosition(); 
			Vector3 dir = next - GlobalPosition; dir.Y = 0; 
			
			if (dir.LengthSquared() > 0.001f) 
			{
				lookDirection = dir.Normalized(); 
			}
		} 
		else 
		{ 
			return; 
		} 
		
		Vector3 currentDirection = -GlobalTransform.Basis.Z; 
		currentDirection.Y = 0; 
		currentDirection = currentDirection.Normalized(); 
		float angle = Mathf.Atan2(lookDirection.X, lookDirection.Z) - Mathf.Atan2(currentDirection.X, currentDirection.Z); 
		angle = Mathf.Wrap(angle, -Mathf.Pi, Mathf.Pi); 
		float rotateStep = rotationSpeed; 
		angle = Mathf.Clamp(angle, -rotateStep, rotateStep); 
		RotateY(angle); 
	}
	
	//randomly assings a capture point to team
	private Vector3 PickRandomPointInCapturePoint()
	{
		if (assignedCapturePoint == null)
		{
			return GlobalPosition;
		}

		Vector3 center = assignedCapturePoint.GlobalPosition;
		Vector3 Direction = new Vector3(rng.RandfRange(-1f, 1f), 0, rng.RandfRange(-1f, 1f)).Normalized();
		float distance = rng.RandfRange(1f, 4f);
		Vector3 point = center + Direction * distance;
		return point;
	}
	
	//updates target to the first team that interacts with them
	private void UpdateTarget()
	{
		if (currentTarget != null && IsInstanceValid(currentTarget) && IsTeamInDetectionRange())
		{
			return;
		}

		var bodies = EnemyDetection.GetOverlappingBodies();
		Node3D nearest = null;
		float nearestDist = float.MaxValue;
		//checks all team inside
		foreach (var body in bodies)
		{
			if (body is Node3D node && node.IsInGroup("Team"))
			{
				float dist = GlobalPosition.DistanceTo(node.GlobalPosition);
				if (dist < nearestDist)
				{
					nearestDist = dist;
					nearest = node;
				}
			}
		}

		if (nearest != null)
		{
			currentTarget = nearest;
		}
	}
	//looks at team
	private void LookAtTargetOrDirection(double delta) 
	{ 
		if (currentTarget == null || !IsInstanceValid(currentTarget)) 
		{
			return; 
		}
		
		Node3D armPivot = GetNodeOrNull<Node3D>("ArmPivot"); 
		Vector3 targetCenter = GetTargetCenter(currentTarget);
		Vector3 toTarget = targetCenter - GlobalPosition;
		
		if (toTarget.LengthSquared() < 0.001f) 
		{
			return; 
		}
		
		Vector3 FaceDirection = toTarget; 
		FaceDirection.Y = 0; 
		
		if (FaceDirection.LengthSquared() > 0.001f) 
		{ 
			FaceDirection = FaceDirection.Normalized(); 
			Vector3 currentDirection = -GlobalTransform.Basis.Z; 
			currentDirection.Y = 0; 
			currentDirection = currentDirection.Normalized(); 
			float angle = Mathf.Atan2(FaceDirection.X, FaceDirection.Z) - Mathf.Atan2(currentDirection.X, currentDirection.Z); 
			angle = Mathf.Wrap(angle, -Mathf.Pi, Mathf.Pi); 
			float rotateStep = lookRotationSpeed; 
			angle = Mathf.Clamp(angle, -rotateStep, rotateStep); 
			RotateY(angle); 
		} 
		
		float targetDistance = new Vector2(toTarget.X, toTarget.Z).Length(); 
		float pitch = Mathf.Atan2(toTarget.Y, targetDistance); 
		pitch = Mathf.Clamp(pitch, -Mathf.DegToRad(45f), Mathf.DegToRad(45f)); 
		Vector3 armRot = armPivot.Rotation; armRot.X = Mathf.Lerp(armRot.X, pitch, 6f * (float)delta); 
		armPivot.Rotation = armRot;  
	}
	//tells team where to aim
	private Vector3 GetTargetCenter(Node3D target)
	{
		Marker3D aim = target.GetNodeOrNull<Marker3D>("AimMarker");
		
		if (aim != null)
		{
			return aim.GlobalPosition;
		}

		return target.GlobalPosition;
	} 
	//checks whether team is inside raycast
	private bool CanSeeTeam(out Node3D seenTarget)
	{
		seenTarget = null;

		if (!EnemySight.Enabled || !EnemySight.IsColliding())
		{
			return false;
		}

		if (EnemySight.GetCollider() is Node3D node && node.IsInGroup("Team"))
		{
			seenTarget = node;
			return true;
		}

		return false;
	}
	//if team is inside raycast, shoot
	private bool ShootDecider(out Node3D target)
	{
		target = null;
 
		foreach (var body in EnemyDetection.GetOverlappingBodies())
		{
			if (body is Node3D node && node.IsInGroup("Team"))
			{
				target = node;
				return true;
			}
		}
 
		if (!IsTeamInDetectionRange() && CanSeeTeam(out Node3D seen))
		{
			target = seen;
			return true;
		}

		return false;
	}
	//shooting logic, see if they can shoot or reload
	private async void Shooting()
	{
		if (isReloading || isShooting)
		{
			return;
		}
 
		if (currentTarget != null && IsInstanceValid(currentTarget))
		{
			float dist = GlobalPosition.DistanceTo(currentTarget.GlobalPosition);
			
			if (dist <= detectionRange || CanSeeTeam(out _))
			{
				await FireLogic();
				return;
			}
		}
 
		if (!ShootDecider(out Node3D target))
		{
			return;
		}

		currentTarget = target;
		await FireLogic();
	}
	
	//if ammo is sufficent, shoot. if not, reload
	private async Task FireLogic()
	{
		if (currentAmmo > 0)
		{
			isShooting = true;
			await Shoot();
			isShooting = false;
		}
		else
		{
			await Reload();
		}
	}
	//fires bullet in a straight line
	private async Task Shoot()
	{
		currentAmmo--;
		Node3D bulletEnemySpawn = gunHolder.GetNodeOrNull<Node3D>($"{gunName}/BulletHole");
		BulletEnemy bulletEnemyInstance = bulletEnemyScene.Instantiate<BulletEnemy>();
		bulletEnemyInstance.GlobalTransform = bulletEnemySpawn.GlobalTransform;
		bulletEnemyInstance.SetGunType(gunName);
		bulletEnemyInstance.EnemyShooter = this;
		Vector3 aimPosition = GetTargetCenter(currentTarget);
		Vector3 shootDirection = (aimPosition - bulletEnemySpawn.GlobalPosition).Normalized();
		bulletEnemyInstance.Direction = shootDirection;
		GetTree().CurrentScene.AddChild(bulletEnemyInstance);
		await ToSignal(GetTree().CreateTimer(fireRate), "timeout");
	}
	//tells enemy they are reloading
	private async Task Reload()
	{
		isReloading = true;
		await RotateGunWhileReloading(reloadTime);
		currentAmmo = ammoAmount;
		isReloading = false;
	}
	//rotates their gun when reloading
	private async Task RotateGunWhileReloading(float duration)
	{
		float elapsed = 0f;
		Node3D gunPivot = GetNodeOrNull<Node3D>("ArmPivot/ArmMovement/GunPivot");
		if (gunPivot == null)
		{
			return;
		}
		//speed in which they rotate
		float totalRotation = Mathf.Tau;
		float rotationSpeed = totalRotation / duration;

		while (elapsed < duration && IsInsideTree())
		{
			if (!IsInstanceValid(gunPivot))
			{
				break;
			}

			float delta = (float)GetProcessDeltaTime();
			elapsed += delta;
			gunPivot.RotateObjectLocal(Vector3.Forward, rotationSpeed * delta);

			if (IsInsideTree())
			{
				await ToSignal(GetTree(), "process_frame");
			}
			else
			{
				break;
			}
		}

		if (IsInstanceValid(gunPivot))
		{
			gunPivot.Rotation = Vector3.Zero;
		}
	}
	//where they spawn on map
	public void Initialize(Marker3D marker, string gun)
	{
		spawnMarker = marker;
		gunName = gun;
		SetGunStats(gunName);
		CallDeferred(nameof(UpdateDetectionRange));
		EquipGun(gunName);
	}
	//which gun is equipped to enemy
	private void EquipGun(string gun)
	{
		string gunPath = $"res://{gun}.tscn";
		PackedScene gunScene = GD.Load<PackedScene>(gunPath);
		Node3D gunHolder;
		gunHolder = GetNode<Node3D>("ArmPivot/ArmMovement/GunPivot/GunHolder");
		//removes unnecessary gun
		foreach (Node child in gunHolder.GetChildren())
		{
			gunHolder.RemoveChild(child);
			child.QueueFree();
		}

		Node3D gunInstance = gunScene.Instantiate<Node3D>();
		gunHolder.AddChild(gunInstance);
	}
	//if hit by bullet, remove hp accordingly
	public void TakeDamage(int amount)
	{
		if (isDead)
		{ 
			return;
		}

		CurrentHealth -= amount;
		if (CurrentHealth <= 0)
		{
			isDead = true;
			KillManager.Instance.AddTeamKill();
			Die();
		}
	}
	//when hp is 0
	private async void Die()
	{
		SetProcess(false);
		SetPhysicsProcess(false);
		isShooting = false;
		isReloading = false;
		DropGun();
		Freeze = false;
		GravityScale = 1.5f;
		AxisLockAngularX = false;
		AxisLockAngularY = false;
		AxisLockAngularZ = false;
		Vector3 impulseDirection = -GlobalTransform.Basis.Z + Vector3.Up * 0.6f;
		ApplyImpulse(impulseDirection.Normalized() * 6f);
		await ToSignal(GetTree().CreateTimer(ragdollTime), "timeout");
		EnemyDied?.Invoke(spawnMarker, gunName);
		CallDeferred(nameof(DeferredDie)); 
	}
	//remove them from scene
	private void DeferredDie()
	{
		QueueFree();
	}
	//drop their gun when they die
	private void DropGun()
	{
		if (!IsInsideTree())
		{
			return;
		}

		if (!IsInstanceValid(gunHolder))
		{
			return;
		}

		if (gunHolder.GetChildCount() == 0)
		{
			return;
		}
		//removes them from the parent (enemy)
		Node3D gunVisual = gunHolder.GetChild<Node3D>(0);
		gunHolder.RemoveChild(gunVisual);
		RigidBody3D rb = new RigidBody3D{Name = "DroppedGun", GravityScale = 1f, Freeze = false, Sleeping = false};
		CollisionShape3D col = new CollisionShape3D();
		col.Shape = new BoxShape3D{Size = new Vector3(0.4f, 0.2f, 1.0f)};
		rb.AddChild(col);
		rb.AddChild(gunVisual);
		GetParent().AddChild(rb);
		rb.GlobalTransform = gunHolder.GlobalTransform;
		Vector3 throwDirection = (-GlobalTransform.Basis.Z + Vector3.Up * 0.5f).Normalized();
		rb.ApplyImpulse(throwDirection * 3.5f);
		StartGunDespawn(rb);
	}
	//how long gun will stay until it is removed
	private async void DespawnGun(RigidBody3D rb)
	{
		await ToSignal(GetTree().CreateTimer(5f), "timeout");

		if (IsInstanceValid(rb))
		{
			rb.QueueFree();
		}
	}
	//timer of how long they stay
	private void StartGunDespawn(RigidBody3D rb)
	{
		Timer t = new Timer();
		t.WaitTime = 5f;
		t.OneShot = true;
		rb.AddChild(t);

		t.Timeout += () =>
		{
			if (IsInstanceValid(rb))
			{
				rb.QueueFree();
			}
		};

		t.Start();
	}
}
