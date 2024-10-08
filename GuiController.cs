using Godot;
using OverwatchTranscript;
using OverwatchTranscriptViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class GuiController : Node, IScriptEventHandler
{
	private FileDialog fd;
	private ProgressBar timeBar;
	private ProgressBar eventsBar;
	private EventsPanelController eventsPanel;
	private BaseButton openButton;
	private OptionButton speedButton;
	private BaseButton playPauseButton;
	private DateTime earliestUtc;
	private TimeSpan totalSpan;
	private float totalMoments;
	private Label timeLabel;
	private Label eventsLabel;

	[Export]
	public CheckBox LineViewOptionsToggle;

	[Export]
	public Panel LineViewOptionsPanel;

	[Export]
	public CheckButton KademliaPositionsCb;

	[Export]
	public CheckButton BootLinesCb;

	[Export]
	public CheckButton UpDownLinesCb;

	[Export]
	public CheckButton DialLinesCb;

	[Export]
	public CheckButton DHTLinesCb;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		eventsPanel = GetNode<EventsPanelController>("EventsPanelController");
		fd = GetNode<FileDialog>("OpenDialog");
		timeBar = GetNode<ProgressBar>("Panel/TimeProgressBar");
		eventsBar = GetNode<ProgressBar>("Panel/EventsProgressBar");
		openButton = GetNode<BaseButton>("Panel/Button");
		speedButton = GetNode<OptionButton>("Panel/OptionButton");
		playPauseButton = GetNode<BaseButton>("Panel/Button3");
		timeLabel = GetNode<Label>("Panel/TimeLabel");
		eventsLabel = GetNode<Label>("Panel/EventsLabel");

		ApplyState(AppState.Empty);
		SceneController.Instance.AppStateChanged += ApplyState;
		SceneController.Instance.RegisterScriptEventHandler(this);
	}

	public void Initialize(ITranscriptReader reader, Placer plaer)
	{
		earliestUtc = reader.Header.EarliestUtc;
		totalSpan = reader.Header.LatestUtc - earliestUtc;
		totalMoments = reader.Header.NumberOfMoments;

		reader.AddMomentHandler(HandleMoment);
	}

	public void _on_open_button_pressed()
	{
		fd.Visible = true;
	}

	public void _on_reset_camera_pressed()
	{
		CameraController.Instance.Reset();
	}

	public void _on_events_panel_pressed()
	{
		eventsPanel.Toggle();
	}

	public void _on_playpause_pressed()
	{
		SceneController.Instance.PlayPause();
	}
	
	public void _on_file_selected(string path)
	{
		if (!File.Exists(path)) return;

		path = JustOpenLatest();
		//@"C:\Projects\cs-codex-dist-tests\Tests\CodexTests\bin\Debug\net7.0\CodexTestLogs\2024-08\05\12-19-29Z_ThreeClientTest\SwarmTest_SwarmTest.owts";

		SceneController.Instance.LoadTranscript(path);
	}

	public void _on_option_button_item_selected(long index)
	{
		var speed = 1.0f;
		if (index == 1) speed = 2.0f;
		else if (index == 2) speed = 5.0f;

		SceneController.Instance.UpdatePlaybackSpeed(speed);
	}

	public void _on_check_box_pressed()
	{
		LineViewOptionsPanel.Visible = LineViewOptionsToggle.ButtonPressed;
	}

	public void _on_lines_checkbox_pressed()
	{
		SceneController.Instance.RaiseLineOptions(
			new LineOptionsEvent(
				KademliaPositionsCb.ButtonPressed,
				BootLinesCb.ButtonPressed,
				UpDownLinesCb.ButtonPressed,
				DialLinesCb.ButtonPressed,
				DHTLinesCb.ButtonPressed
			)
		);
	}

	private void HandleMoment(ActivateMoment m)
	{
		UpdateProgressBars(m.Utc, m.Index);
	}

	private void UpdateProgressBars(DateTime currentTime, long currentMoment)
	{
		var process = currentTime - earliestUtc;
		var factor = process / totalSpan;
		timeBar.Value = 100.0 * factor;
		timeLabel.Text = $"Time: ({Short(process.TotalSeconds)} / {Short(totalSpan.TotalSeconds)})";

		float current = currentMoment;
		factor = current / totalMoments;
		eventsBar.Value = 100.0 * factor;
		eventsLabel.Text = $"Moments: ({Short(current)} / {Short(totalMoments)})";
	}

	private string Short(double d)
	{
		return d.ToString("F1");
	}

	private void ApplyState(AppState state)
	{
		openButton.Disabled = state != AppState.Empty;
		speedButton.Disabled = state != AppState.Stopped;
		playPauseButton.Disabled = state != AppState.Stopped && state != AppState.Playing;
	}

	private string JustOpenLatest()
	{
		var utc = DateTime.MinValue;
		var latest = "";
		var toDo = new List<string> { "C:\\Projects\\cs-codex-dist-tests\\Tests", "D:\\Projects\\cs-codex-dist-tests\\Tests" };

		while (toDo.Any())
		{
			var path = toDo[0];
			toDo.RemoveAt(0);
			if (Directory.Exists(path))
			{
				toDo.AddRange(Directory.GetDirectories(path));

				var files = Directory.GetFiles(path).Where(f => f.ToLowerInvariant().EndsWith(".owts")).ToArray();
				foreach (var file in files)
				{
					var info = new FileInfo(file);
					if (info.LastWriteTimeUtc > utc)
					{
						utc = info.LastWriteTimeUtc;
						latest = file;
					}
				}
			}
		}
		return latest;
	}
}

public class LineOptionsEvent
{
	public LineOptionsEvent(
		bool kademliaPositions,
		bool showBootLines,
		bool showUpDownLines,
		bool showDialLines,
		bool showDHTLines)
	{
		KademliaPositions = kademliaPositions;
		ShowBootLines = showBootLines;
		ShowUpDownLines = showUpDownLines;
		ShowDialLines = showDialLines;
		ShowDHTLines = showDHTLines;
	}

	public bool KademliaPositions { get; }
	public bool ShowBootLines { get; }
	public bool ShowUpDownLines { get; }
	public bool ShowDialLines { get; }
	public bool ShowDHTLines { get; }
}
