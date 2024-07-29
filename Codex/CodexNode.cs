using Godot;
using OverwatchTranscriptViewer;
using OverwatchTranscriptViewer.Codex;
using System;

public partial class CodexNode : Node3D
{
	private const float scaleSpeed = 0.5f;
	private const float startingScale = 0.15f;
	private const float startingSpeed = 10.0f;
	private const float runningScale = 0.3f;
	private const float runningSpeed = 0.1f;
	private float currentScale;
	private float targetScale;
	private rotates rotate;
	private Label3D text;

	public string CodexName { get; private set; }
	public string PeerId { get; private set; }

	public void Starting(string name, string peerId)
	{
		CodexName = name;
		PeerId = peerId;

		targetScale = startingScale;
		rotate.Speed = startingSpeed;
		rotate.TargetSpeed = startingSpeed;

		text.Text = name;

		Lookup.Add(name, this);
		Lookup.Add(peerId, this);
		Lookup.Add(CodexUtils.ToShortId(peerId), this);
	}

	public void Started()
	{
		targetScale = runningScale;
		rotate.TargetSpeed = runningSpeed;

		SceneController.Instance.Proceed();
	}

	public override void _Ready()
	{
		rotate = GetNode<rotates>("MeshInstance3D");
		text = GetNode<Label3D>("Label3D");

		currentScale = 0.01f;
		targetScale = currentScale;
		rotate.Speed = startingSpeed;
		rotate.TargetSpeed = rotate.Speed;
		rotate.SpeedChangeRate = 2.0f;
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
			}
		}
	}
}
