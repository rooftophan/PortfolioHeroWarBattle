using System;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using NewBattleCore;
public class Directing_ComeBackIntoPlace : Directing_Group
{
    public Directing_ComeBackIntoPlace(Action complete, List<BattleDirectingStepInfo.BattleDirectingInfo> waveEndDirectingWaveInfos,
        List<BattleDirectingStepInfo.BattleDirectingInfo> afterWaveInfo, Action OnWaveChange) : base(complete)
    {
        UnitManager.Instance.InitByCombackToInitPos();
        EnqueDirecting(new Directing_BackToInitPosition(null));
        EnqueDirecting(new Directing_CameraPause(null));

        bool isPreDirecting = GetDirecting(waveEndDirectingWaveInfos);
        if(isPreDirecting) {
            EnqueDirecting(new Directing_HideUnit());
            EnqueDirecting_WaveInfo(waveEndDirectingWaveInfos);
            EnqueDirecting(new Directing_ShowUnit());
        }
        
        EnqueDirecting(new Directing_CallBack(null, OnWaveChange));

        bool isAfterDirecting = GetDirecting(afterWaveInfo);

        if(isAfterDirecting) {
            EnqueDirecting(new Directing_HideUnit());
            EnqueDirecting_WaveInfo(afterWaveInfo);
            EnqueDirecting(new Directing_ShowUnit());
        }
        
        EnqueDirecting(new Directing_BattleDirecting(null));
        EnqueDirecting(new Directing_CameraResume(null));

    }

    bool GetDirecting(List<BattleDirectingStepInfo.BattleDirectingInfo> directings)
    {
        if(directings != null && directings.Count > 0) {
            for(int i = 0;i< directings.Count;i++) {
                if (directings[i].IsActive) {
                    if(directings[i].DirectingType != BattleDirectingType.InGameDirectingTimeLineCutScene) {
                        return true;
                    } else {
                        if (BattleManager.ShowBossDirectingAlways() == true)
                            return true;
                    }
                }
            }
        }

        return false;
    }
    
    private void EnqueDirecting_WaveInfo(List<BattleDirectingStepInfo.BattleDirectingInfo> waveInfo)
    {
        if (waveInfo == null)
            return;

        for (int i = 0; i < waveInfo.Count; i++) {
            if (!waveInfo[i].IsActive)
                continue;

            if(waveInfo[i].DirectingType == BattleDirectingType.InGameDirectingTimeLineCutScene &&
                !BattleManager.ShowBossDirectingAlways() ) {
                continue;
            }

            EnqueDirecting(BattleDirectingManager.CreateCutSceneDirecting(waveInfo[i]));
        }
    }
}
