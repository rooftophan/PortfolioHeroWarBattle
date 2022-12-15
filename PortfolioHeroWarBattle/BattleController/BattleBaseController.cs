using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Controller;
using Framework.Controller;
using System.Linq;
using System;

namespace NewBattleCore
{
	public class BattleBaseController : MonoBehaviour, IBattleableContext, IBattleEndable, IBattleStartableStep
    {
		#region Variables

		protected UserModel _userModel;
		protected TextModel _textModel;

		protected TopMenuController _topMenuController = null;

		protected GameSystem System = null;
		protected BattleStepManager _stepManager = null;
        protected BattleDirectingManager _directingManager = null;

		protected bool[] _deployMoveValues = new bool[4];

		protected List<int[]> _unitFuryGauges = new List<int[]> ();

		protected BattleResultType _curResultType;
		protected IView _resultHandler;
		protected UserAssetContainer _assetContainerModel;

        private ItemDrawer _itemDrawer = null;

        protected bool _isDeployCamPlayed = false;

        protected bool _isBattleEnded = false;

        #endregion

        #region Properties

        public BattleScene Scene { get; protected set; }

        public ItemDrawer ItemDrawer
        {
            get
            {
                if(_itemDrawer == null)
                    _itemDrawer = new ItemDrawer(System.Data);
                
                return _itemDrawer;
            }
        }

		#endregion

		#region MonoBehaviour Methods
               

		public virtual void Awake()
		{
			System = GameSystem.Instance;
			BattleManager.Instance.battle.battleEndRule.AttachBattleEndable ((IBattleEndable)this);

			if (ChattingController.Instance != null) {
				ChattingController.Instance.IsBattleState = true;
                UITopDepthNoticeMessage.Instance.PartyNotifyPopupManager.IsBattleState = true;
                ChattingController.Instance.UIMultiChat.CompletePreviewTextTimer ();
			}
		}

		public virtual void Start()
        {

        }

		public virtual void Update()
		{
			if (_stepManager == null)
				return;

			if (_stepManager.ReserveBattleStep != _stepManager.CurBattleStep) {
				_stepManager.SetBattleStepStart (_stepManager.ReserveBattleStep);
			}

            switch (_stepManager.CurBattleStep)
            {
                case BattleDefinitions.BattleStep.Directing:
                case BattleDefinitions.BattleStep.Deploy:
                case BattleDefinitions.BattleStep.Battling:
                case BattleDefinitions.BattleStep.ChangeWave:
                case BattleDefinitions.BattleStep.BattleEndDirecting:
                    if (Scene != null)
                        Scene.Update();

                    _directingManager.Update();
                    break;
            }
		
		}

		public virtual void OnDestroy()
		{
			if(!BattleManager.isRetryGame)
				BattleManager.emptyBattleTransitionController = null;

			if (ChattingController.Instance != null) {
                ChattingController.Instance.IsBattleState = false;
                UITopDepthNoticeMessage.Instance.PartyNotifyPopupManager.IsBattleState = false;
                ChattingController.Instance.UIMultiChat.CompletePreviewTextTimer ();

                if (ChattingController.Instance.ChatSocketManager.ChatCurState == ChattingSocketManager.ChatCurrentState.Connected) {
                    ChattingController.Instance.RootNudgeNode.RefreshNodeNudgeCount();
                }
            }
		}


        public bool IsDirecting()
        {
            if( _directingManager == null || _stepManager == null )
                return false;
            switch( _stepManager.CurBattleStep )
            {
            case BattleDefinitions.BattleStep.Directing:
            case BattleDefinitions.BattleStep.Deploy:
            case BattleDefinitions.BattleStep.Battling:
            case BattleDefinitions.BattleStep.ChangeWave:
            case BattleDefinitions.BattleStep.BattleEndDirecting:

                return _directingManager.IsDirecting();

            }
            return false;
        }


        public void FinishProgressBattleStep(BattleDefinitions.BattleStep step, float value)
        {
            if (BattleManager.Instance.battleType == BattleType.SkillTool ||
               BattleManager.Instance.battleType == BattleType.BattleCoreSceneTest)
                return;
            if ((int)step >= (int)BattleDefinitions.BattleStep.TutorialBattleStart)
                value = 1.0f;

            if (UIHelpScreenUGUI.Instance != null)
            {
                Debug.Log("<color=yellow> Finish Loading Step : " + step.ToString() + "</color>");

                UIHelpScreenUGUI.Instance.SetProgress(0.3f + ( value * 0.7f )); 
            }
        }

        public void HandlerInitUnitLoadProgressStep()
        {
            if (BattleManager.Instance.battleType == BattleType.SkillTool ||
                BattleManager.Instance.battleType == BattleType.BattleCoreSceneTest)
                return;
            
            if( BattleStepManager.stepPrograssCount < BattleStepManager.stepPrograssTotalCount )
            {
                BattleStepManager.stepPrograssCount += 1.0f;
                float _value = BattleStepManager.stepPrograssCount / BattleStepManager.stepPrograssTotalCount;
                if( UIHelpScreenUGUI.Instance != null )
                    UIHelpScreenUGUI.Instance.SetProgress(0.3f + (_value * 0.7f));
            }
        }

        #endregion
       	
        #region Init

