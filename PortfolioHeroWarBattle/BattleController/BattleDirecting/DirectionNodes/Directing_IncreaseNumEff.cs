using System;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class Directing_IncreaseNumEff : BaseDirecting
{
    private float _minTime;
    private float _maxTime;
    private int _minValue;

    private int _goalNum;
    private int _current;
    

    private Action<int> _onUpdate;
    private Directing_Update _updater;
    
    public Directing_IncreaseNumEff(Action complete, Action<int> onUpdate, float minTime, float maxTime, int minValue, int firstValue) : base(complete)
    {
        _minTime = minTime;
        _maxTime = maxTime;
        _minValue = minValue;

        _current = firstValue;
        _goalNum = firstValue;

        _onUpdate = onUpdate;
    }

    public void AddNum(int Num)
    {
        _goalNum += Num;
        Process(_current, _goalNum);
    }

    private void Process(int current, int goal)
    {
        var Num = goal - current;

        if (_updater != null)
            _updater.Dispose();

        if (Mathf.Abs(Num) <= _minValue)
        {
            _onUpdate(_goalNum);
            _current = _goalNum;
            return;
        }

        var limitValue = (_maxTime * Mathf.Abs(Num)) / _minTime;
        var percent = Mathf.Min(1f, (Mathf.Abs(Num) - _minValue) / (_maxTime - _minTime));
        var time = _minTime + percent * (_maxTime - _minTime);

        _updater = new Directing_Update(() => {
            _onUpdate(goal);

        }, (per) => {

            _current = (int)(current + (per * Num));
            _onUpdate(_current);

        }, time);

        _updater.Execute();
    }

    public void Dispose()
    {
        if(_updater != null)
            _updater.Dispose();

        _onUpdate(_goalNum);
    }

    protected override void Complete()
    {
        base.Complete();
    }

}
