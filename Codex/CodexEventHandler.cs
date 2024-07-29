using Godot;
using System;
using CodexPlugin.OverwatchSupport;
using OverwatchTranscriptViewer;
using OverwatchTranscript;
using OverwatchTranscriptViewer.Common;
using OverwatchTranscriptViewer.Codex;

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
		if (@event.BootstrapConfig != null) Handle(@event, @event.BootstrapConfig);
		if (@event.FileUploaded != null) Handle(@event, @event.FileUploaded);
		if (@event.FileDownloaded != null) Handle(@event, @event.FileDownloaded);
		if (@event.BlockReceived != null) Handle(@event, @event.BlockReceived);
	}

	private void Handle(OverwatchCodexEvent @event, BlockReceivedEvent blockReceived)
	{
		SpawnTransferEvent(
			source: Lookup.Get<CodexNode>(blockReceived.SenderPeerId),
			target: Lookup.Get<CodexNode>(@event.PeerId),
			label: blockReceived.BlockAddress
		);
	}

	private void Handle(OverwatchCodexEvent @event, FileDownloadedEvent fileDownloaded)
	{
		SpawnFileEvent(
			target: Lookup.Get<CodexNode>(@event.PeerId),
			cid: fileDownloaded.Cid,
			backwards: true
		);
	}

	private void Handle(OverwatchCodexEvent @event, FileUploadedEvent fileUploaded)
	{
		SpawnFileEvent(
			target: Lookup.Get<CodexNode>(@event.PeerId),
			cid: fileUploaded.Cid,
			backwards: false
		);
	}

	private void Handle(OverwatchCodexEvent @event, BootstrapConfigEvent bootstrapConfig)
	{
		var from = Lookup.Get<CodexNode>(@event.PeerId);
		var to = Lookup.Get<CodexNode>(bootstrapConfig.BootstrapPeerId);

		var line = SpawnConnectionLine();
		line.Initialize(from, to, thickness: 0.08f, speed: 2.5f, new Color(0.2f, 0.2f, 0.2f, 0.4f), () =>
		{
			SceneController.Instance.Proceed();
		});
	}

	private void Handle(OverwatchCodexEvent @event, NodeStartingEvent nodeStarting)
	{
		var node = SpawnCodexNode();
		node.Starting(@event.Name, @event.PeerId);
	}

	private void Handle(OverwatchCodexEvent @event, NodeStartedEvent nodeStarted)
	{
		GetCodex(@event.Name).Started();
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

	private ConnectionLine SpawnConnectionLine()
	{
		var template = GD.Load<PackedScene>("res://Common/connection_line.tscn");
		var instance = template.Instantiate();
		AddChild(instance);
		return instance as ConnectionLine;
	}

	private void SpawnFileEvent(CodexNode target, string cid, bool backwards)
	{
		var template = GD.Load<PackedScene>("res://Codex/file_event.tscn");
		var instance = template.Instantiate();
		AddChild(instance);
		(instance as FileEvent).Initialize(target, cid, speed: 0.9f, backwards, whenDone: () =>
		{
			SceneController.Instance.Proceed();
		});
	}

	private void SpawnTransferEvent(CodexNode source, CodexNode target, string label)
	{
		var template = GD.Load<PackedScene>("res://Codex/transfer_event.tscn");
		var instance = template.Instantiate();
		AddChild(instance);
		(instance as TransferEvent).Initialize(source, target, label, speed: 1.0f, whenDone: () =>
		{
			SceneController.Instance.Proceed();
		});
	}
}
