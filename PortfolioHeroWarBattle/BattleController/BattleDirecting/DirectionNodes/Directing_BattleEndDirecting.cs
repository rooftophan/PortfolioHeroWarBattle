using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;


public class Directing_BattleEndDirecting : Directing_Group
{

    public Directing_BattleEndDirecting(Action complete,
        string battleEndText, List<BattleDirectingStepInfo.BattleDirectingInfo> waveInfo = null, string prefabRes = Res.PREFAB.UIBattleEndUGUI, string textKeyPath = "UIBattleEndUGUI/Text", float delay = 1f, Action<CurrentBattleDirectingWaveInfo> onStartDirecting = null, Action<CurrentBattleDirectingWaveInfo> onEndDirectingType = null) : base(complete)
    {
        EnqueDirecting(new Directing_CreateUIRootNode());
        EnqueDirecting(new Directing_InstantiateUINode(null, prefabRes, Vector3.zero));
        EnqueDirecting(new Directing_UILabelNode(null, textKeyPath, battleEndText, delay));
        EnqueDirecting(new Directing_RemoveUIRootNode());
        EnqueDirecting_WaveInfo(waveInfo, onStartDirecting, onEndDirectingType);

    }

    private void EnqueDirecting_WaveInfo(List<BattleDirectingStepInfo.BattleDirectingInfo> waveInfo, Action<CurrentBattleDirectingWaveInfo> onStartDirecting = null, Action<CurrentBattleDirectingWaveInfo> onEndDirectingType = null)
    {
        if (waveInfo == null)
            return;

        for (int i = 0; i < waveInfo.Count; i++) {
            if (!waveInfo[i].IsActive) continue;

            if (waveInfo[i].DirectingType == BattleDirectingType.InGameDirectingTimeLineCutScene &&
                !NewBattleCore.BattleManager.ShowBossDirectingAlways()) {
                continue;
            }

            EnqueDirecting(BattleDirectingManager.CreateCutSceneDirecting(waveInfo[i], onStartDirecting, onEndDirectingType));
        }
    }

}
