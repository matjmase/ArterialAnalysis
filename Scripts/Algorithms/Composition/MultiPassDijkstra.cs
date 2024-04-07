using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class MultiPassDijkstra : MultipleRelayIterableProcess
{
    private Dijkstra3dAnytime _firstDijkstra;
    private bool _deployedSecond;
    private HashSet<EndPointSnapShot> _firstPassEndPoints = new HashSet<EndPointSnapShot>();
    private Dictionary<IIteratableProcess, HashSet<EndPointSnapShot>> _firstPassEndPointsAndTraversals = new Dictionary<IIteratableProcess, HashSet<EndPointSnapShot>>();
    private HashSet<EndPointSnapShot> _untraverseFirstPassEndpoints = new HashSet<EndPointSnapShot>();
    private HashSet<EndPointSnapShot> _traversedFirstPassEndPoints = new HashSet<EndPointSnapShot>();
    private HashSet<EndPointSnapShot> _finalEndPoints = new HashSet<EndPointSnapShot>();
    
    private Dictionary<IIteratableProcess, Dictionary<PositionCoord3d, PositionData3d>> _maxExceeds = new Dictionary<IIteratableProcess, Dictionary<PositionCoord3d, PositionData3d>>();
    private HashSet<PositionData3d> _nextPassEndPoints = new HashSet<PositionData3d>();
    protected override void PostInitialize()
    {
        PerformFirstPass();
    }
    private void PerformFirstPass()
    {
        var choke = new ChokeChainDijkstra();
        AddProcess(choke, _model.ChangeScale(1));
    }
    private void PerformNextPass(PositionData3d point)
    {
        _nextPassEndPoints.Add(point);

        _untraverseFirstPassEndpoints = _firstPassEndPoints.ToHashSet();
        _traversedFirstPassEndPoints.Clear();

        var choke = new ChokeChainDijkstra();
        var model = _model.NewStart(new Dictionary<PositionCoord3d, PositionData3d>() {{ point.Coord, point }}, new HashSet<PositionCoord3d>());

        AddProcess(choke, model);
    }

    private float _threshold => _model.Scale * Mathf.Sqrt(2);
    
    protected override void PointEnterArchive(IIteratableProcess traversal, PositionData3d point)
    {
        // if(!_deployedSecond)
        // {
        //     base.PointEnterArchive(traversal, point);
        // }

        base.PointEnterArchive(traversal, point);

        if(_deployedSecond)
        {
            var neighbors = _untraverseFirstPassEndpoints.Where(e => PositionCoord3d.CalculateDist(e.Endpoint.Coord, point.Coord) <= _threshold);

            foreach(var neighbor in neighbors)
            {
                _untraverseFirstPassEndpoints.Remove(neighbor);
                _traversedFirstPassEndPoints.Add(neighbor);
            }
        }
    }

    protected override void AquisitionMaxExceed(IIteratableProcess traversal, Dictionary<PositionCoord3d, PositionData3d> aquisition)
    {
        if(!_deployedSecond && traversal is Dijkstra3dAnytime dij && _firstDijkstra != null && !object.ReferenceEquals(_firstDijkstra, dij))
        {
            _maxExceeds.Add(traversal, aquisition);
        }
        _firstPassEndPointsAndTraversals.Remove(traversal);
    }

    protected override void TraversalCreated(IIteratableProcess traversal)
    {
        if(_firstDijkstra == null && traversal is Dijkstra3dAnytime dij)
        {
            _firstDijkstra = dij;
        }

        base.TraversalCreated(traversal);
    }

    protected override void PointLastOfAquisition(IIteratableProcess traversal, PositionData3d point)
    {
        if(_deployedSecond)
        {
            _nextPassEndPoints.Add(point);
        }
        //base.PointLastOfAquisition(traversal, point);
    }

    protected override void EndPointSnapshot(IIteratableProcess traversal, PositionData3d point, AquisitionEmaSnapShot snapshot)
    {
        if(!_deployedSecond)
        {
            if(!_firstPassEndPointsAndTraversals.ContainsKey(traversal))
            {
                _firstPassEndPointsAndTraversals.Add(traversal, new HashSet<EndPointSnapShot>());
            }

            _firstPassEndPointsAndTraversals[traversal].Add(new EndPointSnapShot()
            {
                Endpoint = point,
                Snapshot = snapshot,
            });
        }

        base.EndPointSnapshot(traversal, point, snapshot);
    }


    protected override void TraversalDestroy(IIteratableProcess traversal)
    {
        if(traversal is ChokeChainDijkstra)
        {
            if(!_deployedSecond)
            {
                Debug.WriteLine($"first pass endpoints - {_firstPassEndPointsAndTraversals.Count}");
            }

            if(!_deployedSecond && _firstPassEndPointsAndTraversals.Count != 0)
            {
                _firstPassEndPoints = _firstPassEndPointsAndTraversals.Values.Aggregate(new HashSet<EndPointSnapShot>(), (acc, items) => 
                {
                    foreach(var item in items)
                    {
                        acc.Add(item);
                    }

                    return acc;
                });
                Debug.WriteLine("performing second pass");
                _deployedSecond = true;
                PerformNextPass(_firstPassEndPoints.MostOrDefault((f,s) => f.Endpoint.DistanceFromStart > s.Endpoint.DistanceFromStart).Endpoint);
            }
            else if(_deployedSecond)
            {
                foreach(var nextEndpoint in _nextPassEndPoints)
                {
                    var neighbors = _traversedFirstPassEndPoints.Where(e => PositionCoord3d.CalculateDist(e.Endpoint.Coord, nextEndpoint.Coord) <= _threshold).ToArray();

                    if(neighbors.Count() != 0)
                    {
                        _finalEndPoints.Add(neighbors.MostOrDefault((f,s) => f.Endpoint.DistanceFromStart > s.Endpoint.DistanceFromStart));
                    }
                }

                foreach(var traversed in _traversedFirstPassEndPoints)
                {
                    _firstPassEndPoints.Remove(traversed);
                }

                _nextPassEndPoints.Clear();

                if(_firstPassEndPoints.Count != 0)
                {
                    PerformNextPass(_firstPassEndPoints.MostOrDefault((f,s) => f.Endpoint.DistanceFromStart > s.Endpoint.DistanceFromStart).Endpoint);
                }
                else 
                {
                    EmitCavityResults();
                }
            }
            else
            {
                EmitCavityResults();
            }
        } 

        base.TraversalDestroy(traversal);  
    }

    private void EmitCavityResults()
    {
        Debug.WriteLine($"EMITTED - {_finalEndPoints.Count}, {_maxExceeds.Count}");
        foreach(var endpoint in _finalEndPoints)
        {
            base.PointLastOfAquisition(this, endpoint.Endpoint);
        }

        foreach(var exceed in _maxExceeds)
        {
            base.AquisitionMaxExceed(exceed.Key, exceed.Value);
        }

        var exceedAgg = _maxExceeds.Values.ToHashSet();

        CavityResult(this, _finalEndPoints, exceedAgg);
    }
}