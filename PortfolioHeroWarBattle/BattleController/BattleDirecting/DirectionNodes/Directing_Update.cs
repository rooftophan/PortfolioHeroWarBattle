using DG.Tweening;
using System;

public class Directing_Update : BaseDirecting
{

    private float _updateTime;
    Action<float> _onUpdate;
    private Tween _seq;
    private bool _isLateUpdate;
    private Ease _ease;
    public Directing_Update(Action complete, Action<float> onUpdate, float updateTime, bool isLateUpdate = false, Ease ease = Ease.Linear) : base(complete)
    {
        _updateTime = updateTime;
        _onUpdate = onUpdate;
        _isLateUpdate = isLateUpdate;
        _ease = ease;
    }

    public override void Execute()
    {

        float step = 0f;

        _seq = DOTween.To(() => step, (x) => 
        {
            step = x;
            if (_onUpdate != null)
                _onUpdate(step);
        }, 1f, _updateTime).SetEase(_ease);
        _seq.SetUpdate((_isLateUpdate) ? UpdateType.Late : UpdateType.Normal, false);
        _seq.OnComplete(() =>
        {
            Complete();
        });

        _seq.Play();
    }


    protected override void Complete()
    {
        Dispose();
        base.Complete();
    }

    public void Dispose()
    {
        if (_seq != null)
        {
            _seq.Kill();
        }
    }
}
