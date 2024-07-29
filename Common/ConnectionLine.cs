using Godot;
using System;

namespace OverwatchTranscriptViewer.Common
{
	public partial class ConnectionLine : Node3D
	{
		private MeshInstance3D visual;
		private float appearFactorLeft;
		private double delay;
		private float speed;
		private Action whenDone;
		private Node3D from;
		private Node3D to;

		public override void _Ready()
		{
			visual = GetNode<MeshInstance3D>("Visual");
		}

		public override void _Process(double delta)
		{
			if (appearFactorLeft > 0.0f)
			{
				appearFactorLeft -= Convert.ToSingle(delta) * speed;
				if (appearFactorLeft <= 0.0f)
				{
					whenDone();
					return;
				}

				UpdatePositionRotation(1.0f - appearFactorLeft);
				return;
			}

			if (delay > 0.0) { delay -= delta; return; }
			delay = 2.0;

			UpdatePositionRotation(1.0f);
		}

		private void UpdatePositionRotation(float lengthScale)
		{
			if (from == null || to == null) return;

			var center = GetCenterPoint();

			Transform = new Transform3D
			{
				Origin = from.Transform.Origin.Lerp(center, lengthScale),
				Basis = new Basis(Quaternion.Identity)
			};

			LookAt(to.Transform.Origin, Vector3.Forward);

			var distance = (from.Transform.Origin - to.Transform.Origin).Length();

			visual.Scale = new Vector3(0.1f, distance * 0.5f * lengthScale, 0.1f);
		}

		public void Initialize(Node3D from, Node3D to, float speed, Action whenDone)
		{
			this.from = from;
			this.to = to;
			this.speed = speed;
			this.whenDone = whenDone;

			appearFactorLeft = 1.0f;
		}

		private Vector3 GetCenterPoint()
		{
			return new Vector3(
					(from.Transform.Origin.X + to.Transform.Origin.X) / 2.0f,
					(from.Transform.Origin.Y + to.Transform.Origin.Y) / 2.0f,
					(from.Transform.Origin.Z + to.Transform.Origin.Z) / 2.0f
			);
		}
	}
}
