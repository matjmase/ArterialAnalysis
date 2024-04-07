public class DerivativeEma
{
    protected EmaCalculator _calculator;
    protected float _alpha;
    protected float? _firstDerivitive;
    protected float? _secondDerivitive;

    public float Alpha => _alpha;
    public float CurrentValue => _calculator.Current;
    public float CurrentFirstDerivative => _firstDerivitive == null ? 0 : _firstDerivitive.Value;
    public float CurrentSecondDerivative => _secondDerivitive == null ? 0 : _secondDerivitive.Value;

    public DerivativeEma(float alpha, float firstValue)
    {
        _alpha = alpha;
        _calculator = new EmaCalculator(alpha, firstValue);
    }

    
    public DerivativeEma(float alpha, float firstValue, float div1, float div2) : this(alpha, firstValue)
    {
        _firstDerivitive = div1;
        _secondDerivitive = div2;
    }

    public virtual void Update(float value)
    {
        var lastFirstDerivative = _firstDerivitive;
        
        var lastEma = _calculator.Current;
        _calculator.Update(value);
        var nextEma = _calculator.Current;

        _firstDerivitive = nextEma - lastEma;

        if(lastFirstDerivative != null)
        {
            _secondDerivitive = _firstDerivitive - lastFirstDerivative;
        }
    }

    public DerivativeEmaSnapshot SnapShot()
    {
        return new DerivativeEmaSnapshot()
        {
            Value = _calculator.Current,
            FirstDerivative = CurrentFirstDerivative,
            SecondDerivative = CurrentSecondDerivative,
        };
    }

    public virtual DerivativeEma Clone()
    {
        return new DerivativeEma(_alpha, CurrentValue, CurrentFirstDerivative, CurrentSecondDerivative);
    }
}