        public virtual void Init()
        {
			if(System == null)
				System = GameSystem.Instance;

#if UNITY_EDITOR
			if(System.Data == null)
            {
                if (System.textDataSheet == null)
                {
                    System.textDataSheet = new TextDataSheet();
                    System.textDataSheet.RegistrationSheet();
                }
                    
                System.Data = new DataContext(new DataSheet(), System.textDataSheet);
            }
                
#endif

            _userModel = System.Data.User;
			_textModel = System.Data.Text;
			_assetContainerModel = System.Data.Currency;

			if (System.Sheet == null)
				return;

            InitStepManager();

            //UITopDepthNoticeMessage.Instance.PartyTopNoticeInfo.CloseOnly();
            //            if (NotificationWidgetController.Instance != null)
            //            {
            //	            NotificationWidgetController.Instance?.ShowQuestNotiWidget(false);
            //            }

        }

        protected virtual void InitStepManager()
        {
            _directingManager = new BattleDirectingManager();
            _stepManager = new BattleStepManager();

            _stepManager.BattleStartStepActions = this;
            _stepManager.DirectingManager = _directingManager;

            InitBattleStep();

            _stepManager.StartFirstBattleStep();
        }

        protected virtual void InitBattleStep()
        {
            _stepManager.InitDefaultStep();
        }

        #endregion

        #region Step : SceneLoad
		
        public virtual void SetSceneLoad()
        {
            _stepManager.LoadBattleDirectingObject(GetBattleDirectingPath());

            NewBattleCore.BattleManager.Instance.onActionAfterSetTroop = OnActionAfterSetTroop;

            OnBuildBattle();

        }

        protected virtual void OnBuildBattle()
        {
            Scene.onFinishLoadStage = OnFinishSceneLoadStep;
			Scene.onFinishChangeWaveLoadStage = OnFinishChangeWaveLoadStep;
            Scene.onReserveNextBattleStep = OnReserveNextBattleStep;

            ResourceLoader.ClearAsset();
        }

        protected virtual void OnFinishSceneLoadStep()
        {
            SetCurEnableData();
            _stepManager.SetNextBattleStep();
        }

        protected virtual void SetCurEnableData() { }

        #endregion

		#region Step : BattleMapLoad

		public virtual void SetBattleMapLoad()
		{
			SetMapEventObj ();
			BattleManager.Instance.battleMap.InitMapEventObject ();
			BattleManager.Instance.battleMap.InitMapEvent ();

			_stepManager.SetNextBattleStep();
		}

		public virtual void SetMapEventObj()
		{
            BattleMap map = BattleManager.Instance.battleMap;
			int waveCount = TroopDeployInfoManager.Instance.FormaionMapInfo.WaveFormationInfos.Count;
			map.MapEventObjects = new GameObject[waveCount];
			for (int i = 0; i < waveCount; i++) {
				WaveFormationInfo waveFormation = TroopDeployInfoManager.Instance.FormaionMapInfo.WaveFormationInfos [i];
				if (string.IsNullOrEmpty (waveFormation.BattleEventPath)) {
					map.MapEventObjects [i] = null;
				} else {
					map.MapEventObjects [i] = ResourceLoader.Instantiate( BattleConfig.MapEventPath + waveFormation.BattleEventPath );
				}
			}
		}

		#endregion

		#region Step : HandlerInitUnitLoad

		public virtual void SetHandlerInitUnitLoad()
		{
            int waveIndex = 0;


            if( BattleManager.replayMode == BattleReplayMode.Play )
            {
                if( BattleManager.Instance.battleType == BattleType.SkillTool  )
                {
                    Debug.LogError("Cant' replay on skill tool mode.");
                }
                else
                {
                    UnitDecorateAndBattleInit();

                    OnCompleteEnemyExtraTroop();
                }
                
                return;
            }

            
             
            if( BattleManager.Instance.battleType == BattleType.SkillTool || 
                BattleManager.Instance.battleType == BattleType.BattleCoreSceneTest )
            {   
                TroopFactory.Build(Scene.Info.Context, NewBattleCore.TroopRelation.Ally, waveIndex, TroopDeployInfoManager.Instance.UserDeckData.UserBattleDecks.Values.ToArray(), false);
                UnitDecorateAndBattleInit();

                Scene.Wave.SetAddAllyTroopInfo();
                Scene.Wave.SetEnemyExtraTroopInfo();

                Scene.BCHandler.Init();

                // Enemy Troop & Unit Set Position
                Scene.Wave.Execute();

                _stepManager.SetNextBattleStep(); 
            }
            else
            {
                if (TroopDeployInfoManager.isAllyWave) {
                    TroopDeployInfoManager.Instance.CurWaveIndex = 0;
                }
                TroopFactory.Build (Scene.Info.Context, NewBattleCore.TroopRelation.Ally, waveIndex, TroopDeployInfoManager.Instance.UserDeckData.GetCurUserBattleDecks().Values.ToArray(), false, ()=>
                {
                    UnitDecorateAndBattleInit();

                    Scene.Wave.SetAddAllyTroopInfo(() => OnCompleteAddAllyTroop());
                });
            }
          
		}

