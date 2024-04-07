using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class ChokeChainDijkstra : IslandDijkstra
{
    private int _iterationCount = 5;
    private IIteratableProcess _firstTraversal = null;
    private bool _firstTraversalGrowing = true;
    private Dictionary<IIteratableProcess, AquisitionEmaIterationModel> _movingAverages = new Dictionary<IIteratableProcess, AquisitionEmaIterationModel>();

    protected override void TraversalProcessing()
    {
        base.TraversalProcessing();
        
        foreach(var traversal in _traversalAquisitions)
        {
            var moving = _movingAverages[traversal.Key];
            
            if(moving.Iterate())
            {
                moving.Update(CurvatureEstimator(traversal.Value.Keys), traversal.Value.Count);

                var value = moving.CuravatureEma.CurrentSecondDerivative;

                if(!object.ReferenceEquals(traversal.Key, _firstTraversal) && value > _model.CurvatureCutoff)
                {
                    Debug.WriteLine("Aquisition Died");
                    AquisitionMaxExceed(traversal.Key, traversal.Value);
                    RemoveProcess(traversal.Key);
                }
                else
                {
                    moving.Reset(ScaleEpochs(traversal.Value.Count));
                }
            }
        }
    }

    protected override void TraversalCreated(IIteratableProcess traversal)
    {
        base.TraversalCreated(traversal);

        if(traversal is Dijkstra3dAnytime)
        {
            if(_firstTraversal == null)
            {
                _firstTraversal = traversal;
            }
            _movingAverages.Add(traversal, new AquisitionEmaIterationModel(new DerivativeEma(_model.CurveEmaAlpha, 0), new DerivativeEma(_model.PopEmaAlpha, 0), new BufferDerivativeEma(_model.PopEmaAlpha, 0, _model.PopBufferLag), _iterationCount));
        }
    }

    protected override void PointEnterAquisition(IIteratableProcess traversal, PositionData3d point)
    {
        if(object.ReferenceEquals(traversal, _firstTraversal) && _traversalAquisitions[traversal].Count >= _model.InitialSeedCutoff)
        {
            if(_movingAverages[traversal].CuravatureEma.CurrentFirstDerivative < 0)
            {
                _firstTraversalGrowing = false;
            }

            if(_firstTraversalGrowing && _traversalAquisitions[traversal].Count >= _model.InitialSeedCutoff)
            {
                Debug.WriteLine("Seed Aquisition Died");
                AquisitionMaxExceed(traversal, _traversalAquisitions[traversal]);
                RemoveProcess(traversal);   
            }
        }
        else
        {
            base.PointEnterAquisition(traversal, point);
        }
    }

    protected override void TraversalSplit(IIteratableProcess oldTraversal, Dictionary<IIteratableProcess, Dictionary<PositionCoord3d, PositionData3d>> traversalsAndAquisition)
    {
        base.TraversalSplit(oldTraversal, traversalsAndAquisition);
        foreach(var traversal in traversalsAndAquisition)
        {
            var ema = _movingAverages[oldTraversal].Clone();
            ema.Update(CurvatureEstimator(traversal.Value.Keys), traversal.Value.Count);
            ema.Reset(ScaleEpochs(traversal.Value.Count));
            _movingAverages[traversal.Key] = ema;
        }
    }

    protected override void TraversalsMerge(HashSet<IIteratableProcess> oldTraversals, IIteratableProcess newTraversal, Dictionary<PositionCoord3d, PositionData3d> aquisition)
    {
        base.TraversalsMerge(oldTraversals, newTraversal, aquisition);
        
        var curveDiv1 = oldTraversals.Aggregate(0f, (s, i) => s + _movingAverages[i].CuravatureEma.CurrentFirstDerivative);
        var curveDiv2 = oldTraversals.Aggregate(0f, (s, i) => s + _movingAverages[i].CuravatureEma.CurrentSecondDerivative);
        var popDiv1 = oldTraversals.Aggregate(0f, (s, i) => s + _movingAverages[i].PopulationEma.CurrentFirstDerivative);
        var popDiv2 = oldTraversals.Aggregate(0f, (s, i) => s + _movingAverages[i].PopulationEma.CurrentSecondDerivative);
        var total = oldTraversals.Count;

        curveDiv1 /= total;
        curveDiv2 /= total;
        popDiv1 /= total;
        popDiv2 /= total;

        _movingAverages[newTraversal] = new AquisitionEmaIterationModel(new DerivativeEma(_model.CurveEmaAlpha, CurvatureEstimator(aquisition.Keys), curveDiv1, curveDiv2), new DerivativeEma(_model.PopEmaAlpha, aquisition.Count, popDiv1, popDiv2), new BufferDerivativeEma(_model.PopEmaAlpha, aquisition.Count, popDiv1, popDiv2, _model.PopBufferLag), ScaleEpochs(aquisition.Count));
    }

    protected override void PointLastOfAquisition(IIteratableProcess traversal, PositionData3d point)
    {
        EndPointSnapshot(traversal, point, _movingAverages[traversal].SnapShot());
        base.PointLastOfAquisition(traversal, point);
    }

    private int ScaleEpochs(int epochs)
    {
        var scaledEpochs = Mathf.RoundToInt(epochs * _model.EpochMultiplier);

        return scaledEpochs < 1 ? 1 : scaledEpochs;
    }

    private float CurvatureEstimator(IEnumerable<PositionCoord3d> points)
    {
        var centroid = PositionCoord3dFloat.ComputeCentroid(points);
        var avg = PositionCoord3dFloat.AvgDistanceFromCentroid(points);
        var min = PositionCoord3dFloat.MinDistanceFromCentroid(points);

        return avg * min;
    }
}
