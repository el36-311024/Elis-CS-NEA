using Godot; 
using System; 
using System.Collections.Generic; 
public partial class CapturePoint : Node3D 
{ 
	public ProgressBar Bar; 
	public enum OwnerType {Neutral, Team, Enemy} 
	public float CaptureTime = 10f; 
	public OwnerType Owner = OwnerType.Neutral; 
	private float progress = 0f; 
	private Area3D captureArea; 
	private MeshInstance3D areaColour; 
	private HashSet<Node> teamInside = new(); 
	private HashSet<Node> enemyInside = new(); 
	
	public override void _Ready() 
	{ 
		captureArea = GetNode<Area3D>("CaptureArea"); 
		areaColour = GetNode<MeshInstance3D>("CaptureAreaColour"); 
		string nodeName = Name.ToString(); 
		var canvas = GetTree().Root.GetNode("Map/CM/CanvasLayer"); 
		
		switch (nodeName) 
		{ 
			case "CapturePoint1": 
				Bar = canvas.GetNode<ProgressBar>("CapturePointA"); 
				break; 
			case "CapturePoint2": 
				Bar = canvas.GetNode<ProgressBar>("CapturePointB"); 
				break; 
			case "CapturePoint3": 
				Bar = canvas.GetNode<ProgressBar>("CapturePointC"); 
				break; 
			default: 
				GD.PrintErr("Unknown capture point node name: " + nodeName); 
				break; 
		} 
		AddToGroup("CapturePoint"); 
		var mat = areaColour.GetActiveMaterial(0) as StandardMaterial3D; 
		mat = (StandardMaterial3D)mat.Duplicate(); 
		areaColour.SetSurfaceOverrideMaterial(0, mat); 
		captureArea.BodyEntered += OnBodyEntered; 
		captureArea.BodyExited += OnBodyExited; 
		UpdateColour(); 
	}
	
	public override void _PhysicsProcess(double delta) 
	{ 
		int teamCount = teamInside.Count; 
		int enemyCount = enemyInside.Count; 
		
		if (teamCount > 0 && enemyCount > 0) 
		{ 
			return; 
		} 
		
		float speed = (float)delta; 
		
		if (teamCount > 0 && enemyCount == 0) 
		{ 
			if (Owner == OwnerType.Enemy) 
			{ 
				progress += speed; 
				
				if (progress >= 0) 
				{ 
					progress = 0; SetOwner(OwnerType.Neutral); 
				} 
			} 
			else 
			{ 
				progress += speed; 
			} 
		} 
		else if (enemyCount > 0 && teamCount == 0) 
		{ 
			if (Owner == OwnerType.Team) 
			{ 
				progress -= speed; 
				
				if (progress <= 0) 
				{ 
					progress = 0; SetOwner(OwnerType.Neutral); 
				} 
			} 
			else 
			{ 
				progress -= speed; 
			} 
		} 
		
		progress = Mathf.Clamp(progress, -CaptureTime, CaptureTime); 
		
		if (progress >= CaptureTime) 
		{ 
			SetOwner(OwnerType.Team); 
		} 
		if (progress <= -CaptureTime) 
		{ 
			SetOwner(OwnerType.Enemy); 
		} 
		
		float normalized = (progress + CaptureTime) / (CaptureTime * 2f); 
		Bar.Value = normalized * 100f; 
	} 
	
	private void SetOwner(OwnerType newOwner) 
	{ 
		if (Owner == newOwner) 
		{ 
			return; 
		} 
		
		var oldOwner = Owner; 
		Owner = newOwner; 
		
		if (Owner == OwnerType.Team) 
		{ 
			progress = CaptureTime; 
		} 
		else if (Owner == OwnerType.Enemy) 
		{ 
			progress = -CaptureTime; 
		} 
		else 
		{ 
			progress = 0f; 
		} 
		
		UpdateColour(); 
		CaptureManager.Instance?.CaptureChanged(oldOwner, newOwner); 
	} 
	
	private void UpdateColour() 
	{ 
		Color c = new Color(0, 1, 0, 0.05f); 
		
		if (Owner == OwnerType.Team) 
		{ 
			c = new Color(0, 0, 1, 0.2f); 
		} 
		else if (Owner == OwnerType.Enemy) 
		{ 
			c = new Color(1, 0, 0, 0.2f); 
		} 
		
		var mat = areaColour.GetActiveMaterial(0) as StandardMaterial3D; 
		mat.Transparency = BaseMaterial3D.TransparencyEnum.Alpha; 
		mat.CullMode = BaseMaterial3D.CullModeEnum.Disabled; 
		mat.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly; 
		mat.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded; 
		mat.NoDepthTest = false; 
		mat.AlbedoColor = c; 
	} 
	
	private void OnBodyEntered(Node body) 
	{ 
		if (body.IsInGroup("Team")) teamInside.Add(body); 
		if (body.IsInGroup("Enemy")) enemyInside.Add(body); 
	} 
	
	private void OnBodyExited(Node body) 
	{ 
		teamInside.Remove(body); 
		enemyInside.Remove(body); 
	} 
	
	public void ResetPoint()
	{ 
		Owner = OwnerType.Neutral; 
		progress = 0f;

		if (Bar != null)
		{
			Bar.Value = 50;
		}

		teamInside.Clear();
		enemyInside.Clear(); 
		UpdateColour();
	}
}
