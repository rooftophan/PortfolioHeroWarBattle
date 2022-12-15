using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using NewBattleCore;
using DG.Tweening;


public class Directing_BackToInitPosition : BaseDirecting
{
    public Directing_BackToInitPosition(Action complete = null):base(complete)
    {
        _complete = complete;
    }

    public override void Execute()
    {
        var units = UnitManager.Instance.LiveUnits(NewBattleCore.TroopRelation.Ally);

        var seq = DOTween.Sequence();
        seq.SetUpdate(false);

        for (int i = 0; i < units.Count; i++)
        {
            var each = units[i];
            seq.Join(MoveSequence(each, each.heroInfo.FirstPosition, each.heroInfo.InitDirect, BattleConfig.MoveSpeed));
        }

        seq.AppendInterval(0.2f);
        seq.OnComplete(() =>
        {
            Complete();
        });

        seq.Play();
    }

    private Sequence MoveSequence(Unit unit, Vector3 goalPos, Vector3 initLookAt, float speed)
    {
        var trans = unit.transform;

        float time = Mathf.Max(0.2f, Vector3.Distance(trans.position, goalPos) / speed);

        float startRotTime = Mathf.Min(0.1f, time / 0.1f);
        float endRotTime = Mathf.Min(0.3f, time / 0.3f);

        var r = DOTween.Sequence();
        
        var moveSeq = DOTween.Sequence();
        moveSeq.Append(trans.DOMove(goalPos, time).SetEase(Ease.Linear));
     
        var runRefeat = DOTween.Sequence();
        runRefeat.AppendInterval(time - endRotTime);
        runRefeat.OnUpdate(() =>
        {
            unit.PlayAnimation("RUN");
        });
        moveSeq.Join(runRefeat);

        var firstLookAt = (goalPos - trans.position).normalized;

        var angle = new Vector3(0f, -Mathf.Atan2(firstLookAt.z, firstLookAt.x) * Mathf.Rad2Deg + 90f, 0f);
        var firstRot = DOTween.Sequence();
        firstRot.Append(trans.DORotate(angle, startRotTime));

        angle = new Vector3(0f, -Mathf.Atan2(initLookAt.z, initLookAt.x) * Mathf.Rad2Deg + 90f, 0f);
        var secondRot = DOTween.Sequence();
        secondRot.Append(trans.DORotate(angle, endRotTime));
 
        var rot = DOTween.Sequence();

        rot.Append(firstRot);
        rot.AppendInterval(time - (startRotTime));
        rot.AppendCallback(() => { unit.PlayAnimation("WAIT"); });
        rot.Append(secondRot);

        r.AppendCallback(() => { unit.PlayAnimation("RUN"); });
        r.Append(moveSeq);
        
        r.Join(rot);

      //  r.AppendCallback(() => { unit.PlayAnimation("WAIT"); });

        return r;
    }

}


