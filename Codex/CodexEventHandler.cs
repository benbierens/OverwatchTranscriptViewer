using Godot;
using System;
using CodexPlugin.OverwatchSupport;

public partial class CodexEventHandler : Node
{
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
		GD.Print("handling codex event!");
	}
}
