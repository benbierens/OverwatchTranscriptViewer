using Godot;
using OverwatchTranscript;
using OverwatchTranscriptViewer.Common;
using System;
using System.Collections.Generic;

namespace OverwatchTranscriptViewer
{
	public partial class EventsPanelController : Node, IScriptEventHandler
	{
		public static EventsPanelController Instance;

		private static PackedScene itemTemplate;
		private Panel panel;
		private bool visible = false;
		private VBoxContainer container;
		private Label infoLabel;
		private BaseButton stepButton;
		private Label eventInfo;
		private readonly List<MomentPanel> panels = new List<MomentPanel>();

		public override void _Ready()
		{
			Instance = this;

			panel = GetNode<Panel>("EventsPanel");
			container = GetNode<VBoxContainer>("EventsPanel/ScrollContainer/VBoxContainer");
			infoLabel = GetNode<Label>("EventsPanel/InfoHeader");
			stepButton = GetNode<BaseButton>("EventsPanel/Button");
			eventInfo = GetNode<Label>("EventsPanel/EventInfo");

			panel.Visible = false;

			if (itemTemplate == null)
			{
				itemTemplate = GD.Load<PackedScene>("res://Common/moment_panel_container.tscn");
			}

			ApplyState(AppState.Empty);
			SceneController.Instance.AppStateChanged += ApplyState;
			SceneController.Instance.RegisterScriptEventHandler(this);
		}

		public void Initialize(ITranscriptReader reader, Placer placer)
		{
			reader.AddMomentHandler(HandleMoment);

			DeleteAll();

			var numberOfEvents = reader.Header.NumberOfEvents;
			var totalSpan = reader.Header.LatestUtc - reader.Header.EarliestUtc;
			var nl = System.Environment.NewLine;
			infoLabel.Text = $"Moments: {reader.Header.NumberOfMoments} - " +
				$"Events: {numberOfEvents}{nl}" +
				$"Total duration: {Utils.FormatDuration(totalSpan)}{nl}" +
				$"First event: {Utils.FormateDateTime(reader.Header.EarliestUtc)}{nl}" +
				$"Last event: {Utils.FormateDateTime(reader.Header.LatestUtc)}";
		}

		public void WaitingForAnim()
		{
			eventInfo.Text = "Waiting for animation...";
		}

		private void HandleMoment(ActivateMoment m)
		{
			SetCurrentMomentDuration(m.Duration);
		}

		public void Toggle()
		{
			visible = !visible;
			panel.Visible = visible;
		}

		public void AddEntry(DateTime utc, string header, params string[] lines)
		{
			AddItem(GetHeader(utc, header), lines);

			while (panels.Count > 20)
			{
				panels[0].QueueFree();
				panels.RemoveAt(0);
			}
		}

		public void SetCurrentMomentDuration(TimeSpan? momentDuration)
		{
			if (momentDuration.HasValue)
			{
				if (momentDuration.Value.TotalSeconds > AnimationConfig.MaxMomentDuration)
				{
					eventInfo.Text = $"{Utils.FormatDuration(momentDuration.Value)} ({AnimationConfig.MaxMomentDuration} secs)";
				}
				else
				{
					eventInfo.Text = Utils.FormatDuration(momentDuration.Value);
				}
			}
			else
			{
				eventInfo.Text = "Playback fininshed";
			}
		}

		public void _on_step_button_pressed()
		{
			SceneController.Instance.Step();
		}

		private string GetHeader(DateTime utc, string headerContent)
		{
			return $"[{Utils.FormateDateTime(utc)}] {headerContent}";
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
			panels.Add(panel);
			
			panel.Initialize(header, () =>
			{
				GD.Print("Click: " + header);
			}, eventLines);
		}

		private void ApplyState(AppState state)
		{
			stepButton.Disabled = state != AppState.Stopped;
		}
	}
}
