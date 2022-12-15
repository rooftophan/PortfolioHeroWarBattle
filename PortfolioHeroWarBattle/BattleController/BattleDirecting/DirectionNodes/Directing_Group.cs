using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NewBattleCore;
using DG.Tweening;

public class Directing_Group : BaseDirecting
{
    private Queue<BaseDirecting> _directingList;
    private BaseDirecting _curDirecting;

    public Directing_Group(Action complete = null) : base(complete)
    {
        _directingList = new Queue<BaseDirecting>();
    }

    protected void EnqueDirecting(BaseDirecting directing, bool Independant = false)
    {
        if (Independant)
            _directingList.Enqueue(new Directing_CallBack(null, directing.Execute));
        else
            _directingList.Enqueue(directing);
    }

    public override void Execute()
    {
        if(_directingList.Count <= 0)
        {
            Complete();
            return;
        }

        _curDirecting = _directingList.Dequeue();
        _curDirecting.Execute();

        var seq = DOTween.Sequence();
        seq.SetUpdate(false);
        seq.OnUpdate(() =>
        {
            if (_curDirecting.IsComplete && _directingList.Count == 0)
            {
                seq.Kill(false);
                Complete();
                return;
            }

            if (_curDirecting.IsComplete)
            {
                _curDirecting = _directingList.Dequeue();
                _curDirecting.Execute();
            }

        });

        seq.AppendInterval(float.MaxValue);
        seq.Play();
    }
}
