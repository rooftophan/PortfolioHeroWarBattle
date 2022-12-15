using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NewBattleCore;

public class Directing_ShowUnit : BaseDirecting
{
    public Directing_ShowUnit(Action complete = null) : base(complete)
    {
    }

    public override void Execute()
    {
        DirectingFadeInOut.CleanUp();

        //var scene = BattleManager.Instance.scene;

        BattleManager.Instance.battleCamera.Revert();
        BattleManager.Instance.battleCamera.Default();
        List<Unit> ally = NewBattleCore.BattleManager.Instance.CurrentAllyUnits;
        List<Unit> enemy = NewBattleCore.BattleManager.Instance.Enemies;

        //List<Hero> ally = _scene.Field[TroopRelation.Ally];
        for (int i = 0; i < ally.Count; i++)
        {
            var each = ally[i];
			if (each.IsDying || each.IsDead)
				continue;
			
			each.transform.gameObject.SetActive(true);
            each.FX.PositionEffectHideRestore(true);
        }

        //List<Hero> enemy = _scene.Field[TroopRelation.Enemy];
        for (int i = 0; i < enemy.Count; i++)
        {
            var each = enemy[i];

            if (each.transform == null)
                continue;
			if (each.IsDying || each.IsDead)
				continue;

            each.transform.gameObject.SetActive(true);
            each.FX.PositionEffectHideRestore(true);
        }
        
        InGameDirectingHandler.CleanUp(ally, enemy);

        Complete();
    }
}
