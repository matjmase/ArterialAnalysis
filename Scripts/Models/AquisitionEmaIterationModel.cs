using System;

public class AquisitionEmaIterationModel
{
    private DerivativeEma _curveEma;
    private DerivativeEma _popEma;
    private BufferDerivativeEma _laggedPopEma;
    private int _numberOfIterations;

    public DerivativeEma CuravatureEma => _curveEma;
    public DerivativeEma PopulationEma => _popEma;
    public BufferDerivativeEma LaggedPopEma => _laggedPopEma;

    private AquisitionEmaIterationModel()
    {}

    public AquisitionEmaIterationModel(DerivativeEma curve, DerivativeEma population, BufferDerivativeEma laggedPopEma, int numberOfIterations)
    {
        if(numberOfIterations == 0)
        {
            throw new ArgumentException("number of iterations cannot be set to zero");
        }

        _curveEma = curve;
        _popEma = population;
        _laggedPopEma = laggedPopEma;
        _numberOfIterations = numberOfIterations;
    }

    public void Update(float curve, float pop)
    {
        _curveEma.Update(curve);
        _popEma.Update(pop);
        _laggedPopEma.Update(pop);
    }

    public bool Iterate()
    {
        _numberOfIterations--;
        
        if(_numberOfIterations < 0)
        {
            throw new System.Exception("Reset the iterations");
        }

        return _numberOfIterations == 0;
    }

    public void Reset(int numberOfIterations)
    {
        if(numberOfIterations == 0)
        {
            throw new ArgumentException("number of iterations cannot be set to zero");
        }

        _numberOfIterations = numberOfIterations;
    }

    public AquisitionEmaIterationModel CloneDiffCurve(float curveValue, float curveDiv1, float curveDiv2)
    {
        return new AquisitionEmaIterationModel()
        {
            _numberOfIterations = _numberOfIterations,
            _curveEma = new DerivativeEma(_curveEma.Alpha, curveValue, curveDiv1, curveDiv2),
            _popEma = _popEma,
            _laggedPopEma = _laggedPopEma,
        };
    }

    public AquisitionEmaIterationModel Clone()
    {
        return new AquisitionEmaIterationModel()
        {
            _numberOfIterations = _numberOfIterations,
            _curveEma = new DerivativeEma(_curveEma.Alpha, _curveEma.CurrentValue, _curveEma.CurrentFirstDerivative, _curveEma.CurrentSecondDerivative),
            _popEma = new DerivativeEma(_popEma.Alpha, _popEma.CurrentValue, _popEma.CurrentFirstDerivative, _popEma.CurrentSecondDerivative),
            _laggedPopEma = new BufferDerivativeEma(_laggedPopEma.Alpha, _laggedPopEma.CurrentValue, _laggedPopEma.CurrentFirstDerivative, _laggedPopEma.CurrentSecondDerivative, _laggedPopEma.BufferAmt)
        };
    }

    public AquisitionEmaIterationModel CloneDiffCurve(float curveValue)
    {
        return CloneDiffCurve(curveValue, _curveEma.CurrentFirstDerivative, _curveEma.CurrentSecondDerivative);
    }

    public AquisitionEmaIterationModel CloneDiffPop(float popValue, float popDiv1, float popDiv2)
    {
        return new AquisitionEmaIterationModel()
        {
            _numberOfIterations = _numberOfIterations,
            _popEma = new DerivativeEma(_popEma.Alpha, popValue, popDiv1, popDiv2),
            _laggedPopEma = new BufferDerivativeEma(_laggedPopEma.Alpha, popValue, popDiv1, popDiv2, _laggedPopEma.BufferAmt),
            _curveEma = _curveEma,
        };
    }

    public AquisitionEmaIterationModel CloneDiffPop(float popValue)
    {
        return CloneDiffCurve(popValue, _popEma.CurrentFirstDerivative, _popEma.CurrentSecondDerivative);
    }

    public AquisitionEmaSnapShot SnapShot()
    {
        return new AquisitionEmaSnapShot()
        {
            Curvature = _curveEma.SnapShot(),
            Population = _popEma.SnapShot(),
            BufferPop = _laggedPopEma.SnapShot(),
        };
    }
}