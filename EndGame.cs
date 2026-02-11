using Godot;
using System;

public partial class EndGame : Control
{
	private Label endGameTitle;
	private Label teamKillsLabel;
	private Label enemyKillsLabel;
	private Label teamCapturesLabel;
	private Label enemyCapturesLabel;
	private Label timeTakenLabel;
	private Label scoreLabel;
	private Button exit;
	
	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Visible;
		endGameTitle = GetNode<Label>("TitlePanel/EndgameTitle");
		teamKillsLabel = GetNode<Label>("Panel/TeamKills");
		enemyKillsLabel = GetNode<Label>("Panel/EnemyKills");
		teamCapturesLabel = GetNode<Label>("Panel/TeamCaptures");
		enemyCapturesLabel = GetNode<Label>("Panel/EnemyCaptures");
		timeTakenLabel = GetNode<Label>("Panel/TimeTaken");
		scoreLabel = GetNode<Label>("Panel/Score");
		exit = GetNode<Button>("Exit");
		DisplayResults();
	}
	
	private void exitButton()
	{
		int finalScore = MatchStats.Instance.CalculateFinalScore();
		AccountManager.Instance?.TryUpdateScore(finalScore);
		GetTree().ChangeSceneToFile("res://Menu.tscn");
	}
	
	private void DisplayResults()
	{
		if (MatchStats.Instance.TeamWon)
		{
			endGameTitle.Text = "VICTORY!!!";
			endGameTitle.Modulate = Colors.Yellow;
		}
		else
		{
			endGameTitle.Text = "DEFEAT?!?";
			endGameTitle.Modulate = Colors.Red;
		}
			
		teamKillsLabel.Text = KillManager.Instance.TeamKills.ToString();
		enemyKillsLabel.Text = KillManager.Instance.EnemyKills.ToString();
		teamCapturesLabel.Text = CaptureManager.Instance.TeamCaptureCount.ToString();
		enemyCapturesLabel.Text = CaptureManager.Instance.EnemyCaptureCount.ToString();
		int timeTakenSeconds = 10000 - MatchStats.Instance.GetTimeBonus();
		timeTakenLabel.Text = timeTakenSeconds + "s";
		int finalScore = MatchStats.Instance.CalculateFinalScore();
		scoreLabel.Text = finalScore.ToString();
	}
}
