using Godot;
using OverwatchTranscriptViewer.Common;
using System;

namespace OverwatchTranscriptViewer.Codex
{
	public partial class FileEvent : Node3D
	{
		private Vector3 sourcePosition = Vector3.Zero;
		private Node3D target;
		private Node3D visual;
		private float factor;
		private bool isDownload;
		private bool isFinished;
		private double timeLeft;
		private double totalTime;
		private ConnectionLine line;

		public override void _Ready()
		{
			visual = GetNode<Node3D>("Visual");
		}

		public void Initialize(Node3D target, Node3D lineParent, string cid, bool isDownload, double duration)
		{
			this.target = target;
			sourcePosition = target.Transform.Origin * 2.0f;

			var template = GD.Load<PackedScene>("res://Common/connection_line.tscn");
			var instance = template.Instantiate();
			lineParent.AddChild(instance);
			line = instance as ConnectionLine;
			line.Initialize(target, sourcePosition, 0.1f, 3.0f, new Color(1.0f, 1.0f, 1.0f, 1.0f));

			if (duration < 1.0) duration = 1.0;
			timeLeft = duration;
			totalTime = duration;
			this.isDownload = isDownload;
			factor = 0.0f;
			isFinished = false;
			visual.GetNode<Label3D>("Label3D").Text = CodexUtils.ToShortId(cid);
		}

		public void Finish()
		{
			SceneController.Instance.AnimationBegin();
			isFinished = true;
		}
		
		public override void _Process(double delta)
		{
			if (isFinished)
			{
				timeLeft -= delta;
				if (timeLeft <= 0.0)
				{
					SceneController.Instance.AnimationFinished();
					line.QueueFree();
					QueueFree();
					return;
				}
			}

			float factor = 1.0f - Convert.ToSingle(timeLeft / totalTime);
			var targetPos = target.Transform.Origin.Lerp(sourcePosition, 0.1f);

			Transform = new Transform3D
			{
				Origin = isDownload ?
					targetPos.Lerp(sourcePosition, factor) :
					sourcePosition.Lerp(targetPos, factor),
				Basis = new Basis(Quaternion.Identity)
			};
		}
	}
}
