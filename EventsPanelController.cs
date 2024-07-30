using Godot;
using OverwatchTranscript;
using OverwatchTranscriptViewer.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OverwatchTranscriptViewer
{
	public partial class EventsPanelController : Node
	{
		public static EventsPanelController Instance;

		private static PackedScene itemTemplate;
		private Panel panel;
		private bool visible = false;
		private VBoxContainer container;
		private readonly List<MomentPanel> panels = new List<MomentPanel>();

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

			reader.PreviewMoments(PreviewMoment);
		}

		public void Toggle()
		{
			visible = !visible;
			panel.Visible = visible;
		}

		private bool PreviewMoment(OverwatchMoment m)
		{
			AddItem(GetMomentHeader(m), GetMomentEvents(m));
			return true;
		}

		private string GetMomentHeader(OverwatchMoment m)
		{
			var u = m.Utc;
			return $"[{u.Hour}:{u.Minute}:{u.Second}.{u.Millisecond}] ({m.Events.Length})";
		}

		private string[] GetMomentEvents(OverwatchMoment m)
		{
			return m.Events.Select(FormatEvent).ToArray();
		}

		private string FormatEvent(OverwatchEvent e)
		{
			return e.Type;
		}

		private void DeleteAll()
		{
			foreach (var p in panels)
			{
				p.QueueFree();
			}
			panels.Clear();
		}

		private void AddItem(string header, params string[] eventLines)
		{
			var instance = itemTemplate.Instantiate();
			container.AddChild(instance);
			var panel = (instance as MomentPanel);
			
			panel.Initialize(header, () =>
			{
				GD.Print("Click: " + header);
			}, eventLines);
		}
	}
}

