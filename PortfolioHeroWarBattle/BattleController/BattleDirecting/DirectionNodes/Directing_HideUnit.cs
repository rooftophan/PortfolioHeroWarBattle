using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NewBattleCore;

public class Directing_HideUnit : BaseDirecting
{
    public Directing_HideUnit(Action complete = null) : base(complete)
    {
    }

    public override void Execute()
    {
        int allyUnitCount;
        Unit[] curAllyUnits = new Unit[BattleConfig.MAX_UNIT_COUNT];
        UnitManager.Instance.LiveUnits(NewBattleCore.TroopRelation.Ally, curAllyUnits, out allyUnitCount);

        int enemyUnitCount;
        Unit[] curEnemyUnits = new Unit[BattleConfig.MAX_UNIT_COUNT];
        UnitManager.Instance.LiveUnits(NewBattleCore.TroopRelation.Enemy, curEnemyUnits, out enemyUnitCount);

        for (int i = 0; i < allyUnitCount; i++)
        {
            var allyUnit = curAllyUnits[i];
            if (allyUnit.Enable)
            {
                allyUnit.transform.gameObject.SetActive(false);
                allyUnit.UI.HideHUD();
                allyUnit.FX.PositionEffectHideRestore(false);
            }
        }

        for (int i = 0; i < enemyUnitCount; i++)
        {
            var enemyUnit = curEnemyUnits[i];
            if (enemyUnit.Enable)
            {
                if(enemyUnit.transform != null)
                    enemyUnit.transform.gameObject.SetActive(false);
                if(enemyUnit.UI != null)
                    enemyUnit.UI.HideHUD();
                if(enemyUnit.FX != null)
                    enemyUnit.FX.PositionEffectHideRestore(false);
            }
        }

        Complete();
    }
}
