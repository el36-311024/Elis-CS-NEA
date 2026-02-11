using Godot;
using System;
using System.Collections.Generic;

public partial class AccountDatabase : Node
{
	public List<UserData> Users = new();
	public string LastLoggedInEmail;
}
