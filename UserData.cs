using Godot;
using System;
using Newtonsoft.Json;

public partial class UserData : Node
{
	public string Email;
	public string PasswordHash;
	public string Nickname;
	public int BestScore;
}