        void UnitDecorateAndBattleInit()
        {
            foreach( var hero in BattleManager.Instance.GetAllyUnits(0))
            {
                UnitFactory.Decorate( hero, false );
            }

            if( BattleManager.replayMode == BattleReplayMode.Play )
            {
                if( BattleManager.Instance.battleType != BattleType.SkillTool &&
                   BattleManager.Instance.battleType != BattleType.BattleCoreSceneTest &&
                   BattleManager.Instance.battleType != BattleType.NewTest &&
                   BattleManager.Instance.battleType != BattleType.Test )
                {
                    foreach( var hero in BattleManager.Instance.waveUnits[0].enemies )
                    {
                        UnitFactory.Decorate( hero, false );
                    }

                    foreach( var hero in BattleManager.Instance.waveUnits[0].extras )
                    {
                        UnitFactory.Decorate( hero, false );
                    }

                }
            }
            EventSystem eventSystem = GameSystem.Instance.EventSystem;

            if (eventSystem != null && !eventSystem.isActiveAndEnabled)
                eventSystem.enabled = true;

            var input = InteractionInput.Instance;

            Scene.BCHandler.input = input;

            int BuiltInUILayer = 5;

            input.camera2d = Scene.UI.Camera;
            input.camera3d = BattleManager.Instance.battleCamera.mainCamera;
            input.layer2d = BuiltInUILayer;
            input.layer3d = LayerMask.GetMask("CH");

        }

        public virtual void SetAllyHeroesHPValue()
        {

        }

        public virtual void OnCompleteAddAllyTroop()
        {
            Scene.Wave.SetEnemyExtraTroopInfo (()=> OnCompleteEnemyExtraTroop());
        }

        public virtual void OnCompleteEnemyExtraTroop()
        {
            Scene.BCHandler.Init();

            // Enemy Troop & Unit Set Position
            Scene.Wave.Execute ();

            _stepManager.SetNextBattleStep();
        }

		#endregion

        #region Step : BattleDataSetting

        public virtual void SetBattleDataSetting()
        {
            SetMapEventHandler();
            SetSceneLoadedData();

            new DoBattle(this, false).Execute();

            SetCustomBattleDataSetting();

            _stepManager.SetNextBattleStep();

            if(ChattingController.Instance != null && ChattingController.Instance.ChatSocketManager.ChatCurState == ChattingSocketManager.ChatCurrentState.Connected) {
                ChattingController.Instance.RootNudgeNode.RefreshNodeNudgeCount();
            }
        }

        protected virtual void SetMapEventHandler()
        {
            Scene.BCHandler.OnReady = () =>
            {
                    
            };

            Scene.BCHandler.OnUmpired = () =>
            {

            };
            BattleMap map = BattleManager.Instance.battleMap;
            if (map.mapEventHandlers == null)
                return;
            foreach (var mapEventHandler in map.mapEventHandlers)
            {
                if (mapEventHandler != null)
                    mapEventHandler.PreLoadSpawnUnits();
            }
        }

        protected virtual void SetSceneLoadedData()
        {
            _topMenuController = System.GetService<TopMenuController>();

            if (_topMenuController != null)
                _topMenuController.Disable();
        }

        protected virtual void SetCustomBattleDataSetting() { }

        #endregion

        #region Step : Directing
		
        public virtual void SetDirecting()
        {
            InitDirtecting();
            InitStartDirecting(GetDirectingStartInfos());
        }

        protected void InitDirtecting()
        {
			if(_directingManager == null)
				_directingManager = new BattleDirectingManager();

			if (NewBattleCore.BattleManager.emptyBattleTransitionController == null) {
				Debug.Log (string.Format ("InitDirtecting NewBattleCore.BattleManager.emptyBattleTransitionController == null"));
			}

			if (NewBattleCore.BattleManager.emptyBattleTransitionController != null)
				NewBattleCore.BattleManager.emptyBattleTransitionController.Scene = Scene;

            InitDeployMoveValues();
        }

