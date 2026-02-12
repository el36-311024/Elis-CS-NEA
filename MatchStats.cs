using Godot;
using System;

public partial class MatchStats : Node
{
	public static MatchStats Instance;
	public bool TeamWon;
	public bool WinByCapture;
	private double matchTime;
	private const int TimeStartPoints = 10000;
	public const int KillPoints = 5;
	public const int EnemyKillPoints = -1;
	public const int CapturePoints = 25;
	public const int CaptureWinBonus = 100;
	public const int WinBonus = 5000;
	public const int LosePenalty = -5000;

	public override void _Ready()
	{
		Instance = this;
		matchTime = 0;
	}
	
	//resets the results of match at the end of the round
	public void Reset()
	{
		TeamWon = false;
		WinByCapture = false;
		matchTime = 0;
	}
	
	//counts how long it took either one of the team to win
	public override void _Process(double delta)
	{
		matchTime += delta;
	}
	
	//adds a score bonus if game was done quicker
	public int GetTimeBonus()
	{
		return Mathf.Max(TimeStartPoints - (int)matchTime, 0);
	}
	
	//adds up the total score
	public int CalculateFinalScore()
	{
		int score = 0;
		score += KillManager.Instance.TeamKills * KillPoints;
		score += KillManager.Instance.EnemyKills * EnemyKillPoints;
		score += CaptureManager.Instance.TeamCaptureCount * CapturePoints;

		if (WinByCapture)
		{
			score += CaptureWinBonus;
		}

		score += GetTimeBonus();

		if (TeamWon)
		{
			score += WinBonus;
		}
		else
		{
			score += LosePenalty;
		}

		return score;
	}
}
