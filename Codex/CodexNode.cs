using CodexPlugin.OverwatchSupport;
using Godot;
using System;

public partial class CodexNode : Node3D
{
	private static readonly Placer placer = new Placer();
	private const float scaleSpeed = 1.0f;
	private const float targetScale = 0.3f;
	private float scale;
	private rotates visual;
	private Label3D text;
	private NodeStartedEvent startEvent;

	public void Initialize(NodeStartedEvent startEvent)
	{
		this.startEvent = startEvent;
	}

	public override void _Ready()
	{
		Translate(placer.GetPlace());
		visual = GetNode<rotates>("MeshInstance3D");
		text = GetNode<Label3D>("Label3D");

		scale = 0.01f;
		visual.Speed = 10.0f;
	}

	public override void _Process(double delta)
	{
		visual.Scale = new Vector3(scale, scale, scale);
		if (scale < targetScale)
		{
			scale += scaleSpeed * Convert.ToSingle(delta);
			if (scale >= targetScale)
			{
				visual.Speed = 0.1f;
				SceneController.Instance.Proceed();
				text.Text = startEvent.Name;
			}
		}
	}
}

public class Placer
{
	private const float Radius = 2.0f;
	private const int number = 10;
	private int count = 0;

	public Vector3 GetPlace()
	{
		count++;
		float f = count;
		double n = number;
		float t = f * Convert.ToSingle(Math.PI * 2.0 / n);

		return new Vector3(
			Mathf.Sin(t) * Radius,
			Mathf.Cos(t) * Radius,
			0.0f
		);
	}
}
