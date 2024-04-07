using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

public struct PositionCoord3dFloat
{
    public float X;
    public float Y;
    public float Z;

    public PositionCoord3dFloat(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static bool operator ==(PositionCoord3dFloat input1, PositionCoord3dFloat input2)
    {
        return input1.X == input2.X && input1.Y == input2.Y && input1.Z == input2.Z;
    }

    public static bool operator !=(PositionCoord3dFloat input1, PositionCoord3dFloat input2)
    {
        return !(input1 == input2);
    }

    public static PositionCoord3dFloat operator +(PositionCoord3dFloat input1, PositionCoord3dFloat input2)
    {
        return new PositionCoord3dFloat() { X = input1.X + input2.X, Y = input1.Y + input2.Y, Z = input1.Z + input2.Z };
    }

    public static PositionCoord3dFloat operator -(PositionCoord3dFloat input1, PositionCoord3dFloat input2)
    {
        return new PositionCoord3dFloat() { X = input1.X - input2.X, Y = input1.Y - input2.Y, Z = input1.Z - input2.Z };
    }

    public static PositionCoord3dFloat operator /(PositionCoord3dFloat input1, float input2)
    {
        return new PositionCoord3dFloat() { X = input1.X / input2, Y = input1.Y / input2, Z = input1.Z / input2 };
    }

    public float Magnitude()
    {
        return Mathf.Sqrt(Mathf.Pow(X, 2) + Mathf.Pow(Y, 2) + Mathf.Pow(Z, 2));
    }

    public PositionCoord3d RoundToNearest()
    {
        return new PositionCoord3d((int)Math.Round(X), (int)Math.Round(Y), (int)Math.Round(Z));
    }

    public static PositionCoord3dFloat ComputeCentroid(IEnumerable<PositionCoord3d> collection)
    {
        var sum = new PositionCoord3d(0,0,0);

        foreach(var point in collection)
        {
            sum += point;
        }

        var avg = new PositionCoord3dFloat(sum.X, sum.Y, sum.Z) / collection.Count();
        return avg;
    }

    public static float CalculateDist(PositionCoord3dFloat from, PositionCoord3dFloat to)
    {
        return Mathf.Sqrt(Mathf.Pow(from.X - to.X, 2) + Mathf.Pow(from.Y - to.Y, 2) + Mathf.Pow(from.Z - to.Z, 2));
    }

    public static float Magnitude(PositionCoord3dFloat vect)
    {
        return Mathf.Sqrt(Mathf.Pow(vect.X, 2) + Mathf.Pow(vect.Y, 2) + Mathf.Pow(vect.Z, 2));
    }

    public static float AvgDistanceFromCentroid(IEnumerable<PositionCoord3d> collection)
    {
        return AvgDistanceFromCentroid(collection, ComputeCentroid(collection));
    }

    public static float MinDistanceFromCentroid(IEnumerable<PositionCoord3d> collection)
    {
        return MinDistanceFromCentroid(collection, ComputeCentroid(collection));
    }

    private static float AvgDistanceFromCentroid(IEnumerable<PositionCoord3d> points, PositionCoord3dFloat centroid)
    {
        var avg = points.Select(e => CalculateDist(e.ToFloat(), centroid)).Aggregate(0f, (s, i) => s+i);

        return avg;
    }

    private static float MinDistanceFromCentroid(IEnumerable<PositionCoord3d> points, PositionCoord3dFloat centroid)
    {
        var min = points.MostOrDefault((f,s) => CalculateDist(f.ToFloat(), centroid) < CalculateDist(s.ToFloat(), centroid));

        return CalculateDist(min.ToFloat(), centroid);
    }
}