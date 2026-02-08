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
	
	public void Reset()
	{
		TeamKills = 0;
		EnemyKills = 0;
		UpdateUI();
	}

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

	public void AddTeamKill()
	{
		TeamKills++;
		UpdateUI();
		CheckWin();
	}

	public void AddEnemyKill()
	{
		EnemyKills++;
		UpdateUI();
		CheckWin();
	}

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
