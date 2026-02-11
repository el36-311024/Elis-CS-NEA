using Godot;
using System;

public partial class LogInOrSignUpSystem : Control
{
	private LineEdit emailInput;
	private LineEdit passwordInput;
	private LineEdit nicknameInput;

	private Button loginButton;
	private Button signUpButton;
	private Button resetButton;
	private Button deleteButton;
	private Button backButton;

	private Label messageLabel;

	public override void _Ready()
	{
		emailInput = GetNode<LineEdit>("MarginContainer/VBoxContainer/EmailInput");
		passwordInput = GetNode<LineEdit>("MarginContainer/VBoxContainer/PasswordInput");
		nicknameInput = GetNode<LineEdit>("MarginContainer/VBoxContainer/NicknameInput"); 
		messageLabel = GetNode<Label>("MarginContainer/VBoxContainer/MessageLabel");
		loginButton  = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/LogInButton");
		signUpButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/SignUpButton");
		resetButton  = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer2/ResetPasswordButton");
		deleteButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer2/DeleteAccountButton");
		backButton   = GetNode<Button>("MarginContainer/HBoxContainer/Back"); 
		
 
	}
 

	private void LoginPressed()
	{
		string result = AccountManager.Instance.Login(emailInput.Text, passwordInput.Text);

		messageLabel.Text = result;

		if (result == "OK")
		{
			this.QueueFree();
			GetTree().ChangeSceneToFile("res://Menu.tscn");
		}
	}

	private void SignUpPressed()
	{
		string result = AccountManager.Instance.SignUp(emailInput.Text, passwordInput.Text, nicknameInput.Text);
		messageLabel.Text = result;
	}

	private void ResetPasswordPressed()
	{
		bool success = AccountManager.Instance.ResetPassword(emailInput.Text, passwordInput.Text);

		if (success)
		{
			messageLabel.Text = "Password reset successful";
		}
		else
		{
			messageLabel.Text = "Reset failed";
		}
	}

	private void DeleteAccountPressed()
	{
		bool success = AccountManager.Instance.DeleteUser(passwordInput.Text);
		
		if (success)
		{
			messageLabel.Text = "Account deleted";
		}
		else
		{
			messageLabel.Text = "Wrong password";
		}
	}

	private void BackPressed()
	{
		PackedScene backScene = GD.Load<PackedScene>("res://Menu.tscn");
		Control backInstance = (Control)backScene.Instantiate();
		GetTree().Root.AddChild(backInstance);
		this.QueueFree();
	}
}
