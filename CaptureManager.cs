using Godot;
using System;
using System.Collections.Generic;

public partial class CaptureManager : Node
{
	public static CaptureManager Instance;
	private List<CapturePoint> points = new();

	private int teamScore = 0;
	private int enemyScore = 0;

	private const int WinScore = 3;
	public int TeamCaptureCount;
	public int EnemyCaptureCount => enemyScore;

	public override void _Ready()
	{
		Instance = this;
		points.Clear();

		//retrieves all possible capture point (if any)
		foreach (Node n in GetTree().GetNodesInGroup("CapturePoint"))
		{
			if (n is CapturePoint point)
			{
				points.Add(point);
			}
		}

		RegisterCaptureBars();
	}
	
	//reset all capture point when player has restarted game
	public void Reset()
	{
		teamScore = 0;
		enemyScore = 0;
		TeamCaptureCount = 0;

		foreach (var cp in points)
		{
			cp.ResetPoint();
		}
	}
	
	//sets the capture point to have max and min value
	public void RegisterCaptureBars()
	{
		foreach (var cp in points)
		{
			if (cp.Bar != null)
			{
				cp.Bar.MinValue = 0;
				cp.Bar.MaxValue = 100;
				cp.Bar.Value = 50;
			}
		}
	}
	
	//changes the capture point in game, so when team is inside, it goes up. If enemy is inside, it goes down
	public void CaptureChanged(CapturePoint.OwnerType oldOwner, CapturePoint.OwnerType newOwner)
	{
		if (oldOwner == CapturePoint.OwnerType.Team) 
		{
			teamScore--;
		}
		if (oldOwner == CapturePoint.OwnerType.Enemy) 
		{
			enemyScore--;
		}
		if (newOwner == CapturePoint.OwnerType.Team)
		{
			teamScore++;
		}
		if (newOwner == CapturePoint.OwnerType.Enemy) 
		{
			enemyScore++;
		}
		
		TeamCaptureCount = teamScore;
		CheckWinLoss();
	}
	
	//checks if team or enemy won. then goes to screen saying defeat or victory depending on outcome
	private void CheckWinLoss()
	{
		if (teamScore >= WinScore)
		{
			MatchStats.Instance.TeamWon = true;
			MatchStats.Instance.WinByCapture = true;
			GetTree().ChangeSceneToFile("res://EndGame.tscn");
		}
		else if (enemyScore >= WinScore)
		{
			MatchStats.Instance.TeamWon = false;
			MatchStats.Instance.WinByCapture = false;
			GetTree().ChangeSceneToFile("res://EndGame.tscn");
		}
	}
}
