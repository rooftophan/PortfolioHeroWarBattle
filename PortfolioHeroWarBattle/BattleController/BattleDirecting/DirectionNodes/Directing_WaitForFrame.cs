using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class Directing_WaitForFrame : BaseDirecting
{
    private int _waitFrame;
    private int _frame;
    private bool _useInputLock;

    public static Directing_WaitForFrame Execute(Action complete, int waitFrame, bool useInputLock = true)
    {
        var directing = new Directing_WaitForFrame(complete, waitFrame, useInputLock);
        directing.Execute();
        return directing;
    }


    public Directing_WaitForFrame(Action complete, int waitFrame, bool useInputLock=true) : base(complete)
    {
        _waitFrame = waitFrame;
        _useInputLock = useInputLock;
       
    }

    public override void Execute()
    {
        if(_useInputLock)
            GameSystem.Instance.IncreaseEventSystemLock();

        if (_waitFrame == 0)
        {
            Complete();
            return;
        }

        _frame = 0;
        var seq = DOTween.Sequence();
        seq.SetUpdate(false);

        seq.OnUpdate(() => {
            
            if (_frame >= _waitFrame && !_isComplete)
            {
                Complete();
                seq.Kill(false);
            }
            _frame++;
        });
        seq.AppendInterval(float.MaxValue);
        seq.Play();
    }


    protected override void Complete()
    {
        if(_useInputLock)
            GameSystem.Instance.DecreaseEventSystemLock();

        base.Complete();
    }
    

}
