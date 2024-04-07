using System;
using System.Collections.Generic;
using System.Diagnostics;

public class BridgeDijkstra : MultipleRelayIterableProcess
{
    private Dijkstra3dAnytime _radiateSphere;
    private IIteratableProcess _multiIsTraversing;

    protected HashSet<PositionCoord3d> _totalArchive = new HashSet<PositionCoord3d>();
    protected HashSet<PositionCoord3d> _newArchive = new HashSet<PositionCoord3d>();

    public override bool IsComplete => _radiateSphere.IsComplete && base.IsComplete;

    protected override void PostInitialize()
    {
        foreach(var arch in _model.ArchivedPoints)
        {
            _totalArchive.Add(arch);
        }

        _radiateSphere = new Dijkstra3dAnytime();
        SubscribeSphere(_radiateSphere);
        _radiateSphere.Initialize(_model.ChangeValiditiy(RadiateSphereNeighborValidity).ChangeArchive(new HashSet<PositionCoord3d>()));
    }

    protected override void SafeIterate()
    {
        if(_multiIsTraversing == null && !_radiateSphere.IsComplete)
        {
            _radiateSphere.Iterate();
        }
        base.SafeIterate();
    }

    public override void Dispose()
    {
        UnsubscribeSphere(_radiateSphere);
        base.Dispose();
    }

    private void SubscribeSphere(Dijkstra3dAnytime dijkstra)
    {
        dijkstra.OnPointEnterArchive += SphereArchive;
    }

    private void UnsubscribeSphere(Dijkstra3dAnytime dijkstra)
    {
        dijkstra.OnPointEnterArchive -= SphereArchive;
    }

    protected override void TraversalDestroy(IIteratableProcess traversal)
    {
        if(traversal is MultiPassDijkstra)
        {
            _multiIsTraversing = null;
            DumpNewArchive();
        }
        base.TraversalDestroy(traversal);
    }
    protected override void PointEnterArchive(IIteratableProcess traversal, PositionData3d point)
    {   
        _newArchive.Add(point.Coord);
        base.PointEnterArchive(traversal, point);
        base.PointEnterArchive(this, point);
    }

    protected override void PointEnterAquisition(IIteratableProcess traversal, PositionData3d point)
    {
        if(_totalArchive.Contains(point.Coord))
        {
            Debug.WriteLine("Terminate Multi");
            RemoveProcess(_multiIsTraversing);
            _multiIsTraversing = null;

            DumpNewArchive();
        }
        else
        {
            base.PointEnterAquisition(traversal, point);
            base.PointEnterAquisition(this, point);
        }
    }

    private void DumpNewArchive()
    {
        Debug.WriteLine("DUMP!");
        foreach(var newArch in _newArchive)
        {
            _totalArchive.Add(newArch);
        }

        _newArchive.Clear();
    }

    private void SphereArchive(IIteratableProcess traversal, PositionData3d point)
    {
        if(HasNotBeenTraversed(point.Coord))
        {
            _multiIsTraversing = new MultiPassDijkstra();
            AddProcess(_multiIsTraversing, _model.NewStart(point));
        }
    }
    
    private bool HasNotBeenTraversed(PositionCoord3d to)
    {
        return !_newArchive.Contains(to) && !_totalArchive.Contains(to);
    }

    private bool RadiateSphereNeighborValidity(PositionCoord3d from, PositionCoord3d to)
    {
        var dist = PositionCoord3d.CalculateDist(_model.StartPoint, to);
        return dist <= _model.BridgeRadius;
    }
}