public static class NeighborConfigurations
{
    public static readonly PositionCoord3d[] MainConfig = new PositionCoord3d[]
    {
        //new PositionCoord3d(1, 1, 1),
        new PositionCoord3d(1, 1, 0),
        //new PositionCoord3d(1, 1, -1),

        new PositionCoord3d(1, 0, 1),
        new PositionCoord3d(1, 0, 0),
        new PositionCoord3d(1, 0, -1),

        //new PositionCoord3d(1, -1, 1),
        new PositionCoord3d(1, -1, 0),
        //new PositionCoord3d(1, -1, -1),
        
        ////////////////////////////////////////////////////////

        new PositionCoord3d(0, 1, 1),
        new PositionCoord3d(0, 1, 0),
        new PositionCoord3d(0, 1, -1),

        new PositionCoord3d(0, 0, 1),
        //new PositionCoord3d(0, 0, 0), remove center location
        new PositionCoord3d(0, 0, -1),

        new PositionCoord3d(0, -1, 1),
        new PositionCoord3d(0, -1, 0),
        new PositionCoord3d(0, -1, -1),

        ////////////////////////////////////////////////////////

        //new PositionCoord3d(-1, 1, 1),
        new PositionCoord3d(-1, 1, 0),
        //new PositionCoord3d(-1, 1, -1),

        new PositionCoord3d(-1, 0, 1),
        new PositionCoord3d(-1, 0, 0),
        new PositionCoord3d(-1, 0, -1),

        //new PositionCoord3d(-1, -1, 1),
        new PositionCoord3d(-1, -1, 0),
        //new PositionCoord3d(-1, -1, -1),
        
    };
}