        protected virtual void InitStartDirecting(List<BattleDirectingStepInfo.BattleDirectingInfo> inputDirectingWaveInfos = null)
		{
            var isDirecting = IsStartDirectingEnable(inputDirectingWaveInfos);
            _directingManager.Clear();

            if (isDirecting)
                _directingManager.Enqueue(new Directing_HideUnit());

            _directingManager.Enqueue(new Directing_BattleDirecting());
            _directingManager.Enqueue(new Directing_HideBattleUI());

            bool _isShowUnits = false;

            if (isDirecting)
            {
                for (int i = 0; i < inputDirectingWaveInfos.Count; i++)
                {
                    if (!inputDirectingWaveInfos[i].IsActive) continue;

                    if (inputDirectingWaveInfos[i].DirectingType == BattleDirectingType.InGameDirectingTimeLineCutScene &&
                        !BattleManager.ShowBossDirectingAlways())
                        continue;

                    CurrentBattleDirectingWaveInfo waveStartDirecting = BattleDirectingManager.GetCurrentBattleDirectingWaveInfo(inputDirectingWaveInfos[i]);
                    switch (inputDirectingWaveInfos[i].DirectingType)
                    {
                        case BattleDirectingType.NormalReactor:
                            {
                                _directingManager.Enqueue(new Directing_FadeOutIn(null, 0f, 1.5f, new Directing_HideLoadingImage().Execute, OnFinishDirectingFadeOut), true);
                                _directingManager.Enqueue(new Directing_ShowUnit());
                                _directingManager.Enqueue(new Directing_WaveStartDirecting(null, waveStartDirecting));
                            }
                            break;
                        case BattleDirectingType.ArenaTimelineCutScene:
                            {                            
                                _directingManager.Enqueue(new Directing_FadeOutIn(null, 0f, 1.5f, new Directing_HideLoadingImage().Execute, OnFinishDirectingFadeOut), true);                                
                                _directingManager.Enqueue(new Directing_ShowUnit());
                                _directingManager.Enqueue(new Directing_CutSceneArena(null, waveStartDirecting));
                                BattleManager.Instance.battleCamera.SelectRandomDeployAni();
                                EnqueueDeployCam();
                                _isShowUnits = true;
                            }
                            break;

                        case BattleDirectingType.InGameDirectingTimeLineCutScene:
                            {
                                _directingManager.Enqueue(new Directing_FadeOutIn(null, 0f, 1.5f, new Directing_HideLoadingImage().Execute, OnFinishDirectingFadeOut), true);                                
                                _directingManager.Enqueue(new Directing_ShowUnit());

                                _directingManager.Enqueue(new Directing_CutSceneInGame(null, waveStartDirecting));

                                BattleManager.Instance.battleCamera.SelectRandomDeployAni();
                                EnqueueDeployCam();
                                _isShowUnits = true;
                            }
                            break;
                    }
                }
            }

            if (!_isShowUnits)
                _directingManager.Enqueue(new Directing_ShowUnit());

            _directingManager.Enqueue(new Directing_BattleStart());

            if (!isDirecting)
            {
                _directingManager.Enqueue(new Directing_Fade(OnFinishDirectingFadeOut, false, 0f));
                _directingManager.Enqueue(new Directing_HideLoadingImage());
                BattleManager.Instance.battleCamera.SelectRandomDeployAni();
                EnqueueDeployCamReady();
                _directingManager.Enqueue(new Directing_Fade(null, true));
                EnqueueDeployCam();
            }

            _directingManager.Enqueue(new Directing_CallBack(null, () => OnFinishedCurStep(_stepManager.CurBattleStep)));
        }

        protected void OnFinishDirectingFadeOut()
        {
            if (BattleManager.Instance.battleCamera.DeployCamPlayable() == false) {
                BattleManager.Instance.battleCamera.WaveChanged();
            }
           
        }

        protected void EnqueueDeployCamReady()
        {
            if( BattleManager.Instance.battleCamera.DeployCamPlayable() == true )
            {
                _directingManager.Enqueue( new Directing_DeployCamReady( Scene ) );
                _isDeployCamPlayed = true;
            }
        }

        protected void EnqueueDeployCam()
        {
            if( BattleManager.Instance.battleCamera.DeployCamPlayable() == true )
            {
                _directingManager.Enqueue( new Directing_DeployCam( Scene ) );
                float waitTime = BattleManager.Instance.battleCamera.GetStartDeployWalkingTime();
                _directingManager.Enqueue( new Directing_Wait(null, waitTime ) );
                _isDeployCamPlayed = true;
            }
        }

        protected virtual bool IsStartDirectingEnable(List<BattleDirectingStepInfo.BattleDirectingInfo> inputDirectingWaveInfos = null)
        {
            if (inputDirectingWaveInfos == null)
                return false;

            for (int i = 0; i < inputDirectingWaveInfos.Count; i++)
            {
                if (inputDirectingWaveInfos[i].IsActive) {
                    if (inputDirectingWaveInfos[i].DirectingType != BattleDirectingType.InGameDirectingTimeLineCutScene) {
                        return true;
                    } else {
                        if (BattleManager.ShowBossDirectingAlways())
                            return true;
                    }
                        
                }
            }

            return false;
        }

        protected virtual void OnSetChangeWaveData()
        {
			BattleHandler.Instance.ClearWave ();

			Scene.ChangeWaveMapInfo ();
        }

        protected virtual void OnResetWaveCameraPos()
        {
            BattleManager.Instance.battleCamera.WaveChanged();
        }

        protected virtual void OnFinishChangeWaveLoadStep(bool isChangeMap)
		{
			if (isChangeMap) {
				SetMapEventObj ();                
				BattleManager.Instance.battleMap.InitMapEventObject ();
			}

			BattleManager.Instance.battleMap.ResetNewWaveMapEvent(BattleHandler.Instance.CurrentWaveIndex + 1);

			BattleHandler.Instance.OnWave();
			UnitManager.Instance.InitWave();
			UnitManager.Instance.FillFuryGaugeByNextWave();

			OnFinishedCurStep (_stepManager.CurBattleStep);
        }

        protected virtual void OnReserveNextBattleStep(BattleDefinitions.BattleStep battleStep)
        {
            _stepManager.SetReserveBattleStep(battleStep);
        }

        #endregion

        #region Step : Deploy
		
        public virtual void SetDeploy()
        {
            InitStartDeploy();
            InitDeployMoveValues();

            _isBattleEnded = false;
        }

        protected virtual void InitStartDeploy()
        {
            _directingManager.Clear();
            SetChangeWaveDeploy();
            _directingManager.Enqueue(new Directing_CallBack(null, () => OnFinishedCurStep(_stepManager.CurBattleStep)));
        }

