using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NewBattleCore;
using System;

public class Directing_HideBattleUI : BaseDirecting
{
    public Directing_HideBattleUI(Action complete = null) : base(complete)
    {
    }

    public override void Execute()
    {
        UIBattle.This.HideBattle();
        //BattleManager.Instance.scene.UI.gameObject.SetActive(false);
        Complete();
    }
}
