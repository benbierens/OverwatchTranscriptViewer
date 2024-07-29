using Godot;
using System;

public partial class rotates : MeshInstance3D
{
	public float Speed = 0.1f;
	public float TargetSpeed = 0.1f;
	public float SpeedChangeRate = 1.0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var d = Convert.ToSingle(delta);

		Rotate(new Vector3(0, 1, 0), d * Speed);

		if (Speed > TargetSpeed)
		{
			Speed -= d * SpeedChangeRate;
			if (Speed < TargetSpeed)
			{
				Speed = TargetSpeed;
			}
		}
		else if (Speed < TargetSpeed)
		{
			Speed += d * SpeedChangeRate;
			if (Speed > TargetSpeed)
			{
				Speed = TargetSpeed;
			}
		}
	}
}
