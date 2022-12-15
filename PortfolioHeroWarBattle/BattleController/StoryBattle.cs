using System;
using System.Collections;
using System.Collections.Generic;
using Controller;
using Framework;
using Framework.Controller;
using UnityEngine;
using UnityEngine.EventSystems;
		using System.Linq;

namespace NewBattleCore {
	public class StoryBattle : BattleBaseController
    {
		#region Variables

		protected StoryModel _storyModel;
		bool _isClearStage = false;
		int _nextStageIndex = -1;

		private bool _isStageAlreadyCleared;

		#endregion

		#region Methods

		public override void Init () {
			base.Init ();

			_nextStageIndex = -1;
			_storyModel = System.Data.Story;
			var stage = _storyModel.CurrentStageModel;

			int star = 0;
			if (stage != null) star = stage.Star;
			_isStageAlreadyCleared = star > 0;
		}

		protected override string GetBattleDirectingPath () {
			int stageIndex = _storyModel.CurrentStoryStageIndex;

			SheetStoryStage sheetStoryStage = System.Sheet.SheetStoryStage;
			SheetStoryStageRow storyStageInfo = sheetStoryStage[stageIndex];

			return storyStageInfo.BattleDirecting;
		}

		public override void SetNetRequestBattleEnd () {
			base.SetNetRequestBattleEnd ();
			new StoryBattleEnd (this, System.Data, OnBattleEndComplete).Execute ();
		}

