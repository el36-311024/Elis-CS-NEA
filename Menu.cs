using Godot;
using System;

public partial class Menu : Control
{
	private Button startButton;
	private Button quitButton;
	private Button optionButton;
	private Button LogInOrSignUp;
	
	public override void _Ready()
	{
		startButton = GetNode<Button>("MarginContainer/HBoxContainer/VBoxContainer/start");
		quitButton = GetNode<Button>("MarginContainer/HBoxContainer/VBoxContainer/Quit");
		optionButton = GetNode<Button>("MarginContainer/HBoxContainer/VBoxContainer/Option");
		LogInOrSignUp = GetNode<Button>("MarginContainer/LogInOrSignUp");
	}
	
	private void startPressed()
	{
		//resets all the match stats managers
		KillManager.Instance?.UnregisterUI();
		CaptureManager.Instance?.Reset();
		MatchStats.Instance?.Reset();
		PackedScene startScene = GD.Load<PackedScene>("res://start.tscn");
		Control startInstance = (Control)startScene.Instantiate();
		GetTree().Root.AddChild(startInstance);
		this.QueueFree();
	}
	
	//goes to the option screen to show leaderboard
	private void OptionPressed()
	{
		PackedScene optionScene = GD.Load<PackedScene>("res://option.tscn");
		Control optionInstance = (Control)optionScene.Instantiate();
		GetTree().Root.AddChild(optionInstance);
		this.QueueFree();
	}
	
	//game is closed
	private void QuitPressed()
	{
		GetTree().Quit();
	}
	
	//goes to the log in screen
	private void LogInOrSignUpPressed()
	{
		PackedScene LogInOrSignUpScene = GD.Load<PackedScene>("res://LogInOrSignUpSystem.tscn");
		Control LogInOrSignUpInstance = (Control)LogInOrSignUpScene.Instantiate();
		GetTree().Root.AddChild(LogInOrSignUpInstance);
		this.QueueFree();
	}
}
