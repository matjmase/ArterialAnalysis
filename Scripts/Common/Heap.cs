using System;
using System.Collections.Generic;

public class Heap<T> : ICloneableTyped<Heap<T>>
{
    private readonly List<T> _elements;
    private readonly SequencingOrder _correctOrderCompare;

    public delegate bool SequencingOrder(T first, T second);
    public Heap(SequencingOrder comparison, int size = 0)
    {
        _correctOrderCompare = comparison;
        _elements = new List<T>(size);
    }

    public int Count => _elements.Count;    

    private int GetLeftChildIndex(int elementIndex) => 2 * elementIndex + 1;
    private int GetRightChildIndex(int elementIndex) => 2 * elementIndex + 2;
    private int GetParentIndex(int elementIndex) => (elementIndex - 1) / 2;

    private bool HasLeftChild(int elementIndex) => GetLeftChildIndex(elementIndex) < _elements.Count;
    private bool HasRightChild(int elementIndex) => GetRightChildIndex(elementIndex) < _elements.Count;
    private bool IsRoot(int elementIndex) => elementIndex == 0;

    private T GetLeftChild(int elementIndex) => _elements[GetLeftChildIndex(elementIndex)];
    private T GetRightChild(int elementIndex) => _elements[GetRightChildIndex(elementIndex)];
    private T GetParent(int elementIndex) => _elements[GetParentIndex(elementIndex)];

    private void Swap(int firstIndex, int secondIndex)
    {
        var temp = _elements[firstIndex];
        _elements[firstIndex] = _elements[secondIndex];
        _elements[secondIndex] = temp;
    }

    public bool IsEmpty()
    {
        return _elements.Count == 0;
    }

    public T Peek()
    {
        if (_elements.Count == 0)
            throw new IndexOutOfRangeException();

        return _elements[0];
    }

    public T Pop()
    {
        if (_elements.Count == 0)
            throw new IndexOutOfRangeException();

        var result = _elements[0];
        _elements[0] = _elements[_elements.Count - 1];
        _elements.RemoveAt(_elements.Count - 1);

        ReCalculateDown();

        return result;
    }

    public void Add(T element)
    {
        _elements.Add(element);

        ReCalculateUp();
    }

    private void ReCalculateDown()
    {
        int index = 0;
        while (HasLeftChild(index))
        {
            var smallerIndex = GetLeftChildIndex(index);
            if (HasRightChild(index) && _correctOrderCompare(GetRightChild(index) , GetLeftChild(index)))
            {
                smallerIndex = GetRightChildIndex(index);
            }

            if (!_correctOrderCompare(_elements[smallerIndex], _elements[index]))
            {
                break;
            }

            Swap(smallerIndex, index);
            index = smallerIndex;
        }
    }

    private void ReCalculateUp()
    {
        var index = _elements.Count - 1;
        while (!IsRoot(index) && _correctOrderCompare(_elements[index], GetParent(index)))
        {
            var parentIndex = GetParentIndex(index);
            Swap(parentIndex, index);
            index = parentIndex;
        }
    }

    public Heap<T> Clone()
    {
        var output = new Heap<T>(_correctOrderCompare, _elements.Count);

        foreach (var element in _elements)
        {
            output._elements.Add(element);
        }

        return output;
    }

    public SequencingOrder GetSequencingOrder()
    {
        return _correctOrderCompare;
    }

}
