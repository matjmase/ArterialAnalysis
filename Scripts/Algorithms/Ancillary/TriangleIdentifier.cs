using System;
using System.Collections.Generic;
using System.Linq;
using static Dijkstra3dAnytime;

public class TriangleIdentifier
{
    private Dictionary<PositionCoord3d, PositionData3d> _totalAquisition;
    
    private Dictionary<PositionCoord3d, PositionData3d> _oldFocusPoints = new Dictionary<PositionCoord3d, PositionData3d>();

    private IsValidNeighborCheckFunc _originalValidNeighbor;
    private int _scale;
    private Dictionary<PositionCoord3d, PositionData3d> _currentGroup;
    private KeyValuePair<PositionCoord3d, PositionData3d> _start;

    // dependency injected config

    protected readonly PositionCoord3d[] _cubeNeighbors = NeighborConfigurations.MainConfig;

    private PositionCoord3d[] _scaledNeighbors;

    protected PositionCoord3d[] _neighbors => _scaledNeighbors;
    protected HashSet<PositionCoord3d> _neighborHash;


    public TriangleIdentifier(Dictionary<PositionCoord3d, PositionData3d> aquisition, IsValidNeighborCheckFunc validNeighbor, int scale = 1)
    {
        if(aquisition.Count == 0)
        {
            throw new ArgumentException("Aquisition has count of zero.");
        }

        _originalValidNeighbor = validNeighbor;
        _scale = scale;

        _totalAquisition = new Dictionary<PositionCoord3d, PositionData3d>();
        foreach(var kv in aquisition)
        {
            _totalAquisition.Add(kv.Key, kv.Value);
        }

        // scale neighbors
        var scaler = new PositionCoord3d(_scale, _scale, _scale);
        _scaledNeighbors = _cubeNeighbors.Select(e => e * scaler).ToArray();
        _neighborHash = _neighbors.ToHashSet();
    }

    public HashSet<Tuple<PositionData3d, PositionData3d, PositionData3d>> CalculateTriangles()
    {
        var outputTriangles = new HashSet<Tuple<PositionData3d, PositionData3d, PositionData3d>>();

        while (_totalAquisition.Count != 0)
        {
            _currentGroup = new Dictionary<PositionCoord3d, PositionData3d>();
            _start = _totalAquisition.First();

            do
            {
                var traverseAlg = new Dijkstra3dAnytime();
                traverseAlg.OnPointEnterAquisition += PointEnterAquisition;
                
                var customModel = new DijkstraProcessModel()
                {
                    AquisitionPoints = new Dictionary<PositionCoord3d, PositionData3d>(){{ _start.Key, _start.Value }},
                    ArchivedPoints = new HashSet<PositionCoord3d>(),
                    IsValidNeighbor = IsValidNeighborLoop,
                    Scale = _scale,
                };

                traverseAlg.Initialize(customModel);

                while (!traverseAlg.IsComplete || _currentGroup.Count != 3)
                {
                    traverseAlg.Iterate();
                }

                traverseAlg.OnPointEnterAquisition -= PointEnterAquisition;
            }
            while(_currentGroup.Count == 3);
            
            _totalAquisition.Remove(_start.Key);
        }

        return outputTriangles;
    }

    private void PointEnterAquisition(IIteratableProcess traversal, PositionData3d point)
    {
        _currentGroup.Add(point.Coord, point);
    }

    private bool IsValidNeighborLoop(PositionCoord3d from, PositionCoord3d to)
    {
        // Close the loop
        if(_currentGroup.Count == 2)
        {   
            var diff = to - _start.Key;
            return _neighborHash.Contains(diff) && IsValidNeighbor(from, to) && IsValidNeighbor(to, _start.Key);
        }
        else 
        {
            return IsValidNeighbor(from, to);
        }
    }

    private bool IsValidNeighbor(PositionCoord3d from, PositionCoord3d to)
    {
        return _totalAquisition.ContainsKey(to) && _originalValidNeighbor(from, to);
    }
}