        protected override void InitStartDirecting(List<BattleDirectingStepInfo.BattleDirectingInfo> inputDirectingWaveInfos = null)
        {
            var isDirecting = IsStartDirectingEnable(inputDirectingWaveInfos);
            _directingManager.Clear();

            if (isDirecting)
                _directingManager.Enqueue(new Directing_HideUnit());

            _directingManager.Enqueue(new Directing_BattleDirecting());
            _directingManager.Enqueue(new Directing_HideBattleUI());

            bool _isShowUnits = false;

            if (isDirecting) {
                for (int i = 0; i < inputDirectingWaveInfos.Count; i++) {
                    if (!inputDirectingWaveInfos[i].IsActive) continue;

                    if (inputDirectingWaveInfos[i].DirectingType == BattleDirectingType.InGameDirectingTimeLineCutScene &&
                        !BattleManager.ShowBossDirectingAlways())
                        continue;

                    CurrentBattleDirectingWaveInfo waveStartDirecting = BattleDirectingManager.GetCurrentBattleDirectingWaveInfo(inputDirectingWaveInfos[i]);
                    switch (inputDirectingWaveInfos[i].DirectingType) {
                        case BattleDirectingType.NormalReactor: {
                                _directingManager.Enqueue(new Directing_FadeOutIn(null, 0f, 5f, new Directing_HideLoadingImage().Execute, OnFinishDirectingFadeOut), true);
                                _directingManager.Enqueue(new Directing_ShowUnit());
                                _directingManager.Enqueue(new Directing_WaveStartDirecting(null, waveStartDirecting));
                            }
                            break;
                        case BattleDirectingType.ArenaTimelineCutScene: {
                                _directingManager.Enqueue(new Directing_FadeOutIn(null, 0f, 5f, new Directing_HideLoadingImage().Execute, OnFinishDirectingFadeOut), true);
                                _directingManager.Enqueue(new Directing_ShowUnit());
                                _directingManager.Enqueue(new Directing_CutSceneArena(null, waveStartDirecting));
                                BattleManager.Instance.battleCamera.SelectRandomDeployAni();
                                EnqueueDeployCam();
                                _isShowUnits = true;
                            }
                            break;

                        case BattleDirectingType.InGameDirectingTimeLineCutScene: {
                                _directingManager.Enqueue(new Directing_FadeOutIn(null, 0f, 5f, new Directing_HideLoadingImage().Execute, OnFinishDirectingFadeOut), true);
                                _directingManager.Enqueue(new Directing_ShowUnit());

                                var stageDataList = System.Data.Story.GetClearStageList(System.Data.Story.CurrentStoryWorldIndex);
                                bool isCleared = false;
                                if (stageDataList != null) {
                                    isCleared = stageDataList.Contains(System.Data.Story.CurrentStoryStageIndex);
                                }

                                bool alwaysShowStory = System.Data.LocalSaveContainer.IsStoryDirectingAlways;
                                if( UICommonRepeatBattleWidget.autoRepeatBattle == true )
                                {
                                    alwaysShowStory = false;
                                }
                                // 클리어시 스토리 다시보기 옵션
                                if (isCleared && !alwaysShowStory) {

                                } else {
                                    _directingManager.Enqueue(new Directing_CutSceneInGame(null, waveStartDirecting));
                                }

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

            if (!isDirecting) {
                _directingManager.Enqueue(new Directing_Fade(OnFinishDirectingFadeOut, false, 0f));
                _directingManager.Enqueue(new Directing_HideLoadingImage());
                BattleManager.Instance.battleCamera.SelectRandomDeployAni();
                EnqueueDeployCamReady();
                _directingManager.Enqueue(new Directing_Fade(null, true));
                EnqueueDeployCam();
            }

            _directingManager.Enqueue(new Directing_CallBack(null, () => OnFinishedCurStep(_stepManager.CurBattleStep)));

        }

        void OnWatchStory20_8Ending()
        {
            BattleHandler.Instance.scene.Dispose();
            new Controller.UnloadScene(GameSystem.Instance.transform, SceneTag.BattleCore).Execute();
            ResourceLoader.Clear();
            new ShortcutHelper(ShortcutType.Story_EndingVideo).Execute();
	        NotificationWidgetController.Instance?.ShowQuestNotiWidget(false);
            GameSystem.Instance.Data.LocalSaveContainer.Story20_8EndingWatched();
        }

        void OnWatchStory10_8Ending()
        {
            BattleHandler.Instance.scene.Dispose();
            new Controller.UnloadScene(GameSystem.Instance.transform, SceneTag.BattleCore).Execute();
            ResourceLoader.Clear();
            new ShortcutHelper(ShortcutType.Story_10_8EndingVideo).Execute();
	        NotificationWidgetController.Instance?.ShowQuestNotiWidget(false);
            GameSystem.Instance.Data.LocalSaveContainer.Story10_8EndingWatched();
        }

        protected override void SetResultController () {
			base.SetResultController ();

			var handler = new BattleResultStoryHandler(System.Data);
			_resultHandler = handler;
			OnResultLoadComplete();

			handler.ReplayEnergyCount = GetCalcActionPoint ();
			handler.ReplayEnergyImgPath = GetEnergyIconPath ();

            bool win = handler.IsWin();
            bool noReplayButton = false;
            if ( win == true &&
                _storyModel.IsLastStageOfStoryMode() == true &&
                System.Data.User.Tasks.IsComplete(UserTaskTag.Tut_EndingVideoCheck) == false &&
                _storyModel.EliteBattleState == StoryController.StoryEliteModeState.Normal )
            {
                handler.OnButtonHeroInfo = null;
			    handler.OnButtonLobby = null;
			    handler.OnShortcut = null;
                noReplayButton = true;
                handler.OnButtonExit = () =>
                {
                    Scene.System.AudioController.BgmAudioController.Stop();
                    OnWatchStory20_8Ending();
                };

                handler.OnDirectExit = () =>
                {
                    Scene.System.AudioController.BgmAudioController.Stop();
                    OnWatchStory20_8Ending();
                };
            }
            else if (
                win == true &&
                _storyModel.Is10_8StageOfStoryMode() == true &&
                System.Data.User.Tasks.IsComplete(UserTaskTag.Tut_HartsEndingCheck) == false &&
                _storyModel.EliteBattleState == StoryController.StoryEliteModeState.Normal )
            {
                handler.OnButtonHeroInfo = null;
			    handler.OnButtonLobby = null;
			    handler.OnShortcut = null;
                noReplayButton = true;
                handler.OnButtonExit = () =>
                {
                    Scene.System.AudioController.BgmAudioController.Stop();
                    OnWatchStory10_8Ending();
                };

                handler.OnDirectExit = () =>
                {
                    Scene.System.AudioController.BgmAudioController.Stop();
                    OnWatchStory10_8Ending();
                };
            }
            else
            {
                handler.OnButtonHeroInfo = OnHeroInfo;
                handler.OnButtonLobby = OnMainLobby;
                handler.OnShortcut = OnShortCut;
                handler.OnButtonExit = () =>
                {
                    Scene.System.AudioController.BgmAudioController.Stop();
                    OnBattleQuit();
                };

                handler.OnDirectExit = () =>
                {
                    Scene.System.AudioController.BgmAudioController.Stop();
                    OnBattleDirectQuit();
                };
            }

            if( noReplayButton == true )
            {
                handler.OnButtonReplay = null;
            }
            else
            {
                handler.OnButtonReplay = () => OnRetryGameButton (_resultHandler);
            }
            

            if (isNextEnable())
            {
                _nextStageIndex = _storyModel.GetNextStoryStageIndex(_storyModel.EliteBattleState == StoryController.StoryEliteModeState.Normal ? StoryLevelType.Normal : StoryLevelType.Hard);

                if (_nextStageIndex != -1)
                {
	                handler.OnButtonNext = () => OnNextButton(_resultHandler);

	                handler.NextEnergyCount = GetCalcNextActionPoint(_nextStageIndex);
	                handler.NextEnergyImgPath = GetEnergyIconPath();
                }
            }
            
            handler.Show();
		}

		protected override void OnBuildBattle () {
			var context = System.Data.BattleContext;

			int stageIndex = _storyModel.CurrentStoryStageIndex;
			SheetStoryStage sheetStoryStage = System.Sheet.SheetStoryStage;
			SheetStoryStageRow storyStageInfo = sheetStoryStage[stageIndex];

			var troops = new TroopInfo[storyStageInfo.Troop.Length];
			_unitFuryGauges.Clear ();
			for (int i = 0; i < storyStageInfo.Troop.Length; ++i) {
				troops[i] = new StoryTroopInfo ((int) TroopRelation.Enemy, System.Data.BattleContext, storyStageInfo.Troop[i]);
				SheetStoryStageTroopRow stageTroopRow = System.Sheet.SheetStoryStageTroop[storyStageInfo.Troop[i]];
				_unitFuryGauges.Add (stageTroopRow.FuryGauge);
			}

			TroopInfo allyInfo = null;
			allyInfo = TroopInfo.BuildAllyTroopInfo(context);

			var info = new DefaultBattleInfo (context) { Ally = allyInfo };

			info.StageInfo = storyStageInfo;
			info.StageName = _storyModel.GetCurrentStageName ();
			info.Wave = troops;

			info.SetBattleMapName (troops.Length, storyStageInfo.Map);
			info.IsMission = false;
			info.timeOverInSec = storyStageInfo.BattleTimeLimit;

			Scene = BattleScene.Create (System, BattleType.Story, info);

			base.OnBuildBattle ();
		}

		protected override void SetSceneLoadedData () {
			base.SetSceneLoadedData ();
		}

		public override void BattleEnd (BattleResultType resultType) {
			base.BattleEnd (resultType);
		}

		protected override void ReleaseBattleData () {
			base.ReleaseBattleData ();

			Scene.UI.onEndMission = null;

		}

        public bool NeedEnergy()
        {
            if( _assetContainerModel.Get( CurrencyType.Energy ).Amount < GetCalcActionPoint() )
                return true;
            return false;
        }
        

		protected int GetCalcActionPoint () {
			var sheetStoryStage = System.Data.Sheet.SheetStoryStage;
			int currentStoryStageIndex = _storyModel.CurrentStoryStageIndex;

			return sheetStoryStage.Energy[currentStoryStageIndex];
		}

		protected int GetCalcNextActionPoint (int storyStageIndex) {
			var sheetStoryStage = System.Data.Sheet.SheetStoryStage;

			return sheetStoryStage.Energy[storyStageIndex];
		}

		protected string GetEnergyIconPath () {
			return System.Data.Currency.Get (CurrencyType.Energy).IconPath;
		}

		protected override void RequestRestartBattlePacket () {
			base.RequestRestartBattlePacket ();

			new StoryBattleStart (System.Data, OnStartBattleSuccess).Execute ();
		}

		public override void SetBattleEndStoryScript () {
			BattleScene.Instance.HideWorldHeroseForDialog ();

			if (BattleManager.Instance.battle.BattleResult.IsWin) {
				_isClearStage = true;
			}

			if (_isClearStage) {
				_storyModel.StoryEndDialogueName = _storyModel.GetStoryEndDialogueName (_storyModel.CurrentStoryStageIndex);
				if (_storyModel.StoryEndDialogueName != null) {

                    bool alwaysShowStory = System.Data.LocalSaveContainer.IsStoryDirectingAlways;
                    if( UICommonRepeatBattleWidget.autoRepeatBattle == true )
                    {
                        alwaysShowStory = false;
                    }

					// Story Script Replay Option at StageClear
					if (_isStageAlreadyCleared && !alwaysShowStory) {
						OnFinishEndDialog ();
					} else {
                        BattleManager.Instance.battleCamera.SetBattleEndDirecting();
                        
						new PlaySceneBGM (System.AudioController.BgmAudioController, BGMSceneTag.EndReact).Execute ();
						new LoadDialogue (System, _storyModel.StoryEndDialogueName, true, () => {
							new UnloadDialogue (() => {
								OnFinishEndDialog ();
							}).Execute ();
						}).Execute ();

					}
				} else {
					OnFinishEndDialog ();
				}
			} else {
				OnFinishEndDialog ();
			}
		}

		protected override void SetBattleReadyController () {
            base.SetBattleReadyController();

            var controller = BattleManager.emptyBattleTransitionController.Transit<StoryDialogueController>(
             TransitionMethod.Forward, BattleManager.emptyBattleTransitionController,
             () => _resultHandler.Dispose());
            controller.OnBattleResultStart = OnBattleResultStart;
            controller.OnClosedBattleReadyController = OnClosedBattleReadyController;
            controller.OnBattleBackButton = OnBattleReadyBackButton;
        }

        private bool isNextEnable()
        {
            // isClear && ExpansionStageCheck && Tutorial Check.

            return _isClearStage &&
                !_storyModel.IsExistNextStoryStageInSameWorld() && 
                !((_storyModel.CurrentStoryStageIndex == _storyModel.GetOpenConditionStageIndex(UserTaskTag.Tut_OpenEventCondition)) && (!_userModel.Tasks.IsComplete(UserTaskTag.Tut_ClearEvent)));
        }

        #endregion

        #region CallBack Methods

        protected override void OnRetryGameButton (IView handler) {
           
            _storyModel.BattleExitTypeValue = StoryModel.BattleExitType.Retry;

            if( UICommonRepeatBattleWidget.autoRepeatBattle == true)
            {
                if( _assetContainerModel.Get( CurrencyType.Energy ).Amount < GetCalcActionPoint() )
                {
                    return;
                }

                _resultHandler = handler;

                RequestRestartBattlePacket ();
            }
            else
            {
                new PlaySceneBGM( System.AudioController.BgmAudioController, BGMSceneTag.Story ).Execute();

                base.OnRetryGameButton( handler );
            }
			

		}

		protected override void OnNextButton (IView handler) {
			if (_nextStageIndex == -1)
				return;

			new PlaySceneBGM (System.AudioController.BgmAudioController, BGMSceneTag.Story).Execute ();

			_storyModel.CurrentStoryStageIndex = _nextStageIndex;
			_storyModel.BattleExitTypeValue = StoryModel.BattleExitType.NextStage;

			var world = _storyModel.CurrentWorldModel;
			var stage = _storyModel.CurrentStageModel;
            if(_storyModel.EliteBattleState == StoryController.StoryEliteModeState.Normal) {
                GameSystem.Instance.Data.LocalSaveContainer.SaveLastPlayStage(world.Tribe, world.Chapter, stage.StageIndex);
            } else if (_storyModel.EliteBattleState == StoryController.StoryEliteModeState.Elite) {
                GameSystem.Instance.Data.LocalSaveContainer.SaveLastPlayEliteStage(world.Tribe, world.Chapter, stage.StageIndex);
            }

			base.OnNextButton (handler);
		}

		void OnResultLoadComplete () {

		}

		protected override void OnStartBattleSuccess (ResponseParam resParam) {
			base.OnStartBattleSuccess (resParam);
		}

		private void OnFinishEndDialog () {
			_storyModel.StoryEndDialogueName = null;

			_stepManager.SetReserveBattleStep (BattleDefinitions.BattleStep.NetRequestBattleEnd);
		}

        #endregion

	}
}