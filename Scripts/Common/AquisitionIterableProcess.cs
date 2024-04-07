using System;
using System.Collections.Generic;
using System.Diagnostics;
using static Dijkstra3dAnytime;

public abstract class AquisitionIterableProcess : ArchiveIterableProcess
{
    // internal collections - Keeping up with points
    protected Dictionary<IIteratableProcess, Dictionary<PositionCoord3d, PositionData3d>> _traversalAquisitions = new Dictionary<IIteratableProcess, Dictionary<PositionCoord3d, PositionData3d>>();
	protected Dictionary<PositionCoord3d, PositionData3d> _totalAquisitionPositions = new Dictionary<PositionCoord3d, PositionData3d>();

    protected override void SafeIterate()
    {
        base.SafeIterate();
        TraversalProcessing();
        if(IsComplete)
        {
            Dispose();
        }
    }

    protected virtual void TraversalProcessing()
    {}

    protected override void TraversalCreated(IIteratableProcess traversal)
    {
        if(traversal is Dijkstra3dAnytime)
        {
            _traversalAquisitions.Add(traversal, new Dictionary<PositionCoord3d, PositionData3d>());
        }

        base.TraversalCreated(traversal);
    }

    protected override void TraversalDestroy(IIteratableProcess traversal)
    {
        if(_traversalAquisitions.ContainsKey(traversal))
        {
            foreach(var point in _traversalAquisitions[traversal])
            {
                _totalAquisitionPositions.Remove(point.Key);
            }

            _traversalAquisitions.Remove(traversal);
        }

        base.TraversalDestroy(traversal);
    }

    protected override void PointEnterAquisition(IIteratableProcess traversal, PositionData3d point)
    {
        _traversalAquisitions[traversal].Add(point.Coord, point);
        _totalAquisitionPositions.Add(point.Coord, point);
        base.PointEnterAquisition(traversal, point);
    }

    protected override void PointEnterArchive(IIteratableProcess traversal, PositionData3d point)
    {
        _traversalAquisitions[traversal].Remove(point.Coord);
        _totalAquisitionPositions.Remove(point.Coord);
        base.PointEnterArchive(traversal, point);
    }
}