        protected void SetDeployMoveValues(bool moveAlly, bool moveAllyMinion, bool moveEnemy, bool moveEnemyMinion)
        {
            _deployMoveValues[0] = moveAlly;
            _deployMoveValues[1] = moveAllyMinion;
            _deployMoveValues[2] = moveEnemy;
            _deployMoveValues[3] = moveEnemyMinion;
        }

        protected void InitDeployMoveValues()
        {
            _deployMoveValues[0] = true;
            _deployMoveValues[1] = true;
            _deployMoveValues[2] = true;
            _deployMoveValues[3] = true;
        }

        protected virtual bool isEnableDeploy()
        {
            return false;
        }

        #endregion

        #region Step : Battling

        public virtual void SetBattling()
        {
			BattleHandler.Instance.onCheatForceQuit = OnBattleForceQuit;

            _directingManager.Clear();
        }

        #endregion

        #region BattleEnd : Call By BattleEndRule
		
        public virtual void BattleEnd(BattleResultType resultType)
        {
            if (resultType == BattleResultType.NextWave)
			{
                if (_stepManager != null)
				{
                    _stepManager.SetReserveBattleStep(BattleDefinitions.BattleStep.ChangeWave);
                    _stepManager.SetBattleStepStart(BattleDefinitions.BattleStep.ChangeWave);
                }
            }
            else if (resultType != BattleResultType.NextWave)
			{
                _isBattleEnded = true;
                _curResultType = resultType;
                if (_stepManager != null)
				{
					BattleEndVibrate();
                    _stepManager.SetReserveBattleStep(BattleDefinitions.BattleStep.BattleEndDirecting);
                }
            }
        }

        protected virtual void OnHeroInfo()
        {
            BattleScene.Clear();
	        new ShortcutHelper(ShortcutType.HeroInfo).Execute();
		    NotificationWidgetController.Instance?.ShowQuestNotiWidget(true);
        }

        protected virtual void OnMainLobby()
        {
            BattleScene.Clear();
            new ShortcutHelper(ShortcutType.Main).Execute();
	        NotificationWidgetController.Instance?.ShowQuestNotiWidget(true);

            if( MissionManager.Instance != null )
                MissionManager.Instance.SetCurrentMissionContentType(MissionContentType.None);
        }
        
        protected virtual void OnShortCut(ShortcutType shortcutType)
        {
            BattleScene.Clear();
	        new ShortcutHelper(shortcutType).Execute();
	        NotificationWidgetController.Instance?.ShowQuestNotiWidget(true);
        }
        
		protected virtual void OnBattleQuit()
		{
			BattleManager.isRetryGame = false;
			ReleaseBattleData();
			QuitBattleTransition ();
		}

		protected virtual void OnBattleDirectQuit()
		{
			BattleManager.isRetryGame = false;
			ReleaseBattleData();
			QuitBattleDirectTransition ();
		}

		protected virtual void OnBattleForceQuit()
		{
			BattleManager.isRetryGame = false;
			ReleaseBattleData();
			ResourceLoader.UnLoadSceneName(Scene, (x) => QuitBattleTransition(), Scene.Wave.Current);
		}

        protected virtual void QuitBattleTransition()
        {
            new Controller.UnloadScene(System.transform, SceneTag.BattleCore).Execute();

			if (BattleManager.emptyBattleTransitionController != null)
            {
				BattleManager.emptyBattleTransitionController.Terminate();
				BattleManager.emptyBattleTransitionController.Transition.Method = Framework.TransitionMethod.Backward;

                Action endAction = BattleManager.emptyBattleTransitionController.Kill;
                endAction += OnCompleteQuitTransit;

                BattleManager.emptyBattleTransitionController.Transit(BattleManager.emptyBattleTransitionController.Transition, endAction);
            }
			
			NotificationWidgetController.Instance?.ShowQuestNotiWidget(true);
			
            ResourceLoader.Clear();
        }

		private void QuitBattleDirectTransition()
		{
			new Controller.UnloadScene(System.transform, SceneTag.BattleCore).Execute();

			NotificationWidgetController.Instance?.ShowQuestNotiWidget(true);
		
			ResourceLoader.Clear();
		}

        protected virtual void OnCompleteQuitTransit()
        {

        }

        protected virtual void ReleaseBattleData()
        {
	        if (BattleHandler.Instance != null)
		        BattleHandler.Instance.scene.Dispose();
	        
            if (_resultHandler != null)
	            _resultHandler.Dispose();
        }

        protected void BattleEndVibrate()
        {
	        if (System.Data.LocalSaveContainer.IsBattleEndVibration)
	        {
		        if (TapticEnginePlugin.Feedback.IsSupport)
		        {
			        TapticEnginePlugin.Feedback.Notification(TapticEnginePlugin.NotificationFeedback.Success);
			        TapticEnginePlugin.Feedback.Impact(TapticEnginePlugin.ImpactFeedback.Light);
			        TapticEnginePlugin.Feedback.Selection();
		        }
		        else
		        {
			        Handheld.Vibrate();
		        }
	        }
        }
        #endregion

        #region Step : ChangeWaveNetRequest
        public virtual void SetChangeWaveNetRequest()
        {
            _stepManager.SetReserveBattleStep(BattleDefinitions.BattleStep.ChangeWave);
            _stepManager.SetBattleStepStart(BattleDefinitions.BattleStep.ChangeWave);
        }

