using System;
using System.Collections.Generic;
using System.Linq;
using static Dijkstra3dAnytime;

public class IslandIdentifier
{
    private Dictionary<PositionCoord3d, PositionData3d> _totalAquisition;
    private Dictionary<PositionCoord3d, PositionData3d> _currentGroup;

    private IsValidNeighborCheckFunc _originalValidNeighbor;
    private int _scale;

    public IslandIdentifier(Dictionary<PositionCoord3d, PositionData3d> aquisition, IsValidNeighborCheckFunc validNeighbor, int scale = 1)
    {
        if(aquisition.Count == 0)
        {
            throw new ArgumentException("Aquisition has count of zero.");
        }

        _originalValidNeighbor = validNeighbor;
        _scale = scale;

        // clone the hash
        _totalAquisition = new Dictionary<PositionCoord3d, PositionData3d>();

        foreach(var point in aquisition)
        {
            _totalAquisition.Add(point.Key, point.Value);
        }
    }

    public HashSet<Dictionary<PositionCoord3d, PositionData3d>> CalculateGroups()
    {
        var outputGroups = new HashSet<Dictionary<PositionCoord3d, PositionData3d>>();

        while (_totalAquisition.Count != 0)
        {
            _currentGroup = new Dictionary<PositionCoord3d, PositionData3d>();

            var start = _totalAquisition.First();

            var traverseAlg = new Dijkstra3dAnytime();
            traverseAlg.OnPointEnterArchive += PointEnterArchive;
            
            var customModel = new DijkstraProcessModel()
            {
                AquisitionPoints = new Dictionary<PositionCoord3d, PositionData3d>(){{ start.Key, start.Value }},
                ArchivedPoints = new HashSet<PositionCoord3d>(),
                IsValidNeighbor = IsValidNeighbor,
                Scale = _scale,
            };

            traverseAlg.Initialize(customModel);

            while (!traverseAlg.IsComplete)
            {
                traverseAlg.Iterate();
            }

            traverseAlg.OnPointEnterArchive -= PointEnterArchive;

            outputGroups.Add(_currentGroup);
        }

        return outputGroups;
    }

    private void PointEnterArchive(IIteratableProcess traversal, PositionData3d point)
    {
        _currentGroup.Add(point.Coord, _totalAquisition[point.Coord]);
        _totalAquisition.Remove(point.Coord);
    }

    private bool IsValidNeighbor(PositionCoord3d from, PositionCoord3d to)
    {
        return _totalAquisition.ContainsKey(to) && _originalValidNeighbor(from, to);
    }
}