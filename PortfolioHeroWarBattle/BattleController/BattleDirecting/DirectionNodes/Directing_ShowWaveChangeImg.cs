using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NewBattleCore;

public class Directing_ShowWaveChangeImg : BaseDirecting
{
    public Directing_ShowWaveChangeImg(Action complete = null) : base(complete)
    {
    }

    public override void Execute()
    {
        if (UIBattle.This != null)
            UIBattle.This.ShowWaveChangeBase();

        Complete();
    }
}
