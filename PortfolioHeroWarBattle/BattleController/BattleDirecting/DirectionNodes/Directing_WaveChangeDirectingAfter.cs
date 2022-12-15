using System;
using NewBattleCore;
using System.Collections.Generic;
using UnityEngine;

public class Directing_WaveChangeDirectingAfter : BaseDirecting
{
    public Directing_WaveChangeDirectingAfter(Action complete = null) : base(complete)
    {
    }

    public override void Execute()
    {
        BattleManager.Instance.scene.UI.DisappearWaveFX(0.5f, Complete);
    }

    protected override void Complete()
    {
        BattleManager.Instance.scene.UI.HideWave();
        base.Complete();
    }
}
