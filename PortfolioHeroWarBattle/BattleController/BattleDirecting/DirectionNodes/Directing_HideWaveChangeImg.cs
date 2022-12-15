using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NewBattleCore;

public class Directing_HideWaveChangeImg : BaseDirecting
{
    public Directing_HideWaveChangeImg(Action complete = null) : base(complete)
    {
    }

    public override void Execute()
    {
        if(UIBattle.This != null)
            UIBattle.This.HideWaveChangeBase();

        Complete();
    }
}
