using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public abstract class RelayIterableProcess : IIteratableProcess
{
    // public properties
    public abstract bool IsInitialized { get; }
    public abstract bool IsComplete { get; }

    // events
    public event IIteratableProcess.TraversalChangedState OnTraversalCreated;
    public event IIteratableProcess.TraversalChangedState OnTraversalDestroy;
    public event IIteratableProcess.PointChangedState OnPointEnterAquisition;
    public event IIteratableProcess.PointChangedState OnPointEnterArchive;
    public event IIteratableProcess.PointChangedState OnPointLastOfAquisition;
    public event IIteratableProcess.TraversalsSplitState OnTraversalSplit;
    public event IIteratableProcess.TraversalsMergeState OnTraversalsMerge;
    public event IIteratableProcess.PointChangedStateSnapshot OnEndPointSnapshot;
    public event IIteratableProcess.TraversalAquisitionState OnAquisitionMaxExceed;
    public event IIteratableProcess.CavityResultState OnCavityResult;
    public event IIteratableProcess.ArterialEventState OnBlockage;
    public event IIteratableProcess.ArterialEventState OnHemorrhage;

    // lifecycle
    public abstract void Dispose();

    public abstract void Initialize(DijkstraProcessModel model);

    // processing
    public void Iterate()
    {
        if (!IsInitialized)
        {
            throw new Exception("Not Initialized");
        }
        if (IsComplete)
        {
            throw new Exception("Traversal is complete.");
        }

        SafeIterate();
    }

    protected abstract void SafeIterate();

    // hooking into events
    protected void Subscribe(IIteratableProcess process)
    {
        process.OnPointEnterAquisition += PointEnterAquisition;
        process.OnPointEnterArchive += PointEnterArchive;
        process.OnPointLastOfAquisition += PointLastOfAquisition;
        process.OnTraversalCreated += TraversalCreated;
        process.OnTraversalDestroy += TraversalDestroy;

        process.OnTraversalSplit += TraversalSplit;
        process.OnTraversalsMerge += TraversalsMerge;

        process.OnEndPointSnapshot += EndPointSnapshot;
        process.OnAquisitionMaxExceed += AquisitionMaxExceed;

        process.OnCavityResult += CavityResult;

        process.OnBlockage += Blockage;
        process.OnHemorrhage += Hemorrhage;
    }

    protected void Unsubscribe(IIteratableProcess process)
    {
        process.OnPointEnterAquisition -= PointEnterAquisition;
        process.OnPointEnterArchive -= PointEnterArchive;
        process.OnPointLastOfAquisition -= PointLastOfAquisition;
        process.OnTraversalCreated -= TraversalCreated;
        process.OnTraversalDestroy -= TraversalDestroy;
        
        process.OnTraversalSplit -= TraversalSplit;
        process.OnTraversalsMerge -= TraversalsMerge;
        
        process.OnEndPointSnapshot -= EndPointSnapshot;
        process.OnAquisitionMaxExceed -= AquisitionMaxExceed;
        
        process.OnCavityResult -= CavityResult;
        
        process.OnBlockage -= Blockage;
        process.OnHemorrhage -= Hemorrhage;
    }

    

    // filtering layer/relay layer
    protected virtual void PointEnterAquisition(IIteratableProcess traversal, PositionData3d point)
    {
        OnPointEnterAquisition?.Invoke(traversal, point);
    }

    protected virtual void PointEnterArchive(IIteratableProcess traversal, PositionData3d point)
    {
        OnPointEnterArchive?.Invoke(traversal, point);
    }

    protected virtual void PointLastOfAquisition(IIteratableProcess traversal, PositionData3d point)
    {
        OnPointLastOfAquisition?.Invoke(traversal, point);
    }

    protected virtual void TraversalCreated(IIteratableProcess traversal)
    {
        OnTraversalCreated?.Invoke(traversal);
    }

    protected virtual void TraversalDestroy(IIteratableProcess traversal)
    {
        OnTraversalDestroy?.Invoke(traversal);
    }

    protected virtual void TraversalSplit(IIteratableProcess oldTraversal, Dictionary<IIteratableProcess, Dictionary<PositionCoord3d, PositionData3d>> traversalsAndAquisition)
    {
        OnTraversalSplit?.Invoke(oldTraversal, traversalsAndAquisition);
    }

    protected virtual void TraversalsMerge(HashSet<IIteratableProcess> oldTraversals, IIteratableProcess newTraversal, Dictionary<PositionCoord3d, PositionData3d> aquisition)
    {
        OnTraversalsMerge?.Invoke(oldTraversals, newTraversal, aquisition);
    }
    
    protected virtual void EndPointSnapshot(IIteratableProcess traversal, PositionData3d point, AquisitionEmaSnapShot snapshot)
    {
        OnEndPointSnapshot?.Invoke(traversal, point, snapshot);
    }

    protected virtual void AquisitionMaxExceed(IIteratableProcess traversal, Dictionary<PositionCoord3d, PositionData3d> aquisition)
    {
        OnAquisitionMaxExceed?.Invoke(traversal, aquisition);
    }

    protected virtual void CavityResult(IIteratableProcess traversal, IEnumerable<EndPointSnapShot> endPoints, IEnumerable<Dictionary<PositionCoord3d, PositionData3d>> aquisitionExceed)
    {
        OnCavityResult?.Invoke(traversal, endPoints, aquisitionExceed);
    }

    protected virtual void Blockage(IIteratableProcess traversal, PositionCoord3dFloat location)
    {
        OnBlockage?.Invoke(traversal, location);
    }
    
    protected virtual void Hemorrhage(IIteratableProcess traversal, PositionCoord3dFloat location)
    {
        OnHemorrhage?.Invoke(traversal, location);
    }
}