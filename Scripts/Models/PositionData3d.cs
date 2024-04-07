
public struct PositionData3d
{
    public PositionCoord3d Coord;
    public double DistanceFromStart;
    public PositionData3d(PositionCoord3d coord, double distanceFromStart)
    {
        Coord = coord;
        DistanceFromStart = distanceFromStart;
    }
    public PositionData3d(PositionCoord3d coord)
    {
        Coord = coord;
        DistanceFromStart = 0;
    }
}