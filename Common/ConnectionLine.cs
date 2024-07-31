using Godot;
using System;

namespace OverwatchTranscriptViewer.Common
{
	public partial class ConnectionLine : Node3D
	{
		private MeshInstance3D visual;
		private BaseMaterial3D material;
		private float appearFactorLeft;
		private double delay;
		private float thickness;
		private float speed;
		private Node3D from;
		private Node3D to;

		public override void _Ready()
		{
			visual = GetNode<MeshInstance3D>("Visual");

			material = visual.GetSurfaceOverrideMaterial(0) as BaseMaterial3D;
		}

		public override void _Process(double delta)
		{
			if (appearFactorLeft > 0.0f)
			{
				appearFactorLeft -= Convert.ToSingle(delta) * speed;
				if (appearFactorLeft <= 0.0f)
				{
					SceneController.Instance.AnimationFinished();
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

			visual.Scale = new Vector3(thickness, distance * 0.5f * lengthScale, thickness);
		}

		public void Initialize(Node3D from, Node3D to, float thickness, float speed, Color color)
		{
			SceneController.Instance.AnimationBegin();

			this.from = from;
			this.to = to;
			this.speed = speed;
			this.thickness = thickness;

			appearFactorLeft = 1.0f;
			material.AlbedoColor = color;
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