        #endregion

        #region Step : ChangeWave

        public virtual void SetChangeWave()
        {
	        SetChangeWaveDirecting(GetDirectingPreChangeWaveInfos(Scene.Wave.Current + 1), GetDirectingAfterChangeWaveInfos(Scene.Wave.Current + 1), 
		        OnPreDirectingComplete,
		        OnAfterDirectingComplete);
            InitDeployMoveValues();
        }


        protected virtual void SetChangeWaveDirecting(List<BattleDirectingStepInfo.BattleDirectingInfo> preChangeDirecting = null, 
                                                      List<BattleDirectingStepInfo.BattleDirectingInfo> afterDirectingInfo = null,
                                                      System.Action onPreDirectingComplete = null,
                                                      System.Action onAfterDirectingComplete = null)
        {
            _directingManager.Clear();
            SetChangeWaveStart(preChangeDirecting, afterDirectingInfo, onPreDirectingComplete, onAfterDirectingComplete);
            SetChangeWaveDeploy();
        }

        protected virtual void SetChangeWaveStart(List<BattleDirectingStepInfo.BattleDirectingInfo> preChangeDirecting = null, 
                                                  List<BattleDirectingStepInfo.BattleDirectingInfo> afterDirectingInfo = null,
                                                  System.Action onPreDirectingComplete = null,
                                                  System.Action onAfterDirectingComplete = null)
        {
            bool isPreDirecting = IsStartDirectingEnable(preChangeDirecting);
            if (isPreDirecting) {
                _directingManager.EnqueueCutScene(preChangeDirecting, null, null, onPreDirectingComplete);
            }

            _directingManager.Enqueue(new Directing_WaveChangeDirectingPre());

            _directingManager.Enqueue(new Directing_CallBack(null, OnSetChangeWaveData));

            _directingManager.Enqueue(new Directing_CallBack(null, OnResetWaveCameraPos));

            _directingManager.Enqueue(new Directing_BattleDirecting());

            bool isAfterDirecting = IsStartDirectingEnable(afterDirectingInfo);

            if (isAfterDirecting)
            {
	            _directingManager.Enqueue(new Directing_WaitCondition(null, () => BattleManager.Instance.completeLoadEnemy));
	            _directingManager.EnqueueAfterCutScene(afterDirectingInfo, null, null, onAfterDirectingComplete);
            }
            else
            {
	            _directingManager.Enqueue(new Directing_WaveChangeDirectingAfter());
            }
        }

        protected virtual void SetChangeWaveDeploy()
        {
	        if (isEnableDeploy())
                _directingManager.Enqueue(new Directing_BattleDeploy(null, _deployMoveValues[0], _deployMoveValues[1], _deployMoveValues[2], _deployMoveValues[3]));

            _directingManager.Enqueue(new Directing_CreateUIRootNode());
            _directingManager.Enqueue(new Directing_BattleStop());

            _directingManager.Enqueue(new Directing_InstantiateUINode(null, Res.PREFAB.UIBattleStartUGUI, Vector3.zero));
            _directingManager.Enqueue(new Directing_BattleStartText());

            _directingManager.Enqueue(new Directing_Wait(null, 1f));

            _directingManager.Enqueue(new Directing_RemoveUIRootNode());
            _directingManager.Enqueue(new Directing_BattleReady());
            _directingManager.Enqueue(new Directing_ShowBattleUI());
        }

        #endregion

        #region Step : ChangeWaveUnloadScene

        public virtual void SetChangeWaveUnloadScene()
        {
            Debug.Log(string.Format("SetChangeWaveUnloadScene"));
            Scene.ChangeWaveUnloadBattleScene();
        }

        #endregion

        #region Step : ChangeWaveLoadScene

        public virtual void SetChangeWaveLoadScene()
        {
            Debug.Log(string.Format("SetChangeWaveLoadScene"));
            Scene.ChangeWaveLoadBattleScene();
        }

        #endregion

        #region Step : BattleEndDirecting

        public virtual void SetBattleEndDirecting()
        {
            List<BattleDirectingStepInfo.BattleDirectingInfo> battleEndDirecting = GetEndDirectingInfos();

            if (battleEndDirecting != null && _curResultType != BattleResultType.Lose)
            {
                InitEndDirecting(battleEndDirecting);
            }
            else
            {
				_stepManager.SetReserveBattleStep(BattleDefinitions.BattleStep.BattleEndStoryScript);
            }
        }


        protected virtual void InitEndDirecting(List<BattleDirectingStepInfo.BattleDirectingInfo> inputDirectingWaveInfos = null)
        {
            _directingManager.Clear();
            bool isDirectingState = IsEndDirectingEnable(inputDirectingWaveInfos);

            _directingManager.Enqueue(new Directing_BattleDirecting());
            _directingManager.Enqueue(new Directing_HideBattleUI());
            
            if (isDirectingState)
                _directingManager.EnqueueCutScene(inputDirectingWaveInfos, OnBattleEndStartDirecting, OnBattleEndDirectingEndType);

            _directingManager.Enqueue(new Directing_CallBack(null, () => OnFinishedCurStep(_stepManager.CurBattleStep)));
        }

