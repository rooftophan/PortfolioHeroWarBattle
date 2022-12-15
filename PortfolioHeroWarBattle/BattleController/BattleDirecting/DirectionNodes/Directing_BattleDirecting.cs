using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NewBattleCore;

public class Directing_BattleDirecting : BaseDirecting
{
    public Directing_BattleDirecting(Action complete = null) : base(complete)
    {
    }

    public override void Execute()
    {
        BattleManager.Instance.scene.BCHandler.StartDirecting2();
        Complete();
    }
}
