using System.Collections;
using DG.Tweening;
using System;


public class Directing_WaitCondition : BaseDirecting
{
    private Func<bool> _condition;
    private bool _useInputLock;

    public Directing_WaitCondition(Action complete, Func<bool> condition, bool useInputLock=true) : base(complete)
    {
        _condition = condition;
        _useInputLock = useInputLock;
    }

    public override void Execute()
    {
        if(_useInputLock)
            GameSystem.Instance.IncreaseEventSystemLock();
        
        var seq = DOTween.Sequence();
        seq.SetUpdate(false);
        seq.OnUpdate(() =>
        {
            if (_condition != null)
            {
                if (_condition())
                {
                    seq.Kill(false);
                    Complete();
                    return;
                }
            }
            else
            {
                seq.Kill(false);
                Complete();
                return;
            }
         

        });

        seq.AppendInterval(float.MaxValue);
        seq.Play();
    }

    public void ForceFinish()
    {
        if(!IsComplete)
            Complete();
    }

    protected override void Complete()
    {
        if(_useInputLock)
            GameSystem.Instance.DecreaseEventSystemLock();

        base.Complete();
    }
}
