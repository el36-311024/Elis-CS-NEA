using Godot;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public partial class AccountManager : Node
{
	public static AccountManager Instance;
	private string SavePath => ProjectSettings.GlobalizePath("user://accounts.json");
	public AccountDatabase Database = new();
	public UserData CurrentUser;

	public override void _Ready()
	{
		Instance = this;
		Load();
		AutoLogin();
	}

	void Load() //loads all account stored
	{
		if (!File.Exists(SavePath))
		{
			Save();
			return;
		}

		string json = File.ReadAllText(SavePath);
		Database = JsonConvert.DeserializeObject<AccountDatabase>(json);
	}

	void Save() //when account made, it serialize the data
	{
		string json = JsonConvert.SerializeObject(Database, Formatting.Indented);
		File.WriteAllText(SavePath, json);
	}

	string Hash(string input) //turns any text into a unique fixed-length code
	{
		using var sha = SHA256.Create();
		byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
		return Convert.ToHexString(bytes);
	}

	//ensure email and password is valid
	bool ValidEmail(string email) => email.Contains("@") && email.Contains(".");
	bool ValidPassword(string password) => password.Length >= 6 && password.Length <= 20;

	//create a new account which each data type being valid
	public string SignUp(string email, string password, string nickname)
	{
		if (!ValidEmail(email)) 
		{
			return "Invalid email";
		}
		if (!ValidPassword(password)) 
		{
			return "Password must be 6–20 chars";
		}
		if (Database.Users.Any(u => u.Email == email)) 
		{
			return "Account exists";
		}

		Database.Users.Add(new UserData {Email = email, PasswordHash = Hash(password), Nickname = nickname, BestScore = 0});
		Database.LastLoggedInEmail = email;
		Save();
		return "OK";
	}

	//lets user to access their account by entering their right information
	public string Login(string email, string password)
	{
		var user = Database.Users.FirstOrDefault(u => u.Email == email);
		
		if (user == null)
		{
			return "Account not found";
		}
		if (user.PasswordHash != Hash(password)) 
		{
			return "Wrong password";
		}

		CurrentUser = user;
		Database.LastLoggedInEmail = email;
		Save();
		return "OK";
	}

	//auto logs the player whichever account was used last
	void AutoLogin()
	{
		if (string.IsNullOrEmpty(Database.LastLoggedInEmail)) 
		{
			return;
		}

		CurrentUser = Database.Users.FirstOrDefault(u => u.Email == Database.LastLoggedInEmail);
	}
 	
	//reset password if they want to change it
	public bool ResetPassword(string email, string newPassword)
	{
		if (!ValidPassword(newPassword)) 
		{
			return false;
		}

		var user = Database.Users.FirstOrDefault(u => u.Email == email);
		
		if (user == null) 
		{
			return false;
		}
		
		user.PasswordHash = Hash(newPassword);
		Save();
		return true;
	}
 
	//permanetly deletes users data
	public bool DeleteUser(string password)
	{
		if (CurrentUser == null) 
		{
			return false;
		}
		if (CurrentUser.PasswordHash != Hash(password)) 
		{
			return false;
		}

		Database.Users.Remove(CurrentUser);
		CurrentUser = null;
		Database.LastLoggedInEmail = null;
		Save();
		return true;
	}
 
	//only updates when score is higher than previous one
	public void TryUpdateScore(int newScore)
	{
		if (CurrentUser == null) 
		{
			return;
		}
		if (newScore > CurrentUser.BestScore)
		{
			CurrentUser.BestScore = newScore;
			Save();
		}
	}
 	
	//retrieves leaderboard to store each score in order
	public UserData[] GetLeaderboard()
	{
		return Database.Users.OrderByDescending(u => u.BestScore).ToArray();
	}
}
