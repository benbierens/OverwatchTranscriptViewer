using Godot;

public partial class GuiController : Node
{
	private FileDialog fd;
	private double autostart = 2.0;
		
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		fd = GetNode<FileDialog>("OpenDialog");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (autostart > 0.0)
		{
			autostart -= delta;
			if (autostart <= 0.0)
			{
				_on_open_button_pressed();
			}
		}
	}
	
	public void _on_open_button_pressed()
	{
		//fd.Visible = true;
		
		var filepath = @"d:\Projects\cs-codex-dist-tests\Tests\CodexLongTests\bin\Debug\net7.0\CodexTestLogs\2024-07\29\09-03-24Z_MultiPeerDownloadTests\MultiPeerDownload[10,100]_000014.owts";
		
		SceneController.Instance.LoadTranscript(filepath);
	}
	
	public void _on_file_selected(string path)
	{
		GD.Print("file: " + path);
	}
}
