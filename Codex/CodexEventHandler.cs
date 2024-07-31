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

		reader.AddEventHandler<OverwatchCodexEvent>(HandleEvent);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void HandleEvent(ActivateEvent<OverwatchCodexEvent> activateEvent)
	{
		var m = activateEvent.Moment;
		var @event = activateEvent.Payload;

		if (@event.NodeStarting != null) Handle(m, @event, @event.NodeStarting);
		if (@event.NodeStarted != null) Handle(m, @event, @event.NodeStarted);
		if (@event.BootstrapConfig != null) Handle(m, @event, @event.BootstrapConfig);
		if (@event.FileUploaded != null) Handle(m, @event, @event.FileUploaded);
		if (@event.FileDownloaded != null) Handle(m, @event, @event.FileDownloaded);
		if (@event.BlockReceived != null) Handle(m, @event, @event.BlockReceived);
	}

	private void Handle(ActivateMoment moment, OverwatchCodexEvent @event, BlockReceivedEvent blockReceived)
	{
		SpawnTransferEvent(
			source: GetCodex(blockReceived.SenderPeerId),
			target: GetCodex(@event.PeerId),
			label: blockReceived.BlockAddress,
			totalTime: SceneController.Instance.Player.ApplySpeedToDuration(moment.Duration)
		);

		AddToPanel(moment, $"{@event.Name} received block.",
			"Block: " + blockReceived.BlockAddress,
			"Sender: " + blockReceived.SenderPeerId);
	}

	private void Handle(ActivateMoment moment, OverwatchCodexEvent @event, FileDownloadedEvent fileDownloaded)
	{
		SpawnFileEvent(
			target: GetCodex(@event.PeerId),
			cid: fileDownloaded.Cid,
			backwards: true,
			totalTime: SceneController.Instance.Player.ApplySpeedToDuration(moment.Duration)
		);

		AddToPanel(moment, $"{@event.Name} downloaded file.", "Cid: " + fileDownloaded.Cid);
	}

	private void Handle(ActivateMoment moment, OverwatchCodexEvent @event, FileUploadedEvent fileUploaded)
	{
		SpawnFileEvent(
			target: GetCodex(@event.PeerId),
			cid: fileUploaded.Cid,
			backwards: false,
			totalTime: SceneController.Instance.Player.ApplySpeedToDuration(moment.Duration)
		);

		AddToPanel(moment, $"{@event.Name} uploaded file.", "Cid: " + fileUploaded.Cid);
	}

	private void Handle(ActivateMoment moment, OverwatchCodexEvent @event, BootstrapConfigEvent bootstrapConfig)
	{
		var from = GetCodex(@event.PeerId);
		var to = GetCodex(bootstrapConfig.BootstrapPeerId);

		var line = SpawnConnectionLine();
		line.Initialize(from, to, thickness: 0.08f, speed: 2.5f, new Color(0.2f, 0.2f, 0.2f, 0.4f));

		AddToPanel(moment, $"{@event.Name} bootstrapped.", "bootnode: " + bootstrapConfig.BootstrapPeerId);
	}

	private void Handle(ActivateMoment moment, OverwatchCodexEvent @event, NodeStartingEvent nodeStarting)
	{
		var node = SpawnCodexNode();
		node.Starting(@event.Name, @event.PeerId);

		AddToPanel(moment, $"{@event.Name} is starting...");
	}

	private void Handle(ActivateMoment moment, OverwatchCodexEvent @event, NodeStartedEvent nodeStarted)
	{
		GetCodex(@event.Name).Started();

		AddToPanel(moment, $"{@event.Name} has started.");
	}

	private void AddToPanel(ActivateMoment m, string msg, params string[] lines)
	{
		EventsPanelController.Instance.AddEntry(m.Utc, msg, lines);
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

	private void SpawnFileEvent(CodexNode target, string cid, bool backwards, TimeSpan? totalTime)
	{
		var duration = totalTime.HasValue ? totalTime.Value.TotalSeconds : 3.0;
		var template = GD.Load<PackedScene>("res://Codex/file_event.tscn");
		var instance = template.Instantiate();
		AddChild(instance);
		(instance as FileEvent).Initialize(target, cid, backwards, duration);
		(instance as Node3D).Transform = new Transform3D
		{
			Origin = new Vector3(10.0f, 10.0f, 10.0f),
			Basis = new Basis(Quaternion.Identity)
		};
	}

	private void SpawnTransferEvent(CodexNode source, CodexNode target, string label, TimeSpan? totalTime)
	{
		var duration = totalTime.HasValue ? totalTime.Value.TotalSeconds : 0.5;
		var template = GD.Load<PackedScene>("res://Codex/transfer_event.tscn");
		var instance = template.Instantiate();
		AddChild(instance);

		(instance as TransferEvent).Initialize(source, target, label, totalTime: duration);
	}
}
