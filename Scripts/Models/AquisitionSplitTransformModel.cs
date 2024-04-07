
using System.Collections.Generic;

public struct AquisitionSplitTransformModel
{
    public IIteratableProcess OldTraversal;
    public HashSet<Dictionary<PositionCoord3d, PositionData3d>> NewTraversals;

    public AquisitionSplitTransformModel(IIteratableProcess oldTraversal, HashSet<Dictionary<PositionCoord3d, PositionData3d>> newTraversals)
    {
        OldTraversal = oldTraversal;
        NewTraversals = newTraversals;
    }
}