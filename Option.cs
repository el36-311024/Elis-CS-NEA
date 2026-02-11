using Godot;
using System;

public partial class Option : Control
{
	private Button creditsButton;
	private Button helpButton;
	private Button screenButton;
	private Button backButton;
	private VBoxContainer leaderboardContainer;

	public override void _Ready()
	{
		creditsButton = GetNode<Button>("MarginContainer/CreditsHelpScreen/HBoxContainer/Credits");
		helpButton    = GetNode<Button>("MarginContainer/CreditsHelpScreen/HBoxContainer/Help");
		screenButton  = GetNode<Button>("MarginContainer/CreditsHelpScreen/HBoxContainer/Screen");
		backButton    = GetNode<Button>("MarginContainer/CreditsHelpScreen/HBoxContainer/Back");
		leaderboardContainer = GetNode<VBoxContainer>("ScrollContainer/VBoxContainer");

		PopulateLeaderboard();
	}

	private void PopulateLeaderboard()
	{
		var leaderboard = AccountManager.Instance.GetLeaderboard();

		foreach (var user in leaderboard)
		{
			Label l = new Label();
			l.Text = $"{user.Nickname} - {user.BestScore}";
			l.HorizontalAlignment = HorizontalAlignment.Center;

			leaderboardContainer.AddChild(l);   // ✅ CORRECT
		}
	}
	
	private void CreditsPressed()
	{
		PackedScene creditsScene = GD.Load<PackedScene>("res://credits.tscn");
		Control creditsInstance = (Control)creditsScene.Instantiate();
		GetTree().Root.AddChild(creditsInstance);
		this.QueueFree();
	}
	
	private void HelpPressed()
	{
		PackedScene helpScene = GD.Load<PackedScene>("res://help.tscn");
		Control helpInstance = (Control)helpScene.Instantiate();
		GetTree().Root.AddChild(helpInstance);
		this.QueueFree();
	}
	private void ScreenPressed()
	{
		PackedScene screenScene = GD.Load<PackedScene>("res://screen.tscn");
		Control screenInstance = (Control)screenScene.Instantiate();
		GetTree().Root.AddChild(screenInstance);
		this.QueueFree();
	}
	
	private void BackPressed()
	{
		PackedScene backScene = GD.Load<PackedScene>("res://Menu.tscn");
		Control backInstance = (Control)backScene.Instantiate();
		GetTree().Root.AddChild(backInstance);
		this.QueueFree();
	}
}
