using Godot;
using System;

public partial class Start : Control
{
	private Button Back;
	private Button Pistol;
	private Button Rifle1;
	private Button Rifle2;
	private Button Heavy;
	private Button Sniper;
	
	public override void _Ready()
	{
		Back = GetNode<Button>("MarginContainer/BackButton/Back");
		Pistol = GetNode<Button>("MarginContainer/CharacterSelection/VBoxContainer/Pistol");
		Rifle1 = GetNode<Button>("MarginContainer/CharacterSelection/VBoxContainer/Rifle1");
		Rifle2 = GetNode<Button>("MarginContainer/CharacterSelection/VBoxContainer/Rifle2");
		Heavy = GetNode<Button>("MarginContainer/CharacterSelection/VBoxContainer2/Heavy");
		Sniper = GetNode<Button>("MarginContainer/CharacterSelection/VBoxContainer2/Sniper");
	}
	
	private void BackPressed()
	{
		PackedScene backScene = GD.Load<PackedScene>("res://menu.tscn");
		Control backInstance = (Control)backScene.Instantiate();
		GetTree().Root.AddChild(backInstance);
		this.QueueFree();
	}
	
	private void PistolPressed()
	{
		SelectGun("Pistol");
	}
	private void Rifle1Pressed()
	{
		SelectGun("Rifle1");
	}
	private void Rifle2Pressed()
	{
		SelectGun("Rifle2");
	}
	private void HeavyPressed()
	{
		SelectGun("Heavy");
	}
	private void SniperPressed()
	{
		SelectGun("Sniper");
	}
	
	private void SelectGun(string gunName)
	{
		var globalData = GetNode<GlobalData>("/root/GlobalData");
		globalData.SelectedGunName = gunName;
		this.QueueFree();
		GetTree().ChangeSceneToFile("res://Map.tscn");
	}
}
