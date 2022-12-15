using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectingScheduler
{

    #region Variables

    private Queue<BaseDirecting> _directings = new Queue<BaseDirecting>();
    private BaseDirecting _curDirecting = null;

    #endregion

    public void Update()
    {
        if (_directings.Count == 0)
            return;

        if (_curDirecting == null || _curDirecting.IsComplete)
        {
            _curDirecting = _directings.Dequeue();
            _curDirecting.Execute();
        }
    }

    public bool IsDirecting()
    {
        if (_directings.Count == 0)
            return false;

        if (_curDirecting == null || _curDirecting.IsComplete)
        {
            return false;
        }
        return true;
    }

    public bool IsEmpty()
    {
        return _directings.Count == 0;
    }

    public void Clear()
    {
        _directings.Clear();
        _curDirecting = null;
    }

    public void Enqueue(BaseDirecting directing, bool Independant = false)
    {
        if (Independant)
            _directings.Enqueue(new Directing_CallBack(null, directing.Execute));
        else
            _directings.Enqueue(directing);
    }
}
