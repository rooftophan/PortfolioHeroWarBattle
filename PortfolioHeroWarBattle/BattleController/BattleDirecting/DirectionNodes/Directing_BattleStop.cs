using System;
using System.Collections;
using System.Collections.Generic;
using NewBattleCore;

public class Directing_BattleStop : BaseDirecting
{
    public Directing_BattleStop(Action complete = null) : base(complete)
    {
    }

    public override void Execute()
    {
        BattleManager.Instance.scene.BCHandler.Stop();
        Complete();
    }
}
