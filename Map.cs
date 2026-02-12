using Godot;
using System;
using System.Collections.Generic;

public partial class Map : Node3D
{
	public static Map Instance;
	private Node3D currentPlayer;
	private ProgressBar teamBar;
	private ProgressBar enemyBar;
	private Label teamCount;
	private Label enemyCount;
	private CanvasLayer CaptureUI;

	public override void _Ready()
	{
		Instance = this;
		SpawnPlayer();
		SpawnEnemies();
		SpawnTeam();
		KMBar();
		CaptureUI = GetNode<CanvasLayer>("CM/CanvasLayer");
		ShowCaptureManagerUI(true);
		KillManager.Instance.RegisterUI(teamBar, enemyBar, teamCount, enemyCount);
		KillManager.Instance.Reset();
	}
	
	//deletes and removed from scnene
	public override void _ExitTree()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}
	
	//ensures the bar is visible
	public void ShowCaptureManagerUI(bool show)
	{
		CaptureUI.Visible = show;
	}
	
	//gets all the killmanager bars and show them
	private void KMBar()
	{
		teamBar = GetNode<ProgressBar>("KM/CanvasLayer/TeamBar");
		enemyBar = GetNode<ProgressBar>("KM/CanvasLayer/EnemyBar");
		teamCount = GetNode<Label>("KM/CanvasLayer/TeamCount");
		enemyCount = GetNode<Label>("KM/CanvasLayer/EnemyCount");
		KillManager.Instance.RegisterUI(teamBar,enemyBar,teamCount,enemyCount);
		ShowKillManagerUI(true);
	}
	
	//ensures the kill manager bars are visible
	public void ShowKillManagerUI(bool show)
	{
		teamBar.Visible = show;
		enemyBar.Visible = show;
		teamCount.Visible = show;
		enemyCount.Visible = show;
	}
	
	//spawns character onto map
	private void SpawnPlayer()
	{
		if (currentPlayer != null && GodotObject.IsInstanceValid(currentPlayer))
		{
			currentPlayer.CallDeferred("queue_free");
   			currentPlayer = null;
		}
		
		var globalData = GetNode<GlobalData>("/root/GlobalData");
		string selectedGun = globalData.SelectedGunName;
		Node3D allSpawns = GetNode<Node3D>("AllSpawnPoints");
		Marker3D SpawnPoint = allSpawns.GetNode<Marker3D>("SpawnPoint");
		PackedScene playerScene = GD.Load<PackedScene>("res://character.tscn");
		Node3D playerInstance = playerScene.Instantiate<Node3D>();
		AddChild(playerInstance);
		playerInstance.GlobalTransform=SpawnPoint.GlobalTransform;
		Node3D gunHolder = playerInstance.GetNode<Node3D>("ArmPivot/ArmMovement/GunPivot/GunHolder");
		
		//removes any unnecessary gun thats not being used
		foreach (Node child in gunHolder.GetChildren())
		{
			child.QueueFree();
		}
		
		//adds the necessary one
		PackedScene gunScene = GD.Load<PackedScene>($"res://{selectedGun}.tscn");
		Node3D gunInstance = gunScene.Instantiate<Node3D>();
		gunHolder.AddChild(gunInstance);
	}
	
	//spawns enemy onto marker nodes	
	private void SpawnEnemies()
	{
		//set amount of each gun type i want
		List<string> enemyGuns = new List<string>(){"Sniper", "Sniper", "Heavy", "Heavy", "Rifle1", "Rifle1","Rifle2", "Rifle2", "Pistol", "Pistol", "Rifle1"};
		Node3D allSpawns = GetNode<Node3D>("AllSpawnPoints");
		
		//spawns them here on each marker
		for (int i = 0; i < enemyGuns.Count; i++)
		{
			Marker3D marker = allSpawns.GetNode<Marker3D>($"enemy{i + 1}");
			SpawnEnemy(marker, enemyGuns[i]);
		}
	}
	
	//spawns every enemy
	private void SpawnEnemy(Marker3D marker, string gunName)
	{
		PackedScene enemyScene = GD.Load<PackedScene>("res://enemy.tscn");
		enemy enemyInstance = enemyScene.Instantiate<enemy>();
		AddChild(enemyInstance);
		enemyInstance.GlobalTransform = marker.GlobalTransform;
		enemyInstance.Initialize(marker, gunName);
		enemyInstance.EnemyDied += OnEnemyDied;
	}
	
	//when enemy health is 0, remove them totally, then respawn them back onto same marker node
	private async void OnEnemyDied(Marker3D marker, string gunName)
	{
		await ToSignal(GetTree().CreateTimer(10.0f), "timeout");
		SpawnEnemy(marker, gunName);
	}
	
	//spawns team onto each marker nodes
	private void SpawnTeam()
	{
		//gets the amount of gun types i want
		List<string> teamGuns = new List<string>(){"Sniper", "Sniper", "Heavy", "Heavy", "Rifle1", "Rifle1","Rifle2", "Rifle2", "Pistol", "Pistol"};
		Node3D allSpawns = GetNode<Node3D>("AllSpawnPoints");
		
		//spawn each team to the marker nodes
		for (int i = 0; i < teamGuns.Count; i++)
		{
			Marker3D marker = allSpawns.GetNode<Marker3D>($"team{i + 1}");
			SpawnTeam(marker, teamGuns[i]);
		}
	}
	
	//spawns the team into map
	private void SpawnTeam(Marker3D marker, string gunName)
	{
		PackedScene teamScene = GD.Load<PackedScene>("res://team.tscn");
		team teamInstance = teamScene.Instantiate<team>();
		AddChild(teamInstance);
		teamInstance.GlobalTransform = marker.GlobalTransform;
		teamInstance.Initialize(marker, gunName);
		teamInstance.TeamDied += OnTeamDied;
	}
	
	//when team health is 0, they are removed from map, then spawn again onto the marker nodes
	private async void OnTeamDied(Marker3D marker, string gunName)
	{
		await ToSignal(GetTree().CreateTimer(10.0f), "timeout");
		SpawnTeam(marker, gunName);
	}
	
	//when characyer dies, gets the gun theyve selected, then respawn them back onto their marker
	public void RespawnPlayer(string gunName)
	{
		GetNode<GlobalData>("/root/GlobalData").SelectedGunName = gunName;
		SpawnPlayer();
		ShowKillManagerUI(true);
		ShowCaptureManagerUI(true);
	}
}
