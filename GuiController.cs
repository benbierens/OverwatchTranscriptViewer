using Godot;
using OverwatchTranscriptViewer;
using System;

public partial class GuiController : Node
{
	public static GuiController Instance;

	private FileDialog fd;
	private ProgressBar timeBar;
	private ProgressBar eventsBar;
	private EventsPanelController eventsPanel;
	private BaseButton openButton;
	private OptionButton speedButton;
	private BaseButton playPauseButton;
	private DateTime earliestUtc;
	private TimeSpan totalSpan;
	private float totalEvents;
	private Label timeLabel;
	private Label eventsLabel;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;

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
	}

	public void Initialize(DateTime earliestUtc, DateTime latestUtc, long totalEvents)
	{
		this.earliestUtc = earliestUtc;
		totalSpan = latestUtc - earliestUtc;
		this.totalEvents = totalEvents;
	}

	public void UpdateProgressBar(DateTime currentTime, long currentEvent)
	{
		var process = currentTime - earliestUtc;
		var factor = process / totalSpan;
		timeBar.Value = 100.0 * factor;
		timeLabel.Text = $"Time: ({Short(process.TotalSeconds)} / {Short(totalSpan.TotalSeconds)})";

		float current = currentEvent;
		factor = current / totalEvents;
		eventsBar.Value = 100.0 * factor;
		eventsLabel.Text = $"Events: ({Short(current)} / {Short(totalEvents)})";
	}

	private string Short(double d)
	{
		return d.ToString("F1");
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
		var filepath = @"d:\Projects\cs-codex-dist-tests\Tests\CodexLongTests\bin\Debug\net7.0\CodexTestLogs\2024-07\29\12-10-59Z_MultiPeerDownloadTests\MultiPeerDownload[10,100]_000014.owts";

		SceneController.Instance.LoadTranscript(filepath);
	}

	public void _on_option_button_item_selected(long index)
	{
		var speed = 1.0f;
		if (index == 1) speed = 2.0f;
		else if (index == 2) speed = 5.0f;

		SceneController.Instance.UpdatePlaybackSpeed(speed);
	}

	private void ApplyState(AppState state)
	{
		openButton.Disabled = state != AppState.Empty;
		speedButton.Disabled = state != AppState.Stopped;
		playPauseButton.Disabled = state != AppState.Stopped && state != AppState.Playing;
		GD.Print("gui updated to " + state);
	}
}

