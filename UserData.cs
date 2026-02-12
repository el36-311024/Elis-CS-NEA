using Godot;
using System;
using Newtonsoft.Json;

public partial class UserData : Node
{
	//stores all the user data that are needed
	public string Email;
	public string PasswordHash;
	public string Nickname;
	public int BestScore;
}
