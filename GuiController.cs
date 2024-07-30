using Godot;
using OverwatchTranscriptViewer;
using System;

public partial class GuiController : Node
{
	public static GuiController Instance;

	private FileDialog fd;
	private ProgressBar bar;
	private EventsPanelController eventsPanel;
	private double autostart = 0.5;
		
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;

		eventsPanel = GetNode<EventsPanelController>("EventsPanelController");
		fd = GetNode<FileDialog>("OpenDialog");
		bar = GetNode<ProgressBar>("Panel/ProgressBar");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (autostart > 0.0)
		{
			autostart -= delta;
			if (autostart <= 0.0)
			{
				_on_open_button_pressed();
			}
		}
	}

	public void UpdateProgressBar(DateTime earliest, DateTime latest, DateTime current)
	{
		var totalSpan = latest - earliest;
		var process = current - earliest;
		var factor = process / totalSpan;

		bar.Value = 100.0 * factor;
	}
	
	public void _on_open_button_pressed()
	{
		//fd.Visible = true;
		
		var filepath = @"d:\Projects\cs-codex-dist-tests\Tests\CodexLongTests\bin\Debug\net7.0\CodexTestLogs\2024-07\29\12-10-59Z_MultiPeerDownloadTests\MultiPeerDownload[10,100]_000014.owts";
		
		SceneController.Instance.LoadTranscript(filepath);
	}

	public void _on_reset_camera_pressed()
	{
		CameraController.Instance.Reset();
	}

	public void _on_events_panel_pressed()
	{
		eventsPanel.Toggle();
	}
	
	public void _on_file_selected(string path)
	{
		GD.Print("file: " + path);
	}
}