        protected virtual bool IsEndDirectingEnable(List<BattleDirectingStepInfo.BattleDirectingInfo> inputDirectingWaveInfos = null)
        {
            if (inputDirectingWaveInfos == null)
                return false;

            for(int i = 0;i< inputDirectingWaveInfos.Count;i++) {
                if(inputDirectingWaveInfos[i].IsActive)
                    return true;
            }

            return false;
        }

        #endregion

		#region Step : BattleEndStoryScript

		public virtual void SetBattleEndStoryScript()
		{
			_stepManager.SetReserveBattleStep (BattleDefinitions.BattleStep.NetRequestBattleEnd);
		}

		#endregion
	
        #region Step : NetRequestBattleEnd

        public virtual void SetNetRequestBattleEnd()
        {
            if(ChattingController.Instance != null) {
                if(ChattingController.Instance.IsChattingPopup) {
                    ChattingController.Instance.UIMultiChat.CloseChattingPopup();
                }
            }
        }

        protected virtual void OnBattleEndComplete(ResponseParam resParam)
        {
            _stepManager.SetReserveBattleStep(BattleDefinitions.BattleStep.BattleEnded);
        }

        #endregion

        #region Step : BattleEnded

        public virtual void SetBattleEnded()
        {
            if( BattleManager.Instance != null && BattleManager.Instance.battleCamera != null )
            {
                BattleManager.Instance.battleCamera.BackgroundRemoved();
            }
            
            if( Scene.changeObj != null )
            {
                Scene.changeObj.GetComponent<VirtualEnvDirecting>().gameObject.SetActive(false);
            }

			ResourceLoader.UnLoadSceneName(Scene, null, Scene.Wave.Current);
            SetResultController();
        }

        public virtual void ForceQuitBattle()
        {
            Scene.System.AudioController.BgmAudioController.Stop();
            if( BattleManager.Instance != null && BattleManager.Instance.battleCamera != null )
            {
                BattleManager.Instance.battleCamera.BackgroundRemoved();
            }
            
            if( Scene.changeObj != null )
            {
                Scene.changeObj.GetComponent<VirtualEnvDirecting>().gameObject.SetActive(false);
            }

			ResourceLoader.UnLoadSceneName(Scene, null, Scene.Wave.Current);
	        OnBattleQuit();
	
	        BattleManager.isRetryGame = false; 
        }

        protected virtual void SetResultController()
        {
			BattleHandler.Instance.SetPreBattleResultData();
        }
        
        #endregion

		#region Step : TutorialBattleStart

		public virtual void SetTutorialBattleStart()
		{
            _stepManager.SetNextBattleStep ();
		}

        #endregion

        #region GetDirectingData

        protected List<BattleDirectingStepInfo.BattleDirectingInfo> GetDirectingStartInfos()
        {
            List<BattleDirectingStepInfo.BattleDirectingInfo> directingWaveInfos = null;
            if (_stepManager.CurBattleDirectingObject != null) {
                directingWaveInfos = _stepManager.CurBattleDirectingObject.GetBattleStartDirectingInfos();
            }

            return directingWaveInfos;
        }

        protected List<BattleDirectingStepInfo.BattleDirectingInfo> GetDirectingPreChangeWaveInfos(int waveIndex)
        {
            List<BattleDirectingStepInfo.BattleDirectingInfo> directingWaveInfos = null;

            if (_stepManager.CurBattleDirectingObject != null) {
                directingWaveInfos = _stepManager.CurBattleDirectingObject.GetChangeWavePreDirectingInfos(waveIndex);
            }

            return directingWaveInfos;
        }

        protected List<BattleDirectingStepInfo.BattleDirectingInfo> GetDirectingAfterChangeWaveInfos(int waveIndex)
        {
            List<BattleDirectingStepInfo.BattleDirectingInfo> directingWaveInfos = null;

            if (_stepManager.CurBattleDirectingObject != null) {
                directingWaveInfos = _stepManager.CurBattleDirectingObject.GetChangeWaveAfterDirectingInfos(waveIndex);
            }

            return directingWaveInfos;
        }

        protected List<BattleDirectingStepInfo.BattleDirectingInfo> GetEndDirectingInfos()
        {
            List<BattleDirectingStepInfo.BattleDirectingInfo> directingWaveInfos = null;
            if (_stepManager.CurBattleDirectingObject != null) {
                directingWaveInfos = _stepManager.CurBattleDirectingObject.GetBattleEndDirectingInfos();
            }

            return directingWaveInfos;
        }

        protected List<BattleDirectingStepInfo.BattleDirectingInfo> GetBattleEndDirectingInfos(int waveIndex)
		{
			return null;
		}

		protected virtual string GetBattleDirectingPath()
		{
			return "";
		}

		#endregion

		#region CallBack Methods

        protected virtual void OnFinishedCurStep(BattleDefinitions.BattleStep battleStep)
		{
			if (battleStep == BattleDefinitions.BattleStep.ChangeWave ||
                battleStep == BattleDefinitions.BattleStep.ChangeWaveLoadScene) {
                _stepManager.ReserveBattleStep = BattleDefinitions.BattleStep.Battling;
				_stepManager.CurBattleStep = BattleDefinitions.BattleStep.Battling;
                
			} else if(battleStep == BattleDefinitions.BattleStep.BattleEndDirecting) {
				_stepManager.SetReserveBattleStep (BattleDefinitions.BattleStep.BattleEndStoryScript);
            } else if (battleStep == BattleDefinitions.BattleStep.Directing) {
                _stepManager.SetNextBattleStep();
            } else if (battleStep == BattleDefinitions.BattleStep.Deploy) {
                SetAllyHeroesHPValue();
                _stepManager.SetNextBattleStep();
            } else {
                _stepManager.SetNextBattleStep();
            }
		}

