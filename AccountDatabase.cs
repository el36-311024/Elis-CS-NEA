using Godot;
using System;
using System.Collections.Generic;

public partial class AccountDatabase : Node
{
	//Used to store data of user, so account can be used instantly without signing in all the time
	public List<UserData> Users = new();
	public string LastLoggedInEmail;
}
