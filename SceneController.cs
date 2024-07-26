using Godot;
using System;
using OverwatchTranscript;

public partial class SceneController : Node
{
	public static SceneController Instance;
	
	private ITranscriptReader reader;
	private bool running;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		running = false;
		Instance = this;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (!running) return;
		
		GD.Print("run");
	}
	
	public void LoadTranscript(string filepath)
	{
		GD.Print("yeah: " + filepath);
		
		reader = Transcript.NewReader(filepath);
		
		running = true;
	}

	public override void _Notification(int what)
	{
		if (what == NotificationWMCloseRequest)
		{
			running = false;
			if (reader != null)
			{
				reader.Close();
				GD.Print("Closed.");
			}
		}
	}
}
