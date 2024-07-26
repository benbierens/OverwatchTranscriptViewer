using Godot;
using System;
using OverwatchTranscript;
using CodexPlugin.OverwatchSupport;

public partial class SceneController : Node
{
	public static SceneController Instance;
	
	private ITranscriptReader reader;
	private bool running;
	private double time;
	
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
		
		time += delta;
		if (time > 0.2)
		{
			time -= 0.2;
			reader.Next();
		}
	}
	
	public void LoadTranscript(string filepath)
	{
		GD.Print("yeah: " + filepath);
		
		reader = Transcript.NewReader(filepath);
		
		var codexHandler = GetNode<Node>("CodexEventHandler") as CodexEventHandler;
		
		reader.AddHandler<OverwatchCodexEvent>((utc, e) => codexHandler.HandleEvent(utc, e));
		
		running = true;
		time = 0.0;
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
