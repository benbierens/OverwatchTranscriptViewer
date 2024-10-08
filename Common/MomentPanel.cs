using Godot;
using System;

namespace OverwatchTranscriptViewer.Common
{
	public partial class MomentPanel : Node
	{
		private static PackedScene labelTemplate;
		private VBoxContainer container;
		private Action onClicked;
		private BaseButton button;

		public override void _Ready()
		{
			container = GetNode<VBoxContainer>("VBoxContainer");
			button = GetNode<BaseButton>("Button");
			button.Disabled = true;
			button.Visible = false;

			if (labelTemplate == null)
			{
				labelTemplate = GD.Load<PackedScene>("res://Common/moment_panel_label.tscn");
			}
		}

		public void Initialize(string momentHeader, Action onClicked, params string[] eventLines)
		{
			this.onClicked = onClicked;
			AddLabel(momentHeader);
			foreach (var l in eventLines)
			{
				AddLabel("   " + l);
			}
		}

		public void SetEnabled(bool enabled)
		{
			button.Disabled = !enabled;
		}

		public void _on_button_pressed()
		{
			GD.Print("Button click");
			onClicked();
		}

		private void AddLabel(string label)
		{
			var instance = labelTemplate.Instantiate();
			container.AddChild(instance);
			(instance as Label).Text = label;
		}
	}
}
