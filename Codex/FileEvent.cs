using Godot;
using System;

namespace OverwatchTranscriptViewer.Codex
{
	public partial class FileEvent : Node3D
	{
		private Vector3 sourcePosition = Vector3.Zero;
		private Node3D visual;
		//private BaseMaterial3D material;
		private float factor;
		private bool backwards;
		private double timeLeft;
		private double totalTime;
		private Node3D target;

		public override void _Ready()
		{
			visual = GetNode<Node3D>("Visual");

			//material = visual.GetSurfaceOverrideMaterial(0) as BaseMaterial3D;
		}

		public override void _Process(double delta)
		{
			if (timeLeft <= 0.0) return;

			timeLeft -= delta;
			if (timeLeft <= 0.0)
			{
				SceneController.Instance.AnimationFinished();
				QueueFree();
				return;
			}

			float factor = 1.0f - Convert.ToSingle(timeLeft / totalTime);
			Transform = new Transform3D
			{
				Origin = backwards ?
					target.Transform.Origin.Lerp(sourcePosition, factor) :
					sourcePosition.Lerp(target.Transform.Origin, factor),
				Basis = new Basis(Quaternion.Identity)
			};
		}

		public void Initialize(Node3D target, string cid, bool backwards, double duration)
		{
			SceneController.Instance.AnimationBegin();

			this.target = target;
			sourcePosition = target.Transform.Origin * 2.0f;
			if (duration < 1.0) duration = 1.0;
			timeLeft = duration;
			totalTime = duration;
			this.backwards = backwards;
			factor = 0.0f;

			visual.GetNode<Label3D>("Label3D").Text = cid;
		}
	}
}
