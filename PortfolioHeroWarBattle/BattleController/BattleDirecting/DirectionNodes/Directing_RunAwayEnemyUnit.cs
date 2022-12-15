using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using NewBattleCore;
using DG.Tweening;


public class Directing_RunAwayEnemyUnit : BaseDirecting
{
    #region Variables

    Unit[] _liveUnits;

    #endregion

    #region Methods

    public Directing_RunAwayEnemyUnit(Action complete = null):base(complete)
    {
        _complete = complete;
    }

    public override void Execute()
    {
        _liveUnits = UnitManager.Instance.LiveUnits();
        for (int i = 0; i < _liveUnits.Length; i++) {
            Unit u = _liveUnits[i];
            u.CrossFade("WAIT");
        }
        List<Unit> enemyUnits = UnitManager.Instance.LiveUnits(NewBattleCore.TroopRelation.Enemy, UnitType.Battler);

        var seq = DOTween.Sequence();

        seq.OnUpdate(UpdateTween);
        for (int i = 0; i < enemyUnits.Count; i++)
        {
            Unit enemyUnit = enemyUnits[i];
            Vector3 goalPos = new Vector3(enemyUnit.transform.position.x + 20f, 0f, enemyUnit.transform.position.z);
            seq.Join(MoveSequence(enemyUnit, goalPos, 2f));
        }

        seq.AppendInterval(0.2f);
        seq.OnComplete(() =>
        {
            Complete();
        });

        seq.Play();
    }

    private Sequence MoveSequence(Unit unit, Vector3 goalPos, float moveTime)
    {
        Sequence retSequence = DOTween.Sequence();
        Transform unitTrans = unit.transform;

        Vector3 destAngle = new Vector3(0f, 90f, 0f);

        retSequence.Append(unitTrans.DORotate(destAngle, 0.1f));
        retSequence.AppendCallback(() =>
        {
            unit.CrossFade("RUN");
        });
        retSequence.Append(unitTrans.DOMove(goalPos, moveTime).SetEase(Ease.Linear));
        retSequence.AppendCallback(() =>
        {
            unit.CrossFade("WAIT");
        });

        return retSequence;
    }

    void UpdateTween()
    {
        if (_liveUnits != null) {
            for (int i = 0; i < _liveUnits.Length; i++) {
                Unit u = _liveUnits[i];
                u.animationHandler.Update(Time.deltaTime);
            }
        }
    }

    #endregion
}


