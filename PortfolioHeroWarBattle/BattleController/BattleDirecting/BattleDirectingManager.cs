using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleDirectingManager : DirectingScheduler
{
 
	#region Methods

    public void SetBattleDirectingData(List<BattleDirectingStepInfo.BattleDirectingInfo> inputDirectingWaveInfos,
                                       Action<CurrentBattleDirectingWaveInfo> onStartDirecting = null,
                                       Action<CurrentBattleDirectingWaveInfo> onEndDirectingType = null,
                                       Action onDirectingComplete = null)
	{
        if (inputDirectingWaveInfos == null)
            return;
        
        for (int i = 0; i < inputDirectingWaveInfos.Count; i++) {
            if (!inputDirectingWaveInfos[i].IsActive) continue;

            if (inputDirectingWaveInfos[i].DirectingType == BattleDirectingType.InGameDirectingTimeLineCutScene &&
                !NewBattleCore.BattleManager.ShowBossDirectingAlways()) {
                continue;
            }

            if (inputDirectingWaveInfos[i].DirectingType == BattleDirectingType.InGameDirectingTimeLineCutScene){
                Enqueue(new Directing_FadeOutIn(null, 0.2f, 0.8f, null, null, 0.8f), true);
                Enqueue(new Directing_HideUnit());
                Enqueue(CreateCutSceneDirecting(inputDirectingWaveInfos[i], onStartDirecting, onEndDirectingType, onDirectingComplete));
                Enqueue(new Directing_ShowUnit());
            } else {
                Enqueue(CreateCutSceneDirecting(inputDirectingWaveInfos[i], onStartDirecting, onEndDirectingType, onDirectingComplete));
            }
        }
    }

    public void EnqueueCutScene(List<BattleDirectingStepInfo.BattleDirectingInfo> inputDirectingWaveInfos, 
                                Action<CurrentBattleDirectingWaveInfo> onStartDirecting = null, 
                                Action<CurrentBattleDirectingWaveInfo> onEndDirectingType = null,
                                Action onDirectingComplete = null)
    {
        SetBattleDirectingData(inputDirectingWaveInfos, onStartDirecting, onEndDirectingType, onDirectingComplete);
    }

    public void EnqueueAfterCutScene(List<BattleDirectingStepInfo.BattleDirectingInfo> inputDirectingWaveInfos, 
                                     Action<CurrentBattleDirectingWaveInfo> onStartDirecting = null,
                                     Action<CurrentBattleDirectingWaveInfo> onEndDirectingType = null,
                                     Action onAfterDirectingComplete = null)
    {
        if (inputDirectingWaveInfos == null) {
            Enqueue(new Directing_WaveChangeDirectingAfter());
            return;
        }

        List<BattleDirectingStepInfo.BattleDirectingInfo> validDirectingList = new List<BattleDirectingStepInfo.BattleDirectingInfo>();

        for (int i = 0; i < inputDirectingWaveInfos.Count; i++) {
            if (!inputDirectingWaveInfos[i].IsActive) continue;

            if (inputDirectingWaveInfos[i].DirectingType == BattleDirectingType.InGameDirectingTimeLineCutScene &&
                !NewBattleCore.BattleManager.ShowBossDirectingAlways()) {
                continue;
            }

            validDirectingList.Add(inputDirectingWaveInfos[i]);
        }

        if (validDirectingList.Count > 0) {
            bool isDirectingAfter = false;
            for (int i = 0; i < validDirectingList.Count; i++) {
                if (validDirectingList[i].DirectingType == BattleDirectingType.InGameDirectingTimeLineCutScene) {
                    Enqueue(new Directing_FadeOutIn(null, 0.2f, 0.8f, null, null, 0.8f), true);
                    Enqueue(new Directing_HideUnit());
                    Enqueue(CreateCutSceneDirecting(validDirectingList[i], onStartDirecting, onEndDirectingType, onAfterDirectingComplete));
                    Enqueue(new Directing_ShowUnit());
                    if (!isDirectingAfter) {
                        isDirectingAfter = true;
                        Enqueue(new Directing_WaveChangeDirectingAfter());
                    }
                } else {
                    if (!isDirectingAfter) {
                        isDirectingAfter = true;
                        Enqueue(new Directing_WaveChangeDirectingAfter());
                    }
                    Enqueue(CreateCutSceneDirecting(validDirectingList[i], onStartDirecting, onEndDirectingType, onAfterDirectingComplete));
                }
            }
        } else {
            Enqueue(new Directing_WaveChangeDirectingAfter());
        }
    }

    public static BaseDirecting CreateCutSceneDirecting(BattleDirectingStepInfo.BattleDirectingInfo waveInfo, 
                                                        Action<CurrentBattleDirectingWaveInfo> onStartDirecting = null, 
                                                        Action<CurrentBattleDirectingWaveInfo> onEndDirectingType = null, 
                                                        Action onDirectingComplete = null)
    {

        BaseDirecting directing = null;

        var waveDirecting = GetCurrentBattleDirectingWaveInfo(waveInfo);
        switch (waveInfo.DirectingType)
        {
            case BattleDirectingType.NormalReactor:
                directing = new Directing_WaveStartDirecting(onDirectingComplete, waveDirecting, onStartDirecting, onEndDirectingType);
                break;
            case BattleDirectingType.ArenaTimelineCutScene:
                directing = new Directing_CutSceneArena(onDirectingComplete, waveDirecting);
                break;
            case BattleDirectingType.InGameDirectingTimeLineCutScene:
                directing = new Directing_CutSceneInGame(onDirectingComplete, waveDirecting, onStartDirecting, onEndDirectingType);
                break;
        }

        return directing;
    }

    public static CurrentBattleDirectingWaveInfo GetCurrentBattleDirectingWaveInfo(BattleDirectingStepInfo.BattleDirectingInfo directingWaveInfo)
	{
		CurrentBattleDirectingWaveInfo retValue = new CurrentBattleDirectingWaveInfo ();

        retValue.IsActive = directingWaveInfo.IsActive;
        retValue.DirectingType = directingWaveInfo.DirectingType;
        retValue.DirectingPath = directingWaveInfo.DirectingPath;
        retValue.DirectingValues = directingWaveInfo.DirectingValues;

        return retValue;
	}

#endregion

}