        protected virtual void OnActionAfterSetTroop()
		{
			List<Unit> curEnemyLiveUnits = UnitManager.Instance.LiveUnits(TroopRelation.Enemy);

			if (_unitFuryGauges.Count > Scene.Wave.Current) {
				int[] curFuryGauge = _unitFuryGauges [Scene.Wave.Current];
				if (curFuryGauge.Length == 1) {
					for (int i = 0; i < curEnemyLiveUnits.Count; i++) {
						curEnemyLiveUnits [i].stat [(int)BattleStat.FuryGauge] = (float)curFuryGauge [0];
					}
				} else if(curFuryGauge.Length > 1) {
					for (int i = 0; i < curEnemyLiveUnits.Count; i++) {
						if (curFuryGauge.Length <= i)
							break;
						
						curEnemyLiveUnits [i].stat [(int)BattleStat.FuryGauge] = (float)curFuryGauge [i];
					}
				}
			}
		}

        protected virtual void OnSceneDispose()
        {
            Scene.Dispose();
        }

		protected virtual void OnRetryGameButton(IView handler)
		{
			_resultHandler = handler;

			SetBattleReadyController ();
		}

		protected virtual void OnPrevButton(IView handler)
		{
			_resultHandler = handler;

			SetBattleReadyController ();
			
		}

		protected virtual void OnNextButton(IView handler)
		{
			_resultHandler = handler;

			SetBattleReadyController ();
		}

		void OnUnloadSceneRetryGame()
		{
			new Controller.UnloadScene(System.transform, SceneTag.BattleCore).Execute();
		}

		protected virtual void OnStartBattleSuccess(ResponseParam resParam)
		{
			RestartBattle (_resultHandler);
		}

		protected virtual void OnBattleResultStart()
		{
			RestartBattle (_resultHandler);
		}

		protected virtual void OnClosedBattleReadyController()
		{
			if (_topMenuController != null) {
				_topMenuController.Disable ();
                _topMenuController.OnBattleReleaseAction = null;
                _topMenuController = null;
            }
		}

		void SetEventCount()
		{
			while (true) {
				if (GameSystem.Instance.EventSystemLockCount > 0) {
					GameSystem.Instance.DecreaseEventSystemLock ();
				} else if (GameSystem.Instance.EventSystemLockCount < 0) {
					GameSystem.Instance.IncreaseEventSystemLock ();
				}

				if (GameSystem.Instance.EventSystemLockCount == 0)
					break;
			}
		}

		protected virtual void OnBattleReadyBackButton()
		{
            BattleManager.isRetryGame = false;
            if (BattleHandler.Instance != null) {
                ReleaseBattleData();
                new Controller.UnloadScene(System.transform, SceneTag.BattleCore).Execute();
            }
            ResourceLoader.Clear();
        }

        protected virtual void OnBattleEndStartDirecting(CurrentBattleDirectingWaveInfo directingInfo)
        {

        }

        protected virtual void OnBattleEndDirectingEndType(CurrentBattleDirectingWaveInfo directingInfo)
        {

        }

        protected virtual void OnPreDirectingComplete()
        {
		    
        }
        
        protected virtual void OnAfterDirectingComplete()
        {
		    
        }
        
        #endregion

        #region Methods

        protected void RestartBattle(IView handler)
		{
			BattleManager.isRetryGame = true;

			NotificationWidgetController.Instance?.ShowQuestNotiWidget(false);
			
			handler.Dispose();

			ReleaseBattleData();
			OnUnloadSceneRetryGame ();

			RefreshDeckUnits ();

			new LoadUGUI<UIHelpScreenUGUI> (Res.PREFAB.UIHelpScreenUGUI).Execute ();

			new LoadScene (SceneTag.BattleCore).Execute ();
		}

		void RefreshDeckUnits()
		{
			UserModel user = System.Data.User;

			foreach (var each in user.HeroInventory)
			{
				List<int> userbattleDeckKeys = TroopDeployInfoManager.Instance.UserDeckData.GetCurUserBattleDecks().Keys.ToList ();
				for (int i = 0; i < userbattleDeckKeys.Count; i++) {
					UserBattleDeck battleDeck = TroopDeployInfoManager.Instance.UserDeckData.GetCurUserBattleDecks()[userbattleDeckKeys [i]];
					if (battleDeck != null) {
						if (battleDeck.UnitID == each.ID) {
							battleDeck.UnitInfo = each.Info;
							break;
						}
					}
				}
			}

			TroopDeployInfoManager.Instance.RefreshUnitInfo ();
		}

		protected virtual void RequestRestartBattlePacket()
		{
			
		}

		protected virtual void SetBattleReadyController()
		{
            if (_topMenuController != null) {
                _topMenuController.OnBattleReleaseAction = OnBattleReadyBackButton;
            }
        }
		#endregion
    }
}