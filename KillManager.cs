using Godot;

public partial class KillManager : Node
{
	public static KillManager Instance;
	public int TeamKills;
	public int EnemyKills;
	private ProgressBar TeamBar;
	private ProgressBar EnemyBar;
	private Label TeamCount;
	private Label EnemyCount;
	private const int MaxKills = 99;

	public override void _Ready()
	{
		Instance = this;
	}
	
	//when game restarts, the kills scores are reset
	public void Reset()
	{
		TeamKills = 0;
		EnemyKills = 0;
		UpdateUI();
	}
	
	//shows the progress bar of each capture, ensures they have max and min value
	public void RegisterUI(ProgressBar teamBar, ProgressBar enemyBar, Label teamCount, Label enemyCount)
	{
		TeamBar = teamBar;
		EnemyBar = enemyBar;
		TeamCount = teamCount;
		EnemyCount = enemyCount;
		TeamBar.MaxValue = MaxKills;
		EnemyBar.MaxValue = MaxKills;
		UpdateUI();
	}
	
	//if team/enemy gets killed, the counter increments by 1
	private void UpdateUI()
	{
		if (IsInstanceValid(TeamBar))
		{
			TeamBar.Value = TeamKills;
		}

		if (IsInstanceValid(EnemyBar))
		{
			EnemyBar.Value = EnemyKills;
		}

		if (IsInstanceValid(TeamCount))
		{
			TeamCount.Text = TeamKills.ToString();
		}

		if (IsInstanceValid(EnemyCount))
		{
			EnemyCount.Text = EnemyKills.ToString();
		}
	}
	
	//add the total amount of kills for team
	public void AddTeamKill()
	{
		TeamKills++;
		UpdateUI();
		CheckWin();
	}
	//add the total amount of kills for enemy
	public void AddEnemyKill()
	{
		EnemyKills++;
		UpdateUI();
		CheckWin();
	}
	
	//checks whether team/enemy won and display the necessary information
	private void CheckWin()
	{
		if (TeamKills >= MaxKills)
		{
			MatchStats.Instance.TeamWon = true;
			MatchStats.Instance.WinByCapture = false;
			GetTree().ChangeSceneToFile("res://EndGame.tscn");
		}

		if (EnemyKills >= MaxKills)
		{
			MatchStats.Instance.TeamWon = false;
			MatchStats.Instance.WinByCapture = false;
			GetTree().ChangeSceneToFile("res://EndGame.tscn");
		}
	}
	
	public void UnregisterUI()
	{
		TeamBar = null;
		EnemyBar = null;
		TeamCount = null;
		EnemyCount = null;
	}
}
