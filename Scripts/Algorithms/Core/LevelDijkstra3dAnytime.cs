using System;
using System.Collections.Generic;
using System.Linq;
using static Dijkstra3dAnytime;

public class LevelDijkstra3dAnytime : IIteratableProcess
{

    // dependency injected config

    protected readonly PositionCoord3d[] _cubeNeighbors = NeighborConfigurations.MainConfig;

    private PositionCoord3d[] _scaledNeighbors;

    protected PositionCoord3d[] _neighbors => _scaledNeighbors;

    // state
    private bool _initialized;
    private IsValidNeighborCheckFunc _isValidNeighbor;
    // heap is needed if you have neighbors that are equally distant from origin. Otherwise just use a queue.
    private Heap<PositionData3d> _aquisitionPoints = new Heap<PositionData3d>((first, second) => first.DistanceFromStart < second.DistanceFromStart);
    private Dictionary<PositionCoord3d, PositionData3d> _aquisitionPointsHash = new Dictionary<PositionCoord3d, PositionData3d>();
    private HashSet<PositionCoord3d> _archivedPoints = new HashSet<PositionCoord3d>();


    // lifecycle Events
    public event IIteratableProcess.TraversalChangedState OnTraversalCreated;
    public event IIteratableProcess.TraversalChangedState OnTraversalDestroy;
    public event IIteratableProcess.PointChangedState OnPointEnterAquisition;
    public event IIteratableProcess.PointChangedState OnPointEnterArchive;
    public event IIteratableProcess.PointChangedState OnPointLastOfAquisition;
    public event IIteratableProcess.TraversalsMergeState OnTraversalsMerge;
    public event IIteratableProcess.TraversalsSplitState OnTraversalSplit;
    public event IIteratableProcess.PointChangedStateSnapshot OnEndPointSnapshot;
    public event IIteratableProcess.TraversalAquisitionState OnAquisitionMaxExceed;
    public event IIteratableProcess.CavityResultState OnCavityResult;
    public event IIteratableProcess.ArterialEventState OnBlockage;
    public event IIteratableProcess.ArterialEventState OnHemorrhage;

    // public properties
    public bool IsComplete => _aquisitionPoints.Count == 0;
    public bool IsInitialized => _initialized;

    public PositionCoord3d[] NeighborConfiguration => _neighbors;

    // constructors

    public LevelDijkstra3dAnytime()
    {
    }

    public void Initialize(DijkstraProcessModel model)
    {
        _isValidNeighbor = model.IsValidNeighbor;

        // clone them all

        // archive
        foreach(var archive in model.ArchivedPoints)
        {
            _archivedPoints.Add(archive);
        }

        // aquisition
        foreach(var point in model.AquisitionPoints)
        {
            if(_archivedPoints.Contains(point.Key))
            {
                throw new ArgumentException("An aquisition point is contained by the archive points");
            }
            _aquisitionPoints.Add(point.Value);
            _aquisitionPointsHash.Add(point.Key, point.Value);
        }

        // scale neighbors
        var scaler = new PositionCoord3d(model.Scale, model.Scale, model.Scale);
        _scaledNeighbors = _cubeNeighbors.Select(e => e * scaler).ToArray();

        // Check neighbor pattern is not redundant - Dummy proofing for experimenting later.
        var redundancy = new HashSet<PositionCoord3d>();
        foreach(var neighbor in _neighbors)
        {
            if(redundancy.Contains(neighbor))
            {
                throw new ArgumentException("Neighbor config contains duplicates");
            }
            redundancy.Add(neighbor);
        }

        // emit events
        OnTraversalCreated?.Invoke(this);

        foreach(var point in _aquisitionPointsHash)
        {
            OnPointEnterAquisition?.Invoke(this, point.Value);
        }

        _initialized = true;
    }

    public void Iterate()
    {
        if (IsComplete)
        {
            throw new Exception("No more aquisition neighbors to traverse.");
        }

        var focus = _aquisitionPoints.Pop();

        // the point should not have entered the aquisition if it was already archived. 
        if(_archivedPoints.Contains(focus.Coord))
        {
            throw new Exception("Internal Exception");
        }


        var newlyAquiredPoints = new HashSet<PositionData3d>();
        for (var i = 0; i < _neighbors.Length; i++)
        {
            var newCoord = focus.Coord + _neighbors[i];

            if (_archivedPoints.Contains(newCoord) || _aquisitionPointsHash.ContainsKey(newCoord) || !_isValidNeighbor(focus.Coord, newCoord))
                continue;

            var newNeighbor = new PositionData3d(newCoord, focus.DistanceFromStart + PositionCoord3d.CalculateDist(focus.Coord, newCoord));

            // Aquisition
            _aquisitionPoints.Add(newNeighbor);
            _aquisitionPointsHash.Add(newNeighbor.Coord, newNeighbor);
            OnPointEnterAquisition?.Invoke(this, newNeighbor);

            newlyAquiredPoints.Add(newNeighbor);
        }

        // Archive
        _aquisitionPointsHash.Remove(focus.Coord);
        _archivedPoints.Add(focus.Coord);
        OnPointEnterArchive?.Invoke(this, focus);

        if (_aquisitionPoints.Count == 0)
        {
            OnPointLastOfAquisition?.Invoke(this, focus);
            Dispose();
        }
    }

    public void Dispose()
    {
        if(_initialized)
        {
            OnTraversalDestroy?.Invoke(this);
        }
        _initialized = false;
    }
}