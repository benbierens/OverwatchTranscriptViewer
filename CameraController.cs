using Godot;

public partial class CameraController : Node3D
{
	public static CameraController Instance { get; private set; }

	private const float speed = 0.5f;
	private const float moveSpeed = 0.015f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void Reset()
	{
		Transform = new Transform3D
		{
			Origin = new Vector3(0, 0, 5.0f),
			Basis = new Basis(Quaternion.Identity)
		};
	}

	private bool lastPressed = false;

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent)
		{
			HandleScroll(mouseEvent);

			if (mouseEvent.ButtonIndex == MouseButton.Right)
			{
				lastPressed = mouseEvent.Pressed;
			}
		}
		if (@event is InputEventMouseMotion motion)
		{
			if (lastPressed)
			{
				var delta = motion.Relative * moveSpeed;
				Translate(new Vector3(-delta.X, delta.Y, 0.0f));
			}
		}
	}

	private void HandleScroll(InputEventMouseButton mouseEvent)
	{
		switch (mouseEvent.ButtonIndex)
		{
			case MouseButton.WheelDown:
				Translate(new Vector3(0.0f, 0.0f, speed));
				break;
			case MouseButton.WheelUp:
				Translate(new Vector3(0.0f, 0.0f, -speed));
				break;
		}
	}
}
