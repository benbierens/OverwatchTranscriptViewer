using Godot;
using OverwatchTranscript;
using OverwatchTranscriptViewer.Common;
using System;

namespace OverwatchTranscriptViewer
{
	public partial class EventsPanelController : Node
	{
		public static EventsPanelController Instance;

		private static PackedScene itemTemplate;
		private Panel panel;
		private bool visible = false;
		private VBoxContainer container;

		public override void _Ready()
		{
			Instance = this;

			panel = GetNode<Panel>("EventsPanel");
			container = GetNode<VBoxContainer>("EventsPanel/ScrollContainer/VBoxContainer");

			panel.Visible = false;

			if (itemTemplate == null)
			{
				itemTemplate = GD.Load<PackedScene>("res://Common/moment_panel_container.tscn");
			}
		}

		public void Initialize(ITranscriptReader reader)
		{
			DeleteAll();

			//reader.PreviewEvents(PreviewEvent);

			for (var i = 0; i < 100; i++)
			{
				AddItem("header " + i, "event_1_" + i, "event_2_" + i);
			}
		}

		public void Toggle()
		{
			visible = !visible;
			panel.Visible = visible;
		}

		//private bool PreviewEvent(OverwatchEvent e, DateTime utc)
		//{
		//	return true;
		//}

		private void DeleteAll()
		{
		}

		private void AddItem(string header, params string[] eventLines)
		{
			var instance = itemTemplate.Instantiate();
			container.AddChild(instance);
			(instance as MomentPanel).Initialize(header, () => {
				GD.Print("Click: " + header);
			}, eventLines);
		}
	}
}

