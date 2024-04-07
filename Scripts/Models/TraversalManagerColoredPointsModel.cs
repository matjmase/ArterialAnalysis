using System.Collections.Generic;
using Godot;

public struct TraversalManagerPointsModel
{
	public TraversalManagerPointsModel(float scale, Color color)
	{
		Scale = scale;
		Color = color;
		Points = new Dictionary<PositionCoord3d, MeshInstance3D>();
	}

	public Dictionary<PositionCoord3d, MeshInstance3D> Points;
	public Color Color;
	public float Scale;
}
