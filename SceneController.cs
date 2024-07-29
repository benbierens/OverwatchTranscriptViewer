using Godot;
using OverwatchTranscript;
using CodexPlugin.OverwatchSupport;
using OverwatchTranscriptViewer;

public partial class SceneController : Node
{
	public static SceneController Instance;
	
	private ITranscriptReader reader;
	private readonly Placer placer = new Placer();
	private bool running;
	private bool hold;
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
			if (hold)
			{
				time = 2.0f;
			}
			else
			{
				time -= 0.2;
				reader.Next();
				hold = true;
			}
		}
	}
	
	public void LoadTranscript(string filepath)
	{
		GD.Print("yeah: " + filepath);
		
		reader = Transcript.NewReader(filepath);
		
		var codexHandler = GetNode<Node>("CodexEventHandler") as CodexEventHandler;
		codexHandler.Initialize(reader, placer);
		
		reader.AddHandler<OverwatchCodexEvent>((utc, e) => codexHandler.HandleEvent(utc, e));
		
		running = true;
		time = 0.0;
	}

	public void Proceed()
	{
		hold = false;
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
