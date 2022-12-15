using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Directing_CallBack : BaseDirecting
{
    private Action _callBack;
    public Directing_CallBack(Action complete, Action callBack) : base(complete)
    {
        _callBack = callBack;
    }

    public override void Execute()
    {
        if(_callBack != null)
            _callBack();
        Complete();
    }
}
