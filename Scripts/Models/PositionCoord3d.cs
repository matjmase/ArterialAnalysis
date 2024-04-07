
using System;
using Godot;

public struct PositionCoord3d
{
    public int X;
    public int Y;
    public int Z;

    public PositionCoord3d(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static bool operator ==(PositionCoord3d input1, PositionCoord3d input2)
    {
        return input1.X == input2.X && input1.Y == input2.Y && input1.Z == input2.Z;
    }

    public static bool operator !=(PositionCoord3d input1, PositionCoord3d input2)
    {
        return !(input1 == input2);
    }

    public static PositionCoord3d operator +(PositionCoord3d input1, PositionCoord3d input2)
    {
        return new PositionCoord3d() { X = input1.X + input2.X, Y = input1.Y + input2.Y, Z = input1.Z + input2.Z };
    }

    public static PositionCoord3d operator -(PositionCoord3d input1, PositionCoord3d input2)
    {
        return new PositionCoord3d() { X = input1.X - input2.X, Y = input1.Y - input2.Y, Z = input1.Z - input2.Z };
    }

    public static PositionCoord3d operator *(PositionCoord3d input1, PositionCoord3d input2)
    {
        return new PositionCoord3d() { X = input1.X * input2.X, Y = input1.Y * input2.Y, Z = input1.Z * input2.Z };
    }

    public PositionCoord3dFloat ToFloat()
    {
        return new PositionCoord3dFloat() {
            X = X,
            Y = Y,
            Z = Z
        };
    }

    public static float CalculateDist(PositionCoord3d from, PositionCoord3d to)
    {
        return Mathf.Sqrt(Mathf.Pow(from.X - to.X, 2) + Mathf.Pow(from.Y - to.Y, 2) + Mathf.Pow(from.Z - to.Z, 2));
    }
}