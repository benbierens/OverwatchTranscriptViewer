using Godot;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace OverwatchTranscriptViewer.Codex
{
	public partial class FileEvent : Node3D
	{
		private MeshInstance3D visual;
		private BaseMaterial3D material;
		private float factor;
		private bool backwards;
		private float speed;
		private Action whenDone;
		private Node3D target;

		public override void _Ready()
		{
			visual = GetNode<MeshInstance3D>("Visual");

			material = visual.GetSurfaceOverrideMaterial(0) as BaseMaterial3D;
		}

		public override void _Process(double delta)
		{
			factor += Convert.ToSingle(delta) * speed;
			if (factor > 1.0f)
			{
				whenDone();
				QueueFree();
				return;
			}

			Transform = new Transform3D
			{
				Origin = backwards ?
					target.Transform.Origin.Lerp(Vector3.Zero, factor) :
					Vector3.Zero.Lerp(target.Transform.Origin, factor),
				Basis = new Basis(Quaternion.Identity)
			};
		}

		public void Initialize(Node3D target, string cid, float speed, bool backwards, Action whenDone)
		{
			this.target = target;
			this.speed = speed;
			this.whenDone = whenDone;
			this.backwards = backwards;
			factor = 0.0f;

			visual.GetNode<Label3D>("Label3D").Text = cid;
		}
	}
}
