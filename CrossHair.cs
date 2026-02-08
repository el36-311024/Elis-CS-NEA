using Godot;
using System;

public partial class CrossHair : CenterContainer
{
	public float LineLength = 8f;
	public float LineGap = 10f;
	public float LineThickness = 2f;
	public float DotRadius = 2f;
	public Color CrosshairColor = Colors.White;

	public override void _Ready()
	{
		SetProcess(true);
	}

	public override void _Process(double delta)
	{
		QueueRedraw();
	}

	public override void _Draw()
	{
		Vector2 center = GetViewportRect().Size / 2f;

		DrawCircle(center, DotRadius, CrosshairColor);

		DrawLine(center - new Vector2(0, LineGap), center - new Vector2(0, LineGap + LineLength), CrosshairColor, LineThickness);
		DrawLine(center + new Vector2(0, LineGap), center + new Vector2(0, LineGap + LineLength), CrosshairColor, LineThickness);
		DrawLine(center - new Vector2(LineGap, 0), center - new Vector2(LineGap + LineLength, 0), CrosshairColor, LineThickness);
		DrawLine(center + new Vector2(LineGap, 0),center + new Vector2(LineGap + LineLength, 0), CrosshairColor, LineThickness);
	}
}
