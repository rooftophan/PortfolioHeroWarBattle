using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public interface IBattleStartableStep
{
	void SetSceneLoad();
	void SetBattleMapLoad();
	void SetHandlerInitUnitLoad();
	void SetBattleDataSetting();
	void SetDirecting();
	void SetDeploy();
	void SetBattling();
    void SetChangeWaveNetRequest();
	void SetChangeWave();

    void SetChangeWaveUnloadScene();
    void SetChangeWaveLoadScene();
	void SetBattleEndDirecting();
	void SetBattleEndStoryScript();
	void SetNetRequestBattleEnd();
	void SetBattleEnded();
	void SetTutorialBattleStart();
    void FinishProgressBattleStep(BattleDefinitions.BattleStep step, float value);
}

public class BattleStepData
{
	#region Variables

	BattleDefinitions.BattleStep _battleStep;
	object _battleStepObject;
	Action _onStartStep;
	Action<BattleDefinitions.BattleStep> _onFinishStep;
    Action<BattleDefinitions.BattleStep, float> _onFinishProgressStep;
	#endregion

	#region Properties

	public BattleDefinitions.BattleStep BattleStep
	{
		get{ return _battleStep; }
		set{ _battleStep = value; }
	}

	public object BattleStepObject
	{
		get{ return _battleStepObject; }
		set{ _battleStepObject = value; }
	}

	public Action OnStartStep
	{
		get{ return _onStartStep; }
		set{ _onStartStep = value; }
	}

	public Action<BattleDefinitions.BattleStep> OnFinishStep
	{
		get{ return _onFinishStep; }
		set{ _onFinishStep = value; }
	}
   
    public Action<BattleDefinitions.BattleStep, float> OnFinishProgressStep
    {
        get { return _onFinishProgressStep; }
        set { _onFinishProgressStep = value; }
    }


	#endregion
}

public class BattleStepManager
{
	#region Variables

	int _curStepPosIndex = 0;

    public  static readonly int stepPrograssTotalCount = 13;
    public  static float stepPrograssCount = 0f;
	Dictionary<BattleDefinitions.BattleStep, BattleStepData> _battleStepInfos = new Dictionary<BattleDefinitions.BattleStep, BattleStepData> ();

	BattleDefinitions.BattleStep _curBattleStep = BattleDefinitions.BattleStep.None;
	BattleDefinitions.BattleStep _reserveBattleStep = BattleDefinitions.BattleStep.None;

	BattleDirectingManager _directingManager = null;

	IBattleStartableStep _battleStartStepActions;
	Action<BattleDefinitions.BattleStep> _onFinishStep;

    BattleDirectingStepInfo _curBattleDirectingObject = null;

	#endregion

	#region Properties

	public int CurStepPosIndex
	{
		get{ return _curStepPosIndex; }
		set{ _curStepPosIndex = value; }
	}

	public Dictionary<BattleDefinitions.BattleStep, BattleStepData> BattleStepInfos
	{
		get{ return _battleStepInfos; }
	}

	public BattleDefinitions.BattleStep CurBattleStep
	{
		get{ return _curBattleStep; }
		set{ _curBattleStep = value; }
	}

	public BattleDefinitions.BattleStep ReserveBattleStep
	{
		get{ return _reserveBattleStep; }
		set{ _reserveBattleStep = value; }
	}

	public BattleDirectingManager DirectingManager
	{
		get{ return _directingManager; }
		set{ _directingManager = value; }
	}

	public IBattleStartableStep BattleStartStepActions
	{
		get{ return _battleStartStepActions; }
		set{ _battleStartStepActions = value; }
	}

	public Action<BattleDefinitions.BattleStep> OnFinishStep
	{
		get{ return _onFinishStep; }
		set{ _onFinishStep = value; }
	}

	public BattleDirectingStepInfo CurBattleDirectingObject
	{
		get{ return _curBattleDirectingObject; }
	}

	#endregion

	#region Methods

