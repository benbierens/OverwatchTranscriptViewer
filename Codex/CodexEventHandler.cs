using Godot;
using System;
using CodexPlugin.OverwatchSupport;
using OverwatchTranscriptViewer;
using OverwatchTranscript;

public partial class CodexEventHandler : Node
{
	private Placer placer;

	public void Initialize(ITranscriptReader reader, Placer placer)
	{
		this.placer = placer;

		var header = reader.GetHeader<OverwatchCodexHeader>("cdx_h");

		placer.SetMaxPlaces(header.TotalNumberOfNodes);
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void HandleEvent(DateTime utc, OverwatchCodexEvent @event)
	{
		if (@event.NodeStarting != null) Handle(@event, @event.NodeStarting); 
		if (@event.NodeStarted != null) Handle(@event, @event.NodeStarted);
	}

	private void Handle(OverwatchCodexEvent @event, NodeStartingEvent nodeStarting)
	{
		var node = SpawnCodexNode();
		node.Starting(@event.Name);
		GD.Print("starting " + @event.Name);
	}

	private void Handle(OverwatchCodexEvent @event, NodeStartedEvent nodeStarted)
	{
		GetCodex(@event.Name).Started(@event.PeerId);
		GD.Print("started " + @event.Name);
	}

	private CodexNode GetCodex(string name)
	{
		return Lookup.Get<CodexNode>(name);
	}

	private CodexNode SpawnCodexNode()
	{
		var template = GD.Load<PackedScene>("res://Codex/codex_node.tscn");
		var instance = template.Instantiate();
		AddChild(instance);
		(instance as Node3D).Translate(placer.GetPlace());
		return instance as CodexNode;
	}
}
