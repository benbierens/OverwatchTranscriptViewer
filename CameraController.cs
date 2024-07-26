using Godot;
using System;

public partial class CameraController : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            switch (mouseEvent.ButtonIndex)
            {
                case MouseButton.WheelDown:
                    Translate(new Vector3(0.0f, 0.0f, 0.1f));
                    break;
                case MouseButton.WheelUp:
                    Translate(new Vector3(0.0f, 0.0f, -0.1f));
                    break;
            }
        }
    }
}
