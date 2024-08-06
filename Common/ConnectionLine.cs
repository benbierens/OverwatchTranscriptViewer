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
		private Vector3 toPos;
		private float highlightTimeLeft;
		private Color color;

		public override void _Ready()
		{
			visual = GetNode<MeshInstance3D>("Visual");

			material = visual.GetSurfaceOverrideMaterial(0).Duplicate() as BaseMaterial3D;
			visual.SetSurfaceOverrideMaterial(0, material);
		}

		public void Initialize(Node3D from, Node3D to, float thickness, float speed, Color color)
		{
			SceneController.Instance.AnimationBegin();

			this.from = from;
			this.to = to;
			this.speed = speed;
			this.thickness = thickness;
			this.color = color;

			appearFactorLeft = 1.0f;
			material.AlbedoColor = color;
		}

		public void Initialize(Node3D from, Vector3 to, float thickness, float speed, Color color)
		{
			SceneController.Instance.AnimationBegin();

			this.from = from;
			this.to = null;
			toPos = to;
			this.speed = speed;
			this.thickness = thickness;
			this.color = color;

			appearFactorLeft = 1.0f;
			material.AlbedoColor = color;
		}

		public void Highlight()
		{
			highlightTimeLeft = 1.0f;
			material.AlbedoColor = new Color(color.R, color.G, color.B, 1.0f);
		}

		public override void _Process(double delta)
		{
			if (highlightTimeLeft > 0.0f)
			{
				highlightTimeLeft -= Convert.ToSingle(delta);
				if (highlightTimeLeft <= 0.0f)
				{
					material.AlbedoColor = color;
				}
			}
			
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

		public void Disappear()
		{
			QueueFree();
		}

		private void UpdatePositionRotation(float lengthScale)
		{
			if (from == null || (to == null && toPos == Vector3.Zero)) return;

			var currentTo = GetTo();
			var center = GetCenterPoint(currentTo);

			Transform = new Transform3D
			{
				Origin = from.Transform.Origin.Lerp(center, lengthScale),
				Basis = new Basis(Quaternion.Identity)
			};

			LookAt(currentTo, Vector3.Forward);

			var distance = (from.Transform.Origin - currentTo).Length();

			visual.Scale = new Vector3(thickness, distance * 0.5f * lengthScale, thickness);
		}

		private Vector3 GetTo()
		{
			if (to != null) return to.Transform.Origin;
			return toPos;
		}

		private Vector3 GetCenterPoint(Vector3 currentTo)
		{
			return new Vector3(
					(from.Transform.Origin.X + currentTo.X) / 2.0f,
					(from.Transform.Origin.Y + currentTo.Y) / 2.0f,
					(from.Transform.Origin.Z + currentTo.Z) / 2.0f
			);
		}
	}
}
