using System;
using DG.Tweening;
using System.Collections.Generic;
using NewBattleCore;
using UnityEngine;
using UnityEngine.UI;

public class Directing_CutSceneInGame : BaseDirecting
{

    protected CurrentBattleDirectingWaveInfo _waveInfo;
    protected CutScene _cutScene = null;

    protected BattleScene _scene;
    protected string _skipButtonResPath;
    protected GameObject _skipButtonObj;

    private InGameDirectingCutscene _inGameCutScene;

    protected Action<CurrentBattleDirectingWaveInfo> _onStartDirecting = null;
    protected Action<CurrentBattleDirectingWaveInfo> _onEndDirectingType = null;



    public Directing_CutSceneInGame(Action complete, CurrentBattleDirectingWaveInfo waveInfo, Action<CurrentBattleDirectingWaveInfo> onStartDirecting = null, Action<CurrentBattleDirectingWaveInfo> onEndDirectingType = null) : base(complete)
    {
        _waveInfo = waveInfo;
        //_skipButtonResPath = "InGameDirecting/CutSceneSkipButtonForSpeech";
        _skipButtonResPath = "InGameDirecting/CutSceneSKipButton";
        _onStartDirecting = onStartDirecting;
        _onEndDirectingType = onEndDirectingType;

        LoadOnCutScene();
    }

    private void LoadOnCutScene()
    {
        _cutScene = CreateCutScene();

        if (_cutScene == null)
        {
            //base.Complete();
            return;
        }

        _cutScene.gameObject.SetActive(false);
        _scene = BattleManager.Instance.scene;
        //InitCutScene();
    }

    public override void Execute()
    {
        //_cutScene = CreateCutScene();

        if (_cutScene == null) {
            base.Complete();
            return;
        }

        if (ChattingController.Instance != null && ChattingController.Instance.IsChattingPopup) {
            ChattingController.Instance.IsOpenCloseState = true;
            ChattingController.Instance.UIMultiChat.CloseChattingPopup();
        }

        _cutScene.gameObject.SetActive(true);
        InitCutScene();

        if (_onStartDirecting != null)
        {
            _onStartDirecting(_waveInfo);
        }

        //_scene = BattleManager.Instance.scene;
        //InitCutScene();

        _skipButtonObj = CreateSkipButton();
        if (_cutScene != null)_cutScene.Excute(OnEndCutScene);

        ExecuteCutScene();
    }

    #region CreateObject
    private CutScene CreateCutScene()
    {
        if (!_waveInfo.IsActive)
            return null;

        string cutscenefileName = _waveInfo.DirectingPath;
        string cutScenePrefabPath = "CutScene/Prefab/";

        GameObject target = ResourceLoader.Instantiate(cutScenePrefabPath + cutscenefileName);
        if (target == null)
        {
            Debug.Log(string.Format("SetTimeLineCutScene target == null CutScenePrefabPath : {0}", cutScenePrefabPath + cutscenefileName));
            return null;
        }

        return target.GetComponent<CutScene>();
    }

    private GameObject CreateSkipButton()
    {
        GameObject skipObj = ResourceLoader.Instantiate(_skipButtonResPath, typeof(GameObject), null) as GameObject;
        Button skipBtn = skipObj.GetComponentInChildren<Button>();
        skipBtn.onClick.RemoveAllListeners();
        skipBtn.onClick.AddListener(() => { _cutScene.Stop(); });
        return skipObj;
    }

    #endregion


    protected virtual void InitCutScene()
    {
        _inGameCutScene = (InGameDirectingCutscene)_cutScene;

        var allys = UnitManager.Instance.LiveUnits(NewBattleCore.TroopRelation.Ally);
        var enemys = UnitManager.Instance.LiveUnits(NewBattleCore.TroopRelation.Enemy);

        //_inGameCutScene.Setting(_scene.Sheet);
        _inGameCutScene.Setting();
        _inGameCutScene.SetSheet(_scene.Sheet);
        _inGameCutScene.LoadNBindingFromGame(_scene.System.Sheet.SheetHero, allys, enemys);
        BattleManager.Instance.battleCamera.mainCamera.gameObject.SetActive(false);
        
        InitUnit(NewBattleCore.TroopRelation.Ally);
        InitUnit(NewBattleCore.TroopRelation.Enemy);
        
        UnitManager.Instance.InitWaveForCutScene();
    }

    protected virtual void InitUnit(NewBattleCore.TroopRelation troopRelation)
    {
        var units = UnitManager.Instance.LiveUnits(troopRelation);

        for (int i = 0; i < units.Count; i++)
        {
            if(units[i].transform == null)
                continue;
            GameObject each = units[i].transform.gameObject;
            if (each == null)
                continue;
            SceneStartUnit(units[i]);
        }
    }

    protected virtual void SceneStartUnit(Unit unit)
    {
        unit.obj.SetActive(false);
        unit.FX.PositionEffectHideRestore(false);
    }

    protected virtual void SceneEndUnit(Unit unit)
    {
        unit.obj.SetActive(true);
        unit.FX.PositionEffectHideRestore(true);
        unit.animationHandler.Stop();
        unit.animationHandler.Play("WAIT");
    }

    private void OnEndCutScene()
    {
        var units = UnitManager.Instance.LiveUnits();
        foreach (var each in units)
            SceneEndUnit(each);

        _cutScene.Dispose();

        GameObject.Destroy(_skipButtonObj);
        GameObject.Destroy(_cutScene.gameObject);

        BattleManager.Instance.battleCamera.mainCamera.gameObject.SetActive(true);
        BattleManager.Instance.battleCamera.Revert();
        BattleManager.Instance.battleCamera.Default();

        if( BattleHandler.Instance != null )
        {
            //BattleHandler.Instance.EndCutScene();
            BattleHandler.Instance.EndDirecting();
        }
            
        
        Complete();
    }

    private void ExecuteCutScene()
    {
        if( BattleHandler.Instance != null )
        {
            BattleHandler.Instance.StartDirecting();
        }

        var seq = DOTween.Sequence();
        seq.SetUpdate(false);

        seq.OnUpdate(() => {

            //if (_cutScene != null)
            //    _cutScene.Update();

            if (_isComplete)
                seq.Kill(false);
        });
        seq.AppendInterval(float.MaxValue);

        seq.Play();
    }

    protected override void Complete()
    {
        if(_onEndDirectingType != null)
            _onEndDirectingType(_waveInfo);

        Debug.Log("End CutScene!");
        base.Complete();

        if (ChattingController.Instance != null && ChattingController.Instance.IsOpenCloseState) {
            ChattingController.Instance.UIMultiChat.ShowChattingPopup();
        }
    }

}
