using Godot;
using System;

public partial class Credits : Control
{
	private Button backButton;
	
	public override void _Ready()
	{
		backButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/Back");
	}
	
	private void BackPressed()
	{
		PackedScene backScene = GD.Load<PackedScene>("res://option.tscn");
		Control backInstance = (Control)backScene.Instantiate();
		
		GetTree().Root.AddChild(backInstance);
		
		this.QueueFree();
	}
}
