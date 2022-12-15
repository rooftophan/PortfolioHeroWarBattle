using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NewBattleCore;

public class Directing_BattleStart : BaseDirecting
{
    public Directing_BattleStart(Action complete = null) : base(complete)
    {
    }

    public override void Execute()
    {
        BattleManager.Instance.scene.BCHandler.Start();
        Complete();
    }
}
