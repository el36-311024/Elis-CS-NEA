using Godot;
using System;

public partial class respawn : Control
{
	private Button pistol;
	private Button rifle1;
	private Button rifle2;
	private Button heavy;
	private Button sniper;
	
	public override void _Ready()
	{
		pistol = GetNode<Button>("MarginContainer/CharacterSelection/VBoxContainer/pistol");
		rifle1 = GetNode<Button>("MarginContainer/CharacterSelection/VBoxContainer/rifle1");
		rifle2 = GetNode<Button>("MarginContainer/CharacterSelection/VBoxContainer/rifle2");
		heavy = GetNode<Button>("MarginContainer/CharacterSelection/VBoxContainer2/heavy");
		sniper = GetNode<Button>("MarginContainer/CharacterSelection/VBoxContainer2/sniper");
	}
	//respawn back with pistol
	private void PistolRespawn()
	{
		SelectGun("Pistol");
	}
	//respawn back with rifle1
	private void Rifle1Respawn()
	{
		SelectGun("Rifle1");
	}
	//respawn back with rifle2
	private void Rifle2Respawn()
	{
		SelectGun("Rifle2");
	}
	//respawn back with heavy
	private void HeavyRespawn()
	{
		SelectGun("Heavy");
	}
	//respawn back with sniper
	private void SniperRespawn()
	{
		SelectGun("Sniper");
	}
	
	//checks which gun has been selected
	private void SelectGun(string gunName)
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		Map.Instance.RespawnPlayer(gunName);
		QueueFree();
	}
}
