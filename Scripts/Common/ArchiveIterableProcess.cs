using System.Collections.Generic;

public abstract class ArchiveIterableProcess : MultipleRelayIterableProcess
{
	protected Dictionary<PositionCoord3d, PositionData3d> _archivedPositions = new Dictionary<PositionCoord3d, PositionData3d>();

    protected override void PointEnterArchive(IIteratableProcess traversal, PositionData3d point)
    {
        _archivedPositions.Add(point.Coord, point);
        base.PointEnterArchive(traversal, point);
    }

    protected void PointEnterArchive(IIteratableProcess traversal, PositionData3d point, bool updateCollection)
    {
        if(updateCollection)
        {
            _archivedPositions.Add(point.Coord, point);
        }
        
        base.PointEnterArchive(traversal, point);
    }
}