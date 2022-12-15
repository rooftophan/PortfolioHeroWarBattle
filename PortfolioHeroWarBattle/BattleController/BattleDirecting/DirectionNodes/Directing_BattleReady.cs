using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NewBattleCore;

public class Directing_BattleReady : BaseDirecting
{
    public Directing_BattleReady(Action complete = null) : base(complete)
    {
    }

    public override void Execute()
    {
        BattleManager.Instance.scene.BCHandler.Ready();
        BattleManager.Instance.battleCamera.Resume();
        Complete();
    }
}
