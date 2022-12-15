using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NewBattleCore;
using DG.Tweening;

public class Directing_CameraPause : BaseDirecting
{
    protected float _delay;
    public Directing_CameraPause(Action complete, float delay = 0f) : base(complete)
    {
        _delay = delay;
    }

    public override void Execute()
    {
        BattleManager.Instance.battleCamera.Pause();
        Complete();

    }

}

public class Directing_CameraResume : Directing_CameraPause
{
    public Directing_CameraResume(Action complete, float delay = 0f) : base(complete, delay) { }

    public override void Execute()
    {
        BattleManager.Instance.battleCamera.Resume();
        Complete();
    }

}