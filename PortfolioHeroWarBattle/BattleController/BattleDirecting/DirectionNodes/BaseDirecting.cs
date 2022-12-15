using System;

public class BaseDirecting
{
    protected Action _complete;
    public BaseDirecting(Action complete = null)
    {
        _complete = complete;
    }

    protected bool _isComplete;

    public bool IsComplete { get { return _isComplete; } }

    public virtual void Execute() { }

    protected virtual void Complete()
    {
        if (_complete != null)
        {
            _complete();
            _complete = null;
        }
        _isComplete = true;
        
    }

    public virtual void Update(float dt) { }
}
