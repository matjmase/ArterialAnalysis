using Godot;
using System;

public partial class MobileCamera : Camera3D
{
	[Export]
	public float _movementSpeed = 3.0f;
	
	private const float _mouseSensitivity = 0.1f;

	private Vector2 _movementUser = new Vector2();

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		this.Position += new Vector3(-Mathf.Cos(this.Rotation.X) * Mathf.Sin(this.Rotation.Y), Mathf.Sin(this.Rotation.X), -Mathf.Cos(this.Rotation.X) * Mathf.Cos(this.Rotation.Y)) * _movementUser.Y * Convert.ToSingle(delta) * _movementSpeed; 
		this.Position += new Vector3(Mathf.Sin(this.Rotation.Y + Mathf.Pi / 2), 0, Mathf.Cos(this.Rotation.Y  + Mathf.Pi / 2)) * _movementUser.X * Convert.ToSingle(delta) * _movementSpeed; 
	}

    public override void _Input(InputEvent @event)
    {
		if(@event is InputEventMouseButton mb)
		{
			if(mb.ButtonIndex == MouseButton.Left && mb.Pressed)
			{
				Input.MouseMode = Input.MouseModeEnum.Captured;
			}
			else if(mb.ButtonIndex == MouseButton.Left && !mb.Pressed)
			{
				Input.MouseMode = Input.MouseModeEnum.Visible;
			}
		}

		if(Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			if(@event is InputEventMouseMotion mm)
			{
				this.Rotation -= new Vector3(Mathf.DegToRad(mm.Relative.Y), Mathf.DegToRad(mm.Relative.X), 0) * _mouseSensitivity;
			}
		}

		if(@event is InputEventKey ek)
			{
				var lastMove = _movementUser;
				if(ek.Pressed)
				{
					if(ek.Keycode == Key.W)
					{
						_movementUser = new Vector2(lastMove.X, 1);
					}
					else if(ek.Keycode == Key.S)
					{
						_movementUser = new Vector2(lastMove.X, -1);
					}
					else if(ek.Keycode == Key.D)
					{
						_movementUser = new Vector2(1, lastMove.Y);
					}
					else if(ek.Keycode == Key.A)
					{
						_movementUser = new Vector2(-1, lastMove.Y);
					}
				}
				else
				{
					if(ek.Keycode == Key.W || ek.Keycode == Key.S)
					{
						_movementUser = new Vector2(lastMove.X, 0);
					}
					else if(ek.Keycode == Key.D || ek.Keycode == Key.A)
					{
						_movementUser = new Vector2(0, lastMove.Y);
					}
				}
			}
        base._Input(@event);
    }
}
