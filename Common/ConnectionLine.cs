using Godot;

namespace OverwatchTranscriptViewer.Common
{
	public partial class ConnectionLine : Node3D
	{
		private MeshInstance3D visual;
		private double delay;
		private Node3D from;
		private Node3D to;

		public override void _Ready()
		{
			visual = GetNode<MeshInstance3D>("Visual");
		}

		public override void _Process(double delta)
		{
			if (delay > 0.0) { delay -= delta; return; }
			delay = 2.0;

			UpdatePositionRotation();
		}

		private void UpdatePositionRotation()
		{
			if (from == null || to == null) return;

			Transform = new Transform3D
			{
				Origin = new Vector3(
					(from.Transform.Origin.X + to.Transform.Origin.X) / 2.0f,
					(from.Transform.Origin.Y + to.Transform.Origin.Y) / 2.0f,
					(from.Transform.Origin.Z + to.Transform.Origin.Z) / 2.0f
				),
				Basis = new Basis(Quaternion.Identity)
			};

			LookAt(to.Transform.Origin, Vector3.Forward);

			var distance = (from.Transform.Origin - to.Transform.Origin).Length();

			visual.Scale = new Vector3(0.1f, distance * 0.5f, 0.1f);
		}

		public void Initialize(Node3D from, Node3D to)
		{
			this.from = from;
			this.to = to;
		}
	}
}
