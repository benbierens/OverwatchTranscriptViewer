using Godot;
using OverwatchTranscriptViewer;
using OverwatchTranscriptViewer.Codex;
using System;

public partial class CodexNode : Node3D
{
	private const float scaleSpeed = 1.0f;
	private const float startingScale = 0.15f;
	private const float startingSpeed = 10.0f;
	private const float runningScale = 0.3f;
	private const float runningSpeed = 0.1f;
	private float currentScale;
	private float targetScale;
	private rotates rotate;
	private Label3D text;

	public void Starting(string name)
	{
		targetScale = startingScale;
		rotate.Speed = startingSpeed;
		rotate.TargetSpeed = startingSpeed;

		text.Text = name;

		Lookup.Add(name, this);
	}

	public void Started(string peerId)
	{
		targetScale = runningScale;
		rotate.TargetSpeed = runningSpeed;

		Lookup.Add(peerId, this);
		Lookup.Add(CodexUtils.ToShortId(peerId), this);
	}

	public override void _Ready()
	{
		rotate = GetNode<rotates>("MeshInstance3D");
		text = GetNode<Label3D>("Label3D");

		currentScale = 0.01f;
		targetScale = currentScale;
		rotate.Speed = startingSpeed;
		rotate.TargetSpeed = rotate.Speed;
		rotate.SpeedChangeRate = 0.5f;
	}

	public override void _Process(double delta)
	{
		rotate.Scale = new Vector3(currentScale, currentScale, currentScale);
		if (currentScale < targetScale)
		{
			currentScale += scaleSpeed * Convert.ToSingle(delta);
			if (currentScale >= targetScale)
			{
				SceneController.Instance.Proceed();

				GD.Print("proceed");
			}
		}
	}
}
