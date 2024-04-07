using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class ArterialDijkstra : MultipleRelayIterableProcess
{
    private HashSet<IIteratableProcess> _traversing = new HashSet<IIteratableProcess>();

    private HashSet<PositionCoord3d> _archivedPoints = new HashSet<PositionCoord3d>();
    private Dictionary<BridgeDijkstra, HashSet<PositionCoord3d>> _bridgeArchives = new Dictionary<BridgeDijkstra, HashSet<PositionCoord3d>>();
    private HashSet<EndPointSnapShot> _newEndpoints = new HashSet<EndPointSnapShot>();

    protected override void PostInitialize()
    {
        foreach(var arch in _model.ArchivedPoints)
        {
            _archivedPoints.Add(arch);
        }

        var multiPassDijkstra = new MultiPassDijkstra();
        var model = _model;

        AddProcess(multiPassDijkstra, model);
        _traversing.Add(multiPassDijkstra);
    }

    protected override void TraversalCreated(IIteratableProcess traversal)
    {
        base.TraversalCreated(traversal);
        if(traversal is BridgeDijkstra bridge)
        {
            _traversing.Add(bridge);
            _bridgeArchives.Add(bridge, new HashSet<PositionCoord3d>());
        }
    }

    protected override void CavityResult(IIteratableProcess traversal, IEnumerable<EndPointSnapShot> endPoints, IEnumerable<Dictionary<PositionCoord3d, PositionData3d>> aquisitionExceed)
    {
        base.CavityResult(traversal, endPoints, aquisitionExceed);

        if(ScrutinizeCavity(endPoints, aquisitionExceed))
        {
            var endpointHash = endPoints.ToHashSet();

            var remove = EndPointCluster(_newEndpoints, endpointHash, e => e.Endpoint.Coord.ToFloat());

            foreach(var item in remove)
            {
                _newEndpoints.Remove(item.Item1);
                endpointHash.Remove(item.Item2);
            }

            foreach(var endpoint in endpointHash)
            {
                _newEndpoints.Add(endpoint);
            }

            foreach(var exceed in aquisitionExceed)
            {
                Hemorrhage(this, PositionCoord3dFloat.ComputeCentroid(exceed.Keys));
            }
        }
    }

    private float DistFromCentroid(PositionCoord3dFloat point, IEnumerable<PositionCoord3d> points) => PositionCoord3dFloat.CalculateDist(point, PositionCoord3dFloat.ComputeCentroid(points));

    private bool ScrutinizeCavity(IEnumerable<EndPointSnapShot> endPoints, IEnumerable<Dictionary<PositionCoord3d, PositionData3d>> aquisitionExceed)
    {
        if(aquisitionExceed.Count() == 1)
        {
            var fail = endPoints.Count() == 0 || endPoints.All(e => DistFromCentroid(e.Endpoint.Coord.ToFloat(), aquisitionExceed.First().Keys) > PositionCoord3dFloat.AvgDistanceFromCentroid(aquisitionExceed.First().Keys) * 1.5f);
            return !fail;
        }
        else
        {
            return true;
        }
    }

    protected override void PointEnterAquisition(IIteratableProcess traversal, PositionData3d point)
    {
        if(traversal is BridgeDijkstra bridge)
        {
            // if(_archivedPoints.Contains(point.Coord))
            // {
            //     foreach(var bridgeArch in _bridgeArchives[bridge])
            //     {
            //         _archivedPoints.Remove(bridgeArch);
            //     }

            //     _bridgeArchives.Remove(bridge);
                
            //     Debug.WriteLine("BridgeCollision");
            //     RemoveProcess(traversal);
            // }
        }
        else
        {
            base.PointEnterAquisition(traversal, point);
        }
    }

    protected override void PointEnterArchive(IIteratableProcess traversal, PositionData3d point)
    {
        if(traversal is BridgeDijkstra bridge)
        {
            if(!_bridgeArchives.ContainsKey(bridge))
            {
                _bridgeArchives.Add(bridge, new HashSet<PositionCoord3d>());
            }
            _bridgeArchives[bridge].Add(point.Coord);
        }
        else
        {
            base.PointEnterArchive(traversal, point);
        }
        
        _archivedPoints.Add(point.Coord);
    }

    protected override void TraversalDestroy(IIteratableProcess traversal)
    {
        base.TraversalDestroy(traversal);

        if(traversal is BridgeDijkstra bridge)
        {
            _bridgeArchives.Remove(bridge);
        }

        if(_traversing.Remove(traversal))
        {
            if(_traversing.Count == 0)
            {

                while(_newEndpoints.Count != 0)
                {
                    var endpoint =  _newEndpoints.First();
                    _newEndpoints.Remove(endpoint);

                    var newBridge = new BridgeDijkstra();
                    var model = _model.NewStart(endpoint.Endpoint).ChangeArchive(_archivedPoints);
                    model = model.ChangeInitialSeedCutoff(Mathf.RoundToInt(endpoint.Snapshot.BufferPop.Value * 4 * 1.0f));

                    Debug.WriteLine("New Initial Seed -" + model.InitialSeedCutoff);
                    
                    AddProcess(newBridge, model);
                }
            }
        }
    }

    private HashSet<Tuple<T,T>> EndPointCluster<T>(HashSet<T> oldEndPoints, HashSet<T> newEndPoints, Func<T, PositionCoord3dFloat> selector)
	{
        var oldClone = oldEndPoints.ToHashSet();
        var newClone = newEndPoints.ToHashSet();

        Func<T,T,float> distance = (f,s) => (selector(f) - selector(s)).Magnitude();

        var output = new HashSet<Tuple<T,T>>();

        foreach(var old in oldClone)
        {
            var matches = newClone.Where(e => distance(e, old) <= _model.BridgeRadius).ToHashSet();

            if(matches.Count() == 0)
            {
                continue;
            }

            foreach(var match in matches)
            {
                newClone.Remove(match);
            }

            var newMax = matches.MostOrDefault((f, s) => distance(old, f) > distance(old, s));

            var add = selector(old) + selector(newMax);

            var blockageLocation = new PositionCoord3dFloat(add.X / 2.0f, add.Y / 2.0f, add.Z / 2.0f);

            Blockage(this, blockageLocation);
            output.Add(new Tuple<T, T>(old, newMax));
        }

        return output;
	}
}