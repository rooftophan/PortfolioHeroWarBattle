using System;
using DG.Tweening;
using System.Collections.Generic;
using NewBattleCore;
using UnityEngine;
using UnityEngine.UI;

public class Directing_CutSceneArena : Directing_CutSceneInGame
{

    private ArenaStartCutScene _arenaCutScene;

    public Directing_CutSceneArena(Action complete, CurrentBattleDirectingWaveInfo waveInfo) : base(complete, waveInfo)
    {
        _skipButtonResPath = "InGameDirecting/CutSceneSKipButton";
    }

    protected override void InitCutScene()
    {
        _arenaCutScene = (ArenaStartCutScene)_cutScene;
        _arenaCutScene.Setting();
        _arenaCutScene.BindingAudioSource(_scene.System.AudioController.GetSourceFromPool(_cutScene.transform)?.Source);

        InitUnit(NewBattleCore.TroopRelation.Ally);
        InitUnit(NewBattleCore.TroopRelation.Enemy);

        BattleManager.Instance.battleCamera.mainCamera.gameObject.SetActive(false);
    }

    protected override void InitUnit(NewBattleCore.TroopRelation troopRelation)
    {
        bool isAlly = (troopRelation == NewBattleCore.TroopRelation.Ally);
        var units = UnitManager.Instance.LiveUnits(troopRelation);

        for (int i = 0; i < units.Count; i++)
        {
            GameObject each = units[i].transform.gameObject;
            if (each == null)
                continue;

            _arenaCutScene.BindingCharacter(isAlly, i, each);
            units[i].FX.PositionEffectHideRestore(false);
        }
    }

    protected override void SceneEndUnit(Unit unit)
    {
        unit.FX.PositionEffectHideRestore(true);
        unit.animationHandler.Stop();
        unit.animationHandler.Play("WAIT");
    }

}
