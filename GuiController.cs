using Godot;
using System;

public partial class GuiController : Node
{
	private FileDialog fd;
		
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		fd = GetNode<FileDialog>("OpenDialog");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void _on_open_button_pressed()
	{
		//fd.Visible = true;
		
		var filepath = @"d:\Projects\cs-codex-dist-tests\Tests\CodexTests\bin\Debug\net7.0\CodexTestLogs\2024-07\26\08-56-52Z_FullyConnectedDownloadTests\FullyConnectedDownloadTest[5,10]_000006.owts";
		
		SceneController.Instance.LoadTranscript(filepath);
	}
	
	public void _on_file_selected(string path)
	{
		GD.Print("file: " + path);
	}
}
