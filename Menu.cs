using Godot;
using System;

public partial class Menu : Control
{
	private Button startButton;
	private Button quitButton;
	private Button optionButton;
	
	public override void _Ready()
	{
		startButton = GetNode<Button>("MarginContainer/HBoxContainer/VBoxContainer/start");
		quitButton = GetNode<Button>("MarginContainer/HBoxContainer/VBoxContainer/Quit");
		optionButton = GetNode<Button>("MarginContainer/HBoxContainer/VBoxContainer/Option");
	}

	private void startPressed()
	{
		PackedScene startScene = GD.Load<PackedScene>("res://start.tscn");
		Control startInstance = (Control)startScene.Instantiate();
		GetTree().Root.AddChild(startInstance);
		this.QueueFree();
	}
	
	private void OptionPressed()
	{
		PackedScene optionScene = GD.Load<PackedScene>("res://option.tscn");
		Control optionInstance = (Control)optionScene.Instantiate();
		GetTree().Root.AddChild(optionInstance);
		this.QueueFree();
	}

	private void QuitPressed()
	{
		GetTree().Quit();
	}
}
