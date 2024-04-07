public class EmaCalculator 
{
    private float _alpha;
    private float _current;

    public float Current => _current;

    public EmaCalculator(float alpha, float firstValue)
    {
        _alpha = alpha;
        _current = firstValue;
    }

    public void Update(float value)
    {
        _current = _alpha * value + (1 - _alpha) * _current;
    }
}