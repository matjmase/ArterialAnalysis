using System.Collections;
using System.Collections.Generic;

public class BufferDerivativeEma : DerivativeEma
{
    protected Queue<float> _buffer = new Queue<float>();
    protected int _bufferAmt;

    public int BufferAmt => _bufferAmt;

    public BufferDerivativeEma(float alpha, float firstValue, int bufferAmt) : base(alpha, firstValue)
    {
        _bufferAmt = bufferAmt;
    }
    
    public BufferDerivativeEma(float alpha, float firstValue, float div1, float div2, int bufferAmt) : this(alpha, firstValue, bufferAmt)
    {
        _firstDerivitive = div1;
        _secondDerivitive = div2;
    }

    public override void Update(float value)
    {
        _buffer.Enqueue(value);
        if(_buffer.Count > _bufferAmt)
        {
            base.Update(_buffer.Dequeue());
        }
    }
}