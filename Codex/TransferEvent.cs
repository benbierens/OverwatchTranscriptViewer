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
		private float speed;
		private Action whenDone;
		private Node3D source;
		private Node3D target;

		public override void _Ready()
		{
			visual = GetNode<Node3D>("Visual");

			//material = visual.GetSurfaceOverrideMaterial(0) as BaseMaterial3D;
		}

		public override void _Process(double delta)
		{
			factor += Convert.ToSingle(delta) * speed;
			if (factor > 1.0f)
			{
				//whenDone();
				QueueFree();
				return;
			}

			Transform = new Transform3D
			{
				Origin = source.Transform.Origin.Lerp(target.Transform.Origin, factor),
				Basis = new Basis(Quaternion.Identity)
			};
		}

		public void Initialize(Node3D source, Node3D target, string label, float speed, Action whenDone)
		{
			this.source = source;
			this.target = target;
			this.speed = speed;
			this.whenDone = whenDone;
			factor = 0.0f;

			visual.GetNode<Label3D>("Label3D").Text = ""; // label
			var rotates = visual.GetNode<rotates>("MeshInstance3D");
			rotates.Speed = 5.0f;
			rotates.TargetSpeed = rotates.Speed;

			Transform = new Transform3D
			{
				Origin = source.Transform.Origin,
				Basis = new Basis(Quaternion.Identity)
			};

			whenDone();
		}
	}
}
