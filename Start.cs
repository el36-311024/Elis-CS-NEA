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
	
	//goes back to menu scene
	private void BackPressed()
	{
		PackedScene backScene = GD.Load<PackedScene>("res://menu.tscn");
		Control backInstance = (Control)backScene.Instantiate();
		GetTree().Root.AddChild(backInstance);
		this.QueueFree();
	}
	//spawn with pistol
	private void PistolPressed()
	{
		SelectGun("Pistol");
	}
	//spawn with rifle1
	private void Rifle1Pressed()
	{
		SelectGun("Rifle1");
	}
	//spawn with rifle2
	private void Rifle2Pressed()
	{
		SelectGun("Rifle2");
	}
	//spawn with heavy
	private void HeavyPressed()
	{
		SelectGun("Heavy");
	}
	//spawn with sniper
	private void SniperPressed()
	{
		SelectGun("Sniper");
	}
	
	//checks which gun has been selected and spawn with that gun
	private void SelectGun(string gunName)
	{
		var globalData = GetNode<GlobalData>("/root/GlobalData");
		globalData.SelectedGunName = gunName;
		this.QueueFree();
		GetTree().ChangeSceneToFile("res://Map.tscn");
	}
}
