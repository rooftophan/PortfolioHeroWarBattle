using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class Directing_Timer : BaseDirecting
{
    private int _remainSecond;
    private int _curSecond;
    private int _startSecond;
    private Action<int, int> _onSecondUpdate;

    private Sequence _seq;

    public Directing_Timer(Action complete, Action<int, int> onSecondUpdate, int remainSecond, int startSecond = 0) : base(complete)
    {
        _remainSecond = Mathf.Max(0, remainSecond);
        _onSecondUpdate = onSecondUpdate;
        _startSecond = Mathf.Max(0, startSecond);

    }

    public override void Execute()
    {
        if (_remainSecond == 0)
        {
            Complete();
            return;
        }

        _curSecond = _startSecond;

        if (_onSecondUpdate != null)
            _onSecondUpdate(_remainSecond, _curSecond);
      

        var seqCallBack = DOTween.Sequence();
        seqCallBack.SetUpdate(true);
        seqCallBack.AppendInterval(1);
        seqCallBack.AppendCallback(()=> {
            if(_onSecondUpdate != null)
                _onSecondUpdate(_remainSecond, ++_curSecond);
        });

        _seq = seqCallBack.SetLoops(_remainSecond - _startSecond);
        _seq.OnComplete(Complete);
        _seq.Play();
    }

    public void Dispose()
    {
        if(_seq != null)
        {
            _seq.Kill();
        }
    }

}