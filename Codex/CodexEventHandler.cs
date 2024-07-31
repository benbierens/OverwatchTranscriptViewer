using Godot;
using System;
using CodexPlugin.OverwatchSupport;
using OverwatchTranscriptViewer;
using OverwatchTranscript;
using OverwatchTranscriptViewer.Common;
using OverwatchTranscriptViewer.Codex;

public partial class CodexEventHandler : Node, IScriptEventHandler
{
	private Placer placer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SceneController.Instance.RegisterScriptEventHandler(this);
	}

	public void Initialize(ITranscriptReader reader, Placer placer)
	{
		this.placer = placer;

		var header = reader.GetHeader<OverwatchCodexHeader>("cdx_h");
		placer.SetMaxPlaces(header.TotalNumberOfNodes);

		reader.AddHandler<OverwatchCodexEvent>(HandleEvent);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void HandleEvent(DateTime utc, OverwatchCodexEvent @event)
	{
		if (@event.NodeStarting != null) Handle(utc, @event, @event.NodeStarting);
		if (@event.NodeStarted != null) Handle(utc, @event, @event.NodeStarted);
		if (@event.BootstrapConfig != null) Handle(utc, @event, @event.BootstrapConfig);
		if (@event.FileUploaded != null) Handle(utc, @event, @event.FileUploaded);
		if (@event.FileDownloaded != null) Handle(utc, @event, @event.FileDownloaded);
		if (@event.BlockReceived != null) Handle(utc, @event, @event.BlockReceived);
	}

	private void Handle(DateTime utc, OverwatchCodexEvent @event, BlockReceivedEvent blockReceived)
	{
		SpawnTransferEvent(
			source: GetCodex(blockReceived.SenderPeerId),
			target: GetCodex(@event.PeerId),
			label: blockReceived.BlockAddress
		);

		AddToPanel(utc, $"{@event.Name} received block.",
			"Block: " + blockReceived.BlockAddress,
			"Sender: " + blockReceived.SenderPeerId);
	}

	private void Handle(DateTime utc, OverwatchCodexEvent @event, FileDownloadedEvent fileDownloaded)
	{
		SpawnFileEvent(
			target: GetCodex(@event.PeerId),
			cid: fileDownloaded.Cid,
			backwards: true
		);

		AddToPanel(utc, $"{@event.Name} downloaded file.", "Cid: " + fileDownloaded.Cid);
	}

	private void Handle(DateTime utc, OverwatchCodexEvent @event, FileUploadedEvent fileUploaded)
	{
		SpawnFileEvent(
			target: GetCodex(@event.PeerId),
			cid: fileUploaded.Cid,
			backwards: false
		);

		AddToPanel(utc, $"{@event.Name} uploaded file.", "Cid: " + fileUploaded.Cid);
	}

	private void Handle(DateTime utc, OverwatchCodexEvent @event, BootstrapConfigEvent bootstrapConfig)
	{
		var from = GetCodex(@event.PeerId);
		var to = GetCodex(bootstrapConfig.BootstrapPeerId);

		var line = SpawnConnectionLine();
		line.Initialize(from, to, thickness: 0.08f, speed: 2.5f, new Color(0.2f, 0.2f, 0.2f, 0.4f), () =>
		{
			SceneController.Instance.Proceed();
		});

		AddToPanel(utc, $"{@event.Name} bootstrapped.", "bootnode: " + bootstrapConfig.BootstrapPeerId);
	}

	private void Handle(DateTime utc, OverwatchCodexEvent @event, NodeStartingEvent nodeStarting)
	{
		var node = SpawnCodexNode();
		node.Starting(@event.Name, @event.PeerId);

		AddToPanel(utc, $"{@event.Name} is starting...");
	}

	private void Handle(DateTime utc, OverwatchCodexEvent @event, NodeStartedEvent nodeStarted)
	{
		GetCodex(@event.Name).Started();

		AddToPanel(utc, $"{@event.Name} has started.");
	}

	private void AddToPanel(DateTime utc, string msg, params string[] lines)
	{
		EventsPanelController.Instance.AddEntry(utc, msg, lines);
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
		(instance as FileEvent).Initialize(target, cid, speed: 0.4f, backwards, whenDone: () =>
		{
			SceneController.Instance.Proceed();
		});
	}

	private void SpawnTransferEvent(CodexNode source, CodexNode target, string label)
	{
		var template = GD.Load<PackedScene>("res://Codex/transfer_event.tscn");
		var instance = template.Instantiate();
		AddChild(instance);
		(instance as TransferEvent).Initialize(source, target, label, speed: 2.0f, whenDone: () =>
		{
			SceneController.Instance.Proceed();
		});
	}
}
