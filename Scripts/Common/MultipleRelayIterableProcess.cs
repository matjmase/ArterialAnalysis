using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public abstract class MultipleRelayIterableProcess : RelayIterableProcess
{
    
    // process state
    private HashSet<IIteratableProcess> _processes = new HashSet<IIteratableProcess>();

    // internal state
    private bool _initialized;
    protected DijkstraProcessModel _model;

    // public properties
    public override bool IsComplete => _processes.Count == 0;

    public override bool IsInitialized => _initialized;

    // methods

    public override void Initialize(DijkstraProcessModel model)
    {
        _model = model;
        TraversalCreated(this);
        _initialized = true;
        PostInitialize();
    }

    protected abstract void PostInitialize();

    protected override void SafeIterate()
    {
        foreach(var process in _processes.ToArray())
        {
            process.Iterate();
            if(process.IsComplete)
            {
                RemoveProcess(process);
            }
        }

        if(IsComplete)
        {
            Dispose();
        }
    }

    // These will be used for adding/removing child processes
    protected void RemoveProcess(IIteratableProcess process)
    {
        if(!_initialized)
        {
            return;
        }

        if(_processes.Remove(process))
        {
            process.Dispose();
            Unsubscribe(process);
        }
    }

    protected void AddProcess(IIteratableProcess process, DijkstraProcessModel model)
    {
        if(!_initialized)
        {
            return;
        }

        Subscribe(process);
        process.Initialize(model);
        _processes.Add(process);
    }

    // emit a traversal destroy (if I am ending my own process, the parent needs to know)
    public override void Dispose()
    {
        var initialValue = _initialized;
        _initialized = false;

        foreach(var process in _processes)
        {
            process.Dispose();
            Unsubscribe(process);
        }

        _processes.Clear();

        if(initialValue)
        {
            TraversalDestroy(this);
        }
    }

    // Remove - Child is telling us it is destoyed. (we may already have removed it at our parent level)
    protected override void TraversalDestroy(IIteratableProcess traversal)
    {
        RemoveProcess(traversal);
        base.TraversalDestroy(traversal);
    }
}