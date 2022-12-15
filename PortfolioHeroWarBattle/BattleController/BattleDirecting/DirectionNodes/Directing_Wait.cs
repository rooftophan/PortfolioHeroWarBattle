using DG.Tweening;
using System;

public class Directing_Wait : BaseDirecting, IDisposable
{
    private readonly float _waitTime;
    private readonly bool _useInputLock;

    private Sequence _seq;
    
    public static Directing_Wait Execute(Action complete, float waitTime, bool useInputLock = true)
    {
        var directing = new Directing_Wait(complete, waitTime, useInputLock);
        directing.Execute();
        return directing;
    }

    public Directing_Wait(Action complete, float waitTime, bool useInputLock=true) : base(complete)
    {
        _waitTime = waitTime;
        _useInputLock = useInputLock;
    }

    public override void Execute()
    {
        if(_useInputLock)
            GameSystem.Instance.IncreaseEventSystemLock();
        
        _seq = DOTween.Sequence();
        _seq.SetUpdate(false);
       
        _seq.AppendInterval(_waitTime);
        _seq.OnComplete(() =>
        {
            Complete();
        });

        _seq.Play();
    }


    protected override void Complete()
    {
        if(_useInputLock)
            GameSystem.Instance.DecreaseEventSystemLock();

        base.Complete();
    }

    public void Dispose()
    {
        if (_seq == null) return;
        
        _seq.Kill();
        _seq = null;
    }
}
