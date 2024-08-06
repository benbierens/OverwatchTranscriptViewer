using Godot;
using System;
using CodexPlugin.OverwatchSupport;
using OverwatchTranscriptViewer;
using OverwatchTranscript;
using OverwatchTranscriptViewer.Common;
using OverwatchTranscriptViewer.Codex;
using Godot.Collections;

public partial class CodexEventHandler : Node, IScriptEventHandler
{
	private readonly Dictionary<string, ConnectionLine> dialConnections = new Dictionary<string, ConnectionLine>();
	private readonly Dictionary<string, FileEvent> fileEvents = new Dictionary<string, FileEvent>();
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
		if (@event.FileUploading != null) Handle(m, @event, @event.FileUploading);
		if (@event.FileUploaded != null) Handle(m, @event, @event.FileUploaded);
		if (@event.FileDownloading != null) Handle(m, @event, @event.FileDownloading);
		if (@event.FileDownloaded != null) Handle(m, @event, @event.FileDownloaded);
		if (@event.BlockReceived != null) Handle(m, @event, @event.BlockReceived);
		if (@event.PeerDropped != null) Handle(m, @event, @event.PeerDropped);
		if (@event.NodeStopping != null) Handle(m, @event, @event.NodeStopping);
		if (@event.DialSuccessful != null) Handle(m, @event, @event.DialSuccessful);
	}

	private void Handle(ActivateMoment m, OverwatchCodexEvent @event, PeerDroppedEvent peerDropped)
	{
		var lineid = GetLineId(@event.PeerId, peerDropped.DroppedPeerId);
		var line = dialConnections[lineid];
		dialConnections.Remove(lineid);
		line.Disappear();

		AddToPanel(m, $"{@event.Name} is dropping a peer.", "peer: " + peerDropped.DroppedPeerId);
	}

	private void Handle(ActivateMoment m, OverwatchCodexEvent @event, NodeStoppingEvent nodeStopping)
	{
		var node = GetCodex(@event.PeerId);
		node.Stopping();

		AddToPanel(m, $"{@event.Name} is stopping.");
	}

	private void Handle(ActivateMoment m, OverwatchCodexEvent @event, PeerDialSuccessfulEvent dialSuccessful)
	{
		var from = GetCodex(@event.PeerId);
		var to = GetCodex(dialSuccessful.TargetPeerId);

		var lineId = GetLineId(from.PeerId, to.PeerId);
		if (dialConnections.ContainsKey(lineId))
		{
			dialConnections[lineId].Highlight();
			AddToPanel(m, $"{@event.Name} dialed peer again!", "peer: " + dialSuccessful.TargetPeerId);
			return;
		}

		var line = SpawnConnectionLine();
		line.Initialize(from, to, thickness: 0.12f, speed: 3.5f, new Color(0.5f, 0.5f, 0.8f, 0.6f));

		dialConnections.Add(lineId, line);

		AddToPanel(m, $"{@event.Name} successfull dialed.", "peer: " + dialSuccessful.TargetPeerId);
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

	private void Handle(ActivateMoment m, OverwatchCodexEvent @event, FileDownloadingEvent fileDownloading)
	{
		SpawnFileEvent(
			id: GetFileId(@event),
			target: GetCodex(@event.PeerId),
			cid: fileDownloading.Cid,
			isDownload: true,
			totalTime: SceneController.Instance.Player.ApplySpeedToDuration(m.Duration)
		);

		AddToPanel(m, $"{@event.Name} starts downloading file.", "Cid: " + fileDownloading.Cid);
	}

	private void Handle(ActivateMoment moment, OverwatchCodexEvent @event, FileDownloadedEvent fileDownloaded)
	{
		var fileEvent = fileEvents[GetFileId(@event)];
		fileEvent.Finish();

		AddToPanel(moment, $"{@event.Name} finished downloading file.", "Cid: " + fileDownloaded.Cid);
	}

	private void Handle(ActivateMoment m, OverwatchCodexEvent @event, FileUploadingEvent fileUploading)
	{
		SpawnFileEvent(
			id: GetFileId(@event),
			target: GetCodex(@event.PeerId),
			cid: "",
			isDownload: false,
			totalTime: SceneController.Instance.Player.ApplySpeedToDuration(m.Duration)
		);

		AddToPanel(m, $"{@event.Name} starts uploading file.");
	}

	private void Handle(ActivateMoment moment, OverwatchCodexEvent @event, FileUploadedEvent fileUploaded)
	{
		var fileEvent = fileEvents[GetFileId(@event)];
		fileEvent.Finish();

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

	private void SpawnFileEvent(string id, CodexNode target, string cid, bool isDownload, TimeSpan? totalTime)
	{
		var duration = totalTime.HasValue ? totalTime.Value.TotalSeconds : 3.0;
		var template = GD.Load<PackedScene>("res://Codex/file_event.tscn");
		var instance = template.Instantiate();
		AddChild(instance);
		(instance as FileEvent).Initialize(target, cid, isDownload, duration);
		(instance as Node3D).Transform = new Transform3D
		{
			Origin = new Vector3(10.0f, 10.0f, 10.0f),
			Basis = new Basis(Quaternion.Identity)
		};

		fileEvents.Add(id, (FileEvent)instance);
	}

	private void SpawnTransferEvent(CodexNode source, CodexNode target, string label, TimeSpan? totalTime)
	{
		var duration = totalTime.HasValue ? totalTime.Value.TotalSeconds : 0.5;
		var template = GD.Load<PackedScene>("res://Codex/transfer_event.tscn");
		var instance = template.Instantiate();
		AddChild(instance);

		(instance as TransferEvent).Initialize(source, target, label, totalTime: duration);
	}

	private string GetLineId(string a, string b)
	{
		if (string.Compare(a, b) > 0)
		{
			return a + b;
		}
		return b + a;
	}

	private string GetFileId(OverwatchCodexEvent @event)
	{
		if (@event.FileUploading != null) return @event.PeerId + @event.FileUploading.UniqueId;
		if (@event.FileUploaded != null) return @event.PeerId + @event.FileUploaded.UniqueId;
		if (@event.FileDownloading != null) return @event.PeerId + @event.FileDownloading.Cid;
		if (@event.FileDownloaded != null) return @event.PeerId + @event.FileDownloaded.Cid;
		throw new Exception("No file event in CodexEvent");
	}
}
