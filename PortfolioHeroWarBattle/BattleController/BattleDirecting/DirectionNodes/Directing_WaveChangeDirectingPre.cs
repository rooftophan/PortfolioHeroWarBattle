using System;
using NewBattleCore;
using System.Collections.Generic;
using UnityEngine;

public class Directing_WaveChangeDirectingPre : BaseDirecting
{
    public static bool isDirectingState = true;

    public Directing_WaveChangeDirectingPre(Action complete = null) : base(complete)
    {
    }

    public override void Execute()
    {
        if(!isDirectingState) {
            isDirectingState = true;
            Complete();
            return;
        }
        BattleManager.Instance.scene.UI.ShowWave();
        BattleManager.Instance.scene.UI.AppearWaveFX(0.5f, Complete);
    }
}
