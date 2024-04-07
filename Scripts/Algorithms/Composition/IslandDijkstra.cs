using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class IslandDijkstra : AquisitionIterableProcess
{
    protected override void PostInitialize()
    {
        var seed = new Dijkstra3dAnytime();
        AddProcess(seed, _model);
    }

    protected override void TraversalProcessing()
    {
        DetectSplit(_traversalAquisitions);
    }

    protected override void PointEnterAquisition(IIteratableProcess traversal, PositionData3d point)
    {
        // also if we have a collission with a merge, we don't want to have another traversal try to claim the point.
        if(!DetectedAquisitionMerge(traversal, point))
        {
            base.PointEnterAquisition(traversal, point);
        }
            
    }

    // Split Logic
    private void DetectSplit(Dictionary<IIteratableProcess, Dictionary<PositionCoord3d, PositionData3d>> aquisitions)
    {
        var newShellSplitTransforms = new HashSet<AquisitionSplitTransformModel>();

        foreach(var traversal in aquisitions)
        {
            var islandIdenify = new IslandIdentifier(traversal.Value, _model.IsValidNeighbor, _model.Scale);
            
            var groups = islandIdenify.CalculateGroups();
            if(groups.Count == 0)
            {
                throw new Exception("Shell return no groups.");
            }
            else if(groups.Count > 1)
            {
                Debug.WriteLine("SPLIT");
                // traversal yielded multiple islands
                newShellSplitTransforms.Add(new AquisitionSplitTransformModel(traversal.Key, groups));
            }
        }

        ProcessShellSplit(newShellSplitTransforms);
    }

    
    private void ProcessShellSplit(HashSet<AquisitionSplitTransformModel> aquisitionModels)
    {
        // apply transform
        foreach(var split in aquisitionModels)
        {
            // Remove old traversal
            RemoveProcess(split.OldTraversal);

            var totalTraversals = new Dictionary<IIteratableProcess, Dictionary<PositionCoord3d, PositionData3d>>();
            // create new traversals
            foreach(var traversal in split.NewTraversals)
            {
                var newTraversal = new Dijkstra3dAnytime();
                
                var model = _model.NewStart(traversal, _archivedPositions.Keys.ToHashSet());

                totalTraversals.Add(newTraversal, traversal);
                AddProcess(newTraversal, model);
            }
            
            // emit
            TraversalSplit(split.OldTraversal, totalTraversals);
        }
    }

    // Merge logic
    private bool DetectedAquisitionMerge(IIteratableProcess traversal, PositionData3d point)
    {
        // check if it is already in any other stages
        if(_totalAquisitionPositions.ContainsKey(point.Coord))
        {
            // we have a merge...
            Debug.WriteLine("Merge");
            HashSet<IIteratableProcess> intersect = new HashSet<IIteratableProcess>();
            foreach(var traversalStage in _traversalAquisitions)
            {
                if(_traversalAquisitions[traversalStage.Key].ContainsKey(point.Coord))
                {
                    intersect.Add(traversalStage.Key);
                }
            }

            if(intersect.Count == 0)
            {
                throw new Exception("Found duplicate stage, can't find parent traversal. Total aquisition and traversal aquisition collections out of sync.");
            }

            intersect.Add(traversal);
            ProcessAquisitionMerge(intersect);

            return true;
        }

        return false;
    }

    private void ProcessAquisitionMerge(IEnumerable<IIteratableProcess> traversals)
    {
        var mergeAquisition = new Dictionary<PositionCoord3d, PositionData3d>();

        foreach(var trav in traversals)
        {
            foreach(var item in _traversalAquisitions[trav])
            {
                mergeAquisition.Add(item.Key, item.Value);
            }
        }
    
        foreach(var other in traversals)
        {
            RemoveProcess(other);
        }

        var newTraversal = new Dijkstra3dAnytime();

        var model = _model.NewStart(mergeAquisition, _archivedPositions.Keys.ToHashSet());

        AddProcess(newTraversal, model);

        TraversalsMerge(traversals.ToHashSet(), newTraversal, mergeAquisition);
    }

}