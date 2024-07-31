using Godot;
using System;

namespace OverwatchTranscriptViewer.Codex
{
	public partial class TransferEvent : Node3D
	{
		private Node3D visual;
		//private BaseMaterial3D material;
		private float factor;
		private bool backwards;
		private double timeLeft;
		private double totalTime;
		private Node3D source;
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
				if (AnimationConfig.WaitForBlockTransferEvents)
					SceneController.Instance.AnimationFinished();
				QueueFree();
				return;
			}

			float factor = 1.0f - Convert.ToSingle(timeLeft / totalTime);
			Transform = new Transform3D
			{
				Origin = source.Transform.Origin.Lerp(target.Transform.Origin, factor),
				Basis = new Basis(Quaternion.Identity)
			};
		}

		public void Initialize(Node3D source, Node3D target, string label, double totalTime)
		{
			if (AnimationConfig.WaitForBlockTransferEvents)
				SceneController.Instance.AnimationBegin();

			this.source = source;
			this.target = target;
			if (totalTime < AnimationConfig.BlockTransferMinDuration) totalTime = AnimationConfig.BlockTransferMinDuration;
			timeLeft = totalTime;
			this.totalTime = totalTime;
			factor = 0.0f;

			visual.GetNode<Label3D>("Label3D").Text = CodexUtils.ToShortId(label);
			var rotates = visual.GetNode<rotates>("MeshInstance3D");
			rotates.Speed = 5.0f;
			rotates.TargetSpeed = rotates.Speed;

			Transform = new Transform3D
			{
				Origin = source.Transform.Origin,
				Basis = new Basis(Quaternion.Identity)
			};
		}
	}
}
