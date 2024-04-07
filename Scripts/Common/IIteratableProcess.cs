using System;
using System.Collections.Generic;

public interface IIteratableProcess : IDisposable
{
    // Notification delegates
    public delegate void TraversalChangedState(IIteratableProcess traversal);
    public delegate void TraversalsCentroidState(HashSet<IIteratableProcess> traversals, PositionCoord3dFloat centroid);
    public delegate void TraversalAquisitionState(IIteratableProcess traversal, Dictionary<PositionCoord3d, PositionData3d> aquisition);
    public delegate void TraversalsMergeState(HashSet<IIteratableProcess> oldTraversals, IIteratableProcess newTraversal, Dictionary<PositionCoord3d, PositionData3d> aquisition);
    public delegate void TraversalsSplitState(IIteratableProcess oldTraversal, Dictionary<IIteratableProcess, Dictionary<PositionCoord3d, PositionData3d>> traversalsAndAquisition);
    public delegate void PointChangedState(IIteratableProcess traversal, PositionData3d point);
    public delegate void PointChangedStateSnapshot(IIteratableProcess traversal, PositionData3d point, AquisitionEmaSnapShot snapshot);
    public delegate void CavityResultState(IIteratableProcess traversal, IEnumerable<EndPointSnapShot> endPoints, IEnumerable<Dictionary<PositionCoord3d, PositionData3d>> aquisitionExceed);
    public delegate void ArterialEventState(IIteratableProcess traversal, PositionCoord3dFloat location);

    // Dijkstra lifecycle Events
    public event TraversalChangedState OnTraversalCreated;
    public event TraversalChangedState OnTraversalDestroy;
    public event PointChangedState OnPointEnterAquisition;
    public event PointChangedState OnPointEnterArchive;
    public event PointChangedState OnPointLastOfAquisition;

    // Island lifecycle Events
    public event TraversalsSplitState OnTraversalSplit;
    public event TraversalsMergeState OnTraversalsMerge;

    // Choke chain
    public event PointChangedStateSnapshot OnEndPointSnapshot;
    public event TraversalAquisitionState OnAquisitionMaxExceed;

    // MultiPass
    public event CavityResultState OnCavityResult;

    // Arterial
    public event ArterialEventState OnBlockage;
    public event ArterialEventState OnHemorrhage;

    // Properties
    public bool IsComplete { get; }
    public bool IsInitialized { get; }

    // DI the model and emit any initial events
    public void Initialize(DijkstraProcessModel model);

    // Anytime paradigm
    public void Iterate();
}