	public void InitDefaultStep()
	{
		_battleStepInfos.Clear ();

		AddBattleStep (BattleDefinitions.BattleStep.SceneLoad);
		AddBattleStep (BattleDefinitions.BattleStep.BattleMapLoad);
		AddBattleStep (BattleDefinitions.BattleStep.HandlerInitUnitLoad);
		AddBattleStep (BattleDefinitions.BattleStep.BattleDataSetting);
        AddBattleStep (BattleDefinitions.BattleStep.TutorialBattleStart);
		AddBattleStep (BattleDefinitions.BattleStep.Directing);
        AddBattleStep(BattleDefinitions.BattleStep.Deploy);
        AddBattleStep (BattleDefinitions.BattleStep.Battling);
        AddBattleStep (BattleDefinitions.BattleStep.ChangeWave);
        AddBattleStep (BattleDefinitions.BattleStep.ChangeWaveUnloadScene);
        AddBattleStep (BattleDefinitions.BattleStep.ChangeWaveLoadScene);
        AddBattleStep (BattleDefinitions.BattleStep.BattleEndDirecting);
		AddBattleStep (BattleDefinitions.BattleStep.BattleEndStoryScript);
		AddBattleStep (BattleDefinitions.BattleStep.NetRequestBattleEnd);
		AddBattleStep (BattleDefinitions.BattleStep.BattleEnded);
	}

    public void InitGuildWarStep()
    {
        _battleStepInfos.Clear();

        AddBattleStep(BattleDefinitions.BattleStep.SceneLoad);
        AddBattleStep(BattleDefinitions.BattleStep.BattleMapLoad);
        AddBattleStep(BattleDefinitions.BattleStep.HandlerInitUnitLoad);
        AddBattleStep(BattleDefinitions.BattleStep.BattleDataSetting);
        AddBattleStep(BattleDefinitions.BattleStep.TutorialBattleStart);
        AddBattleStep(BattleDefinitions.BattleStep.Directing);
        AddBattleStep(BattleDefinitions.BattleStep.Deploy);
        AddBattleStep(BattleDefinitions.BattleStep.Battling);
        AddBattleStep(BattleDefinitions.BattleStep.ChangeWaveNetRequest);
        AddBattleStep(BattleDefinitions.BattleStep.ChangeWave);
        AddBattleStep(BattleDefinitions.BattleStep.ChangeWaveUnloadScene);
        AddBattleStep(BattleDefinitions.BattleStep.ChangeWaveLoadScene);
        AddBattleStep(BattleDefinitions.BattleStep.BattleEndDirecting);
        AddBattleStep(BattleDefinitions.BattleStep.BattleEndStoryScript);
        AddBattleStep(BattleDefinitions.BattleStep.NetRequestBattleEnd);
        AddBattleStep(BattleDefinitions.BattleStep.BattleEnded);
    }

	public void LoadBattleDirectingObject(string battleDirectingPath = "")
	{
		if (string.IsNullOrEmpty(battleDirectingPath))
            return;
		
		_curBattleDirectingObject = ResourceLoader.Instantiate<BattleDirectingStepInfo>( battleDirectingPath );
	}

    public void AddBattleStep(BattleDefinitions.BattleStep step, object stepObject = null, Action startStep = null, Action<BattleDefinitions.BattleStep> finishStep = null)
	{
		BattleStepData inputBattleStep = new BattleStepData ();
		inputBattleStep.BattleStep = step;
		inputBattleStep.BattleStepObject = stepObject;

        switch (step) {
            case BattleDefinitions.BattleStep.SceneLoad:
                inputBattleStep.OnStartStep = _battleStartStepActions.SetSceneLoad;
                break;
            case BattleDefinitions.BattleStep.BattleMapLoad:
                inputBattleStep.OnStartStep = _battleStartStepActions.SetBattleMapLoad;
                break;
            case BattleDefinitions.BattleStep.HandlerInitUnitLoad:
                inputBattleStep.OnStartStep = _battleStartStepActions.SetHandlerInitUnitLoad;
                break;
            case BattleDefinitions.BattleStep.BattleDataSetting:
                inputBattleStep.OnStartStep = _battleStartStepActions.SetBattleDataSetting;
                break;
            case BattleDefinitions.BattleStep.TutorialBattleStart:
                inputBattleStep.OnStartStep = _battleStartStepActions.SetTutorialBattleStart;
                break;
            case BattleDefinitions.BattleStep.Directing:
                inputBattleStep.OnStartStep = _battleStartStepActions.SetDirecting;
                break;
            case BattleDefinitions.BattleStep.Deploy:
                inputBattleStep.OnStartStep = _battleStartStepActions.SetDeploy;
                break;
            case BattleDefinitions.BattleStep.Battling:
                inputBattleStep.OnStartStep = _battleStartStepActions.SetBattling;
                break;
            case BattleDefinitions.BattleStep.ChangeWaveNetRequest:
                inputBattleStep.OnStartStep = _battleStartStepActions.SetChangeWaveNetRequest;
                break;
            case BattleDefinitions.BattleStep.ChangeWave:
                inputBattleStep.OnStartStep = _battleStartStepActions.SetChangeWave;
                break;
            case BattleDefinitions.BattleStep.ChangeWaveUnloadScene:
                inputBattleStep.OnStartStep = _battleStartStepActions.SetChangeWaveUnloadScene;
                break;
            case BattleDefinitions.BattleStep.ChangeWaveLoadScene:
                inputBattleStep.OnStartStep = _battleStartStepActions.SetChangeWaveLoadScene;
                break;
            case BattleDefinitions.BattleStep.BattleEndDirecting:
                inputBattleStep.OnStartStep = _battleStartStepActions.SetBattleEndDirecting;
                break;
            case BattleDefinitions.BattleStep.BattleEndStoryScript:
                inputBattleStep.OnStartStep = _battleStartStepActions.SetBattleEndStoryScript;
                break;
            case BattleDefinitions.BattleStep.NetRequestBattleEnd:
                inputBattleStep.OnStartStep = _battleStartStepActions.SetNetRequestBattleEnd;
                break;
            case BattleDefinitions.BattleStep.BattleEnded:
                inputBattleStep.OnStartStep = _battleStartStepActions.SetBattleEnded;
                break;
            default:
                inputBattleStep.OnStartStep = startStep;
                break;
        }
                

        if (finishStep == null) {
			inputBattleStep.OnFinishStep = _onFinishStep;
		} else {
			inputBattleStep.OnFinishStep = finishStep;
		}

        inputBattleStep.OnFinishProgressStep = _battleStartStepActions.FinishProgressBattleStep;

		_battleStepInfos.Add (step, inputBattleStep);
	}
        
	public void StartFirstBattleStep()
	{
		_curStepPosIndex = 0;
		List<BattleDefinitions.BattleStep> stepKeyValues = _battleStepInfos.Keys.ToList ();
		SetReserveBattleStep (_battleStepInfos [stepKeyValues[_curStepPosIndex]].BattleStep);

        //LoadTimeProfiler.Start(_reserveBattleStep.ToString());

        if( UIHelpScreenUGUI.Instance != null )
         UIHelpScreenUGUI.Instance.SetProgress( NewBattleCore.BattleManager.RandomRange(0.1f, 0.3f) );
	}

	public void SetCurrentBattleStep()
	{
		if (_battleStepInfos.Count <= _curStepPosIndex)
			return;

		List<BattleDefinitions.BattleStep> stepKeyValues = _battleStepInfos.Keys.ToList ();
		_curBattleStep = _battleStepInfos [stepKeyValues[_curStepPosIndex]].BattleStep;
		_reserveBattleStep = _curBattleStep;
		_battleStepInfos [stepKeyValues[_curStepPosIndex]].OnStartStep ();
	}

	public void SetReserveBattleStep(BattleDefinitions.BattleStep step)
	{
		_reserveBattleStep = step;
	}

	public void SetNextBattleStep()
	{
		if (_battleStepInfos.Count <= _curStepPosIndex + 1)
			return;
        List<BattleDefinitions.BattleStep> stepKeyValues = _battleStepInfos.Keys.ToList ();

        SetStepProgress(stepKeyValues);
        //LoadTimeProfiler.Stop(_reserveBattleStep.ToString());
		_curStepPosIndex++;
		_reserveBattleStep = _battleStepInfos [stepKeyValues[_curStepPosIndex]].BattleStep;

        //LoadTimeProfiler.Start(_reserveBattleStep.ToString());
	}

    public void SetStepProgress(List<BattleDefinitions.BattleStep> stepKeyValues)
    {
        if ((int)stepKeyValues[_curStepPosIndex] <= (int)BattleDefinitions.BattleStep.TutorialBattleStart)
        {
            if (stepPrograssCount < stepPrograssTotalCount)
            {
                stepPrograssCount += 1.0f;
                _battleStartStepActions.FinishProgressBattleStep(stepKeyValues[_curStepPosIndex], (float)stepPrograssCount / stepPrograssTotalCount);
            }
        }
        else
        {
            stepPrograssCount = 0;
        }
    }
        
	public void SetBattleStepStart(BattleDefinitions.BattleStep step)
	{
		_curBattleStep = step;
		_battleStepInfos [step].OnStartStep ();
	}

	#endregion
}
