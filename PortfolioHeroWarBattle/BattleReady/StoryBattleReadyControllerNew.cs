using UnityEngine;
using System.Collections;
using System;
using Controller;
using SpreadSheet;
using System.Collections.Generic;
using Framework;
using System.Linq;
using UnityEngine.UI;

public class StoryBattleReadyControllerNew : BattleReadyControllerNew
{
	#region Variables

	protected Action _onBattleStarted;

	private StoryModel _storyModel;
	private HotTimeModel _hotTimeModel;

	private List<FriendModel> _friendModelList = new List<FriendModel>();

    private UICommonBattleReadyNew _battleReadyNewPanel;

    private bool _isFirst = true;
	private bool _playDialogue = false;
    private bool _isFirstInStoryTutorial = false;

	#endregion

	#region Properties

	public Action OnBattleStarted
	{
		get{ return _onBattleStarted; }
		set{ _onBattleStarted = value; }
	}

	#endregion

	#region BaseController Methods

	protected override void OnInit()
	{
		base.OnInit ();

		_storyModel = System.Data.Story;
		_hotTimeModel = System.Data.HotTime;
       

    }

    protected override void OnEnable()
    {
        _isFirstInStoryTutorial = !System.Data.User.Tasks.IsComplete(UserTaskTag.Tut_ClearStory);
        _processor.Resume();
        base.OnEnable();
    }

    protected override void OnClose()
	{
		if (_isClosed)
			return;

        UICommonRepeatBattleWidget.autoRepeatBattle = false;

        _storyModel.BattleExitTypeValue = StoryModel.BattleExitType.None;

        TroopDeployInfoManager.Exit ();

        _isClosed = true;
        TransitToParent (typeof(StoryController), Kill);

		if (OnBattleBackButton != null)
			OnBattleBackButton ();
	}

    #endregion

    #region Methods

    protected override void DrawBenefitLayout()
    {
        if (_storyModel.EliteBattleState == StoryController.StoryEliteModeState.Normal) {
            base.DrawBenefitLayout();

            var isActiveHotTime = System.Data.HotTime.GetHotTimeList(HotTimeTargetType.Story).Count > 0;

            if (isActiveHotTime) {
                var icon = Icon.BenefitHotTime.Create(_battleReadyPanel.BenefitRoot);
                Icon.BenefitHotTime.ShowBattleReady(icon);
                _benefitIconGroupHandler.Register(icon);
            }

            var isActiveSubscription = System.Data.Subscription.SubscriptionPackage.IsActive;

            if (isActiveSubscription) {
                var icon = Icon.BenefitSubscription.Create(_battleReadyPanel.BenefitRoot);
                Icon.BenefitSubscription.ShowBattleReady(icon);
                _benefitIconGroupHandler.Register(icon);
            }
            
            if (Icon.BenefitEventItem.IsDisplay(IconBenefitEventItemHandler.TargetType.Story))
            {
	            var icon = Icon.BenefitEventItem.Create(_battleReadyPanel.BenefitRoot);
			
	            Icon.BenefitEventItem.Show(icon);
	            _benefitIconGroupHandler.Register(icon);
            }
        } else {
            // Apply Benefit Icons.
            var icon = Icon.BenefitApplyEffect.Create(_battleReadyPanel.BenefitRoot);
            Icon.BenefitApplyEffect.ShowEliteMode(icon, GetApplyEffectType());
            _benefitIconGroupHandler.Register(icon);
        }
	}

	protected override string GetStageTitle()
	{
		return _storyModel.GetCurrentStageName();
	}

    private bool CheckStoryStartDialogue(int selectStageIndex)
    {
        if (_storyModel.GetStoryStartDialogueName(_storyModel.CurrentStoryStageIndex) == null)
            return false;

        _storyModel.CurrentStoryStageIndex = selectStageIndex;

        int storyWorldIndex = _storyModel.CurrentStoryWorldIndex;
        int storyStageIndex = _storyModel.CurrentStoryStageIndex;

		var stageData = _storyModel.CurrentStageModel;

        int star = 0;
		if(stageData != null) star = stageData.Star;
        bool _isStageAlreadyCleared = star > 0;

        bool alwaysShowStory = System.Data.LocalSaveContainer.IsStoryDirectingAlways;
        if( UICommonRepeatBattleWidget.autoRepeatBattle == true )
        {
            alwaysShowStory = false;
        }

        if (!((_isStageAlreadyCleared && !alwaysShowStory) || (_storyModel.BattleExitTypeValue == StoryModel.BattleExitType.Retry) || !_isFirst))
            return true;
        else
            return false;
    }

    private void PlayStoryDialouge()
    {
        string dialogueName = _storyModel.GetStoryStartDialogueName(_storyModel.CurrentStoryStageIndex);

        new PlaySceneBGM(System.AudioController.BgmAudioController, BGMSceneTag.Story).Execute();
        new Directing_StoryDialogue(dialogueName, true, () => 
        {
            if( _battleReadyNewPanel != null )
                _battleReadyNewPanel.gameObject.SetActive(true);
                
            TutorialProcess();
        }).Execute();
    }

    protected override void InitBattleReadyPanel(UICommonBattleReadyNew view)
    {
        _battleReadyNewPanel = view;

        if (_storyModel.EliteBattleState == StoryController.StoryEliteModeState.Elite) {
            view.StartButtonType = ReadyStartButtonType.NoConsume;
        }

        base.InitBattleReadyPanel(view);
        
        if(_storyModel.EliteBattleState == StoryController.StoryEliteModeState.Normal) {
            view.SetHireButton(OnHireButton);

            bool isCleared = false;
            int currentStoryStageIndex = _storyModel.CurrentStoryStageIndex;
            var stage = _storyModel.GetStageModel(currentStoryStageIndex);
            if (stage != null && stage.Star > 0)
                isCleared = true;
            view.SetRepeatBattle(isCleared);
        }
    }
    
	protected override void SetCandidateUnits()
	{
		_unitCandidateManager.CandidateUnits.Clear ();
		UserModel userData = System.Data.User;

        foreach (var each in userData.HeroInventory)
		{
            UnitCandidateInfo inputCandidate = new UnitCandidateInfo();
            inputCandidate.UnitModel = each;
            _unitCandidateManager.AddCandidateUnit(inputCandidate);
		}
	}

    protected override void OnClickCandidateUnit(long heroID, UICommonHeroIcon icon, UserModel user)
    {
	    var hero = _heroModels.Find(x => x.ID == heroID);
	    
        if(_storyModel.EliteBattleState == StoryController.StoryEliteModeState.Elite) {
            if (_storyModel.CurrentStoryChapterType != StoryModel.StoryChapterType.Season2) {
                if (hero != null && (int)hero.Tribe != (int)_storyModel.CurrentStoryChapterType) {
                    string tribeText = "";
                    if (_storyModel.CurrentStoryChapterType == StoryModel.StoryChapterType.Hartz) {
                        tribeText = _textModel.GetText(TextKey.UI_Text_25);
                    } else if (_storyModel.CurrentStoryChapterType == StoryModel.StoryChapterType.Union) {
                        tribeText = _textModel.GetText(TextKey.UI_Text_24);
                    }
                    UIFloatingMessagePopup.Show(string.Format(_textModel.GetText(TextKey.Battle_Ready_Elite_Hero_Select_Error_01), tribeText));
                    return;
                }
            }
        }

        base.OnClickCandidateUnit(heroID, icon, user);
    }

    protected override Tribe GetTribe()
    {
        return Tribe.None;
    }

    protected override void InitSelectedUnitData()
	{
		base.InitSelectedUnitData();
		TutorialProcess();
    }

	protected override int[] GetCurTroopHeroes(int troopIndex)
	{
		return System.Sheet.SheetStoryStageTroop.Hero[troopIndex];
	}

	protected override int[] GetCurTroopUnitLevels(int troopIndex)
	{
		return System.Sheet.SheetStoryStageTroop.Level[troopIndex];
	}

	protected override int[] GetCurTroopUnitDifficultyTypes(int troopIndex)
	{
		return System.Sheet.SheetStoryStageTroop.DifficultyType[troopIndex];
	}

	protected override int GetTroopLevelOffset(int waveIndex)
	{
		return 0;
	}

	protected override int GetTroopDifficultyTypeOffset(int waveIndex)
	{
		return 0;
	}

	protected override string GetAllyFormationPath( int troopIndex )
    {
		return System.Sheet.SheetStoryStageTroop.AllyFormationPath[troopIndex];
    }

	protected override string GetEnemyFormationPath( int troopIndex )
    {
		return System.Sheet.SheetStoryStageTroop.FormationPath[troopIndex];
    }

	protected override string GetBattleEventPath( int troopIndex )
	{
		return System.Sheet.SheetStoryStageTroop.BattleEventPath[troopIndex];
	}

	protected override int GetCalcActionPoint()
	{
        var sheetStoryStage = System.Data.Sheet.SheetStoryStage;
        int currentStoryStageIndex = _storyModel.CurrentStoryStageIndex;

        return sheetStoryStage.Energy[currentStoryStageIndex];
    }

    protected override bool CheckStartEnable()
    {
        bool havingUnit = _deployInfoManager.IsHaveUnit();
        bool energy = (_assetContainerModel.Get(CurrencyType.Energy).Amount >= GetCalcActionPoint());
		bool hellModeCheck = true;

		return havingUnit && energy && hellModeCheck;
    }

	protected override void RequestStartBattlePacket()
	{
		base.RequestStartBattlePacket ();

		NewBattleCore.BattleManager.reservedBattleType = BattleType.Story;

        _storyModel.CurrentStageClearedBefore = _storyModel.isClearStageCheck(_storyModel.CurrentStoryStageIndex);
		new StoryBattleStart(System.Data, OnSuccessBattleStart, OnFailBattleStart).Execute();
	}


	protected override void DisplayReward(int[] rewardIndexs, int[] rewardCouns, int[] rewardPercent)
	{
		base.DisplayReward(rewardIndexs, rewardCouns, rewardPercent);
		
		int currentStoryStageIndex = _storyModel.CurrentStoryStageIndex;
		var stage = _storyModel.GetStageModel(currentStoryStageIndex);
		if (stage != null && stage.Star < 3)
		{
            if(_storyModel.EliteBattleState == StoryController.StoryEliteModeState.Elite) {
                if(stage.Star <= 0) {
                    var itemFirstDrawer = new ItemDrawer(System.Data);
                    var firstIcon = itemFirstDrawer.Draw(_battleReadyPanel.GetTransformRewardIconBG(), stage.FirstClearRewardItemIndex, stage.FirstClearRewardItemCount, false, new Vector3(0.75f, 0.75f, 0.75f)).target as UICommonBaseItemIcon;
                    ResourceLoader.Instantiate(Res.PREFAB.UICommonItemIconPart_First, firstIcon.gameObject.transform);
                }
            }

			var itemDrawer = new ItemDrawer(System.Data);
			var icon = itemDrawer.Draw(_battleReadyPanel.GetTransformRewardIconBG(), stage.PerfectClearRewardItemIndex, stage.PerfectClearRewardItemCount, false, new Vector3(0.75f, 0.75f, 0.75f)).target as UICommonBaseItemIcon;
			icon.TogglePerfect(true);
		}
	}

	protected override int[] GetRewardIndexs()
    {
        var sheetStoryStage = System.Data.Sheet.SheetStoryStage;
        int currentStoryStageIndex = _storyModel.CurrentStoryStageIndex;

        return sheetStoryStage.RewardItemIndex[currentStoryStageIndex];
    }

	protected override int[] GetRewardCounts()
	{
		var sheetStoryStage = System.Data.Sheet.SheetStoryStage;
		int currentStoryStageIndex = _storyModel.CurrentStoryStageIndex;

		return sheetStoryStage.RewardItemCount[currentStoryStageIndex];
	}
	
	protected override int[] GetRewardRate()
	{
		var sheetStoryStage = System.Data.Sheet.SheetStoryStage;
		int currentStoryStageIndex = _storyModel.CurrentStoryStageIndex;
		
		return sheetStoryStage.RewardItemRate[currentStoryStageIndex];
	}

	protected override int GetRewardExp()
	{
		var sheetStoryStage = System.Data.Sheet.SheetStoryStage;
		int currentStoryStageIndex = _storyModel.CurrentStoryStageIndex;

		return sheetStoryStage.Exp[currentStoryStageIndex];
	}

	protected override void InitTroopListData()
	{
		base.InitTroopListData ();

		var sheetStoryStage = System.Data.Sheet.SheetStoryStage;

		int currentStoryStageIndex = _storyModel.CurrentStoryStageIndex;

		_troopList = sheetStoryStage.Troop[currentStoryStageIndex];
	}

	protected override void OnDrawHeroIcon(int index, UICommonHeroIcon icon)
	{
		var hero = _heroModels[index];
		Icon.Hero.Show(icon, hero, id => OnClickCandidateUnit(id, icon, System.Data.User));

		if (_storyModel.EliteBattleState == StoryController.StoryEliteModeState.Elite &&
		    _storyModel.CurrentStoryChapterType != StoryModel.StoryChapterType.Season2 &&
		    (int) hero.Tribe != (int) _storyModel.CurrentStoryChapterType)
		{
			icon.ToggleUnuse(true);
			var unused = icon.ObjUnuse.GetComponent<Image>();
			if (unused != null)
				unused.raycastTarget = false;
		}
		else
		{
			icon.ToggleUnuse(false);	
		}
		
		OnDrawFreeHeroIcon(hero.Index, icon);

		if (GetCheckSelectedUnits(hero.ID))
			icon.ToggleSelect(true);
		else
			icon.ToggleSelect(false);
	}

	protected override void RefreshHeroList()
    {
	    var userSaveData = System.Data.LocalSaveContainer.UserSaveData;
	    var sortHandler = _heroListHandler.SortHandler;
	    sortHandler.Set(() =>
	    {
		    userSaveData.HeroSortSelectedIndex = sortHandler.HeroSortSelectedIndex;
		    userSaveData.IsHeroSortAscent = sortHandler.HeroSortIsAscent;
		    userSaveData.Save();
		    RefreshHeroList();
	    });

        List<HeroModel> candidateUnits = new List<HeroModel>();
        for (int i = 0; i < _unitCandidateManager.CandidateUnits.Count; i++) {
            candidateUnits.Add(_unitCandidateManager.CandidateUnits[i].UnitModel);
        }

        List<HeroModel> sortedHeroModels = GetSortedHeroModel(candidateUnits);
        
        _heroModels.Clear();
        var notTribeHero = new List<HeroModel>();
        for (int i = 0; i < sortedHeroModels.Count; i++) {
            HeroModel hero = sortedHeroModels[i];

            if (sortHandler.Tribe != Tribe.None && hero.Tribe != sortHandler.Tribe)
                continue;

            if (_storyModel.EliteBattleState == StoryController.StoryEliteModeState.Elite &&
                _storyModel.CurrentStoryChapterType != StoryModel.StoryChapterType.Season2 &&
                (int) hero.Tribe != (int) _storyModel.CurrentStoryChapterType)
            {
	            notTribeHero.Add(hero);
	            continue;
            }

            _heroModels.Add(hero);
        }

        _heroModels.AddRange(notTribeHero);
        
        if (AddFreeAvailableHero())
        {
	        int heroIndex = _user.HeroContractAvailable.GetLast();
	        HeroModel addHeroModel = new HeroModel(System.Data, heroIndex);
	        _heroModels.Insert(0, addHeroModel);
        }

        if (!_heroListHandler.IsSetReuseScroll)
	        _heroListHandler.SetReuseScroll(_heroModels.Count, OnDrawHeroIcon, OnReleaseHeroIcon);
        else
	        _heroListHandler.ResetReuseScroll(_heroModels.Count);
    }
	
	protected override int GetUnusedCount()
	{
		int count = 0;

		if (_storyModel.EliteBattleState == StoryController.StoryEliteModeState.Elite &&
		    _storyModel.CurrentStoryChapterType != StoryModel.StoryChapterType.Season2)
		{
			foreach (var each in _heroModels)
			{
				if ((int) each.Tribe != (int) _storyModel.CurrentStoryChapterType)
					count++;
			}
		}

		return count;
	}

    protected override void InitDeployDeckInfo()
	{
		base.InitDeployDeckInfo ();

        if (_playDialogue)
            _battleReadyPanel.gameObject.SetActive(false);
    }

	protected override void LoadAllyTroopData()
	{
		base.LoadAllyTroopData ();
        
        if(_storyModel.EliteBattleState == StoryController.StoryEliteModeState.Elite) {
            _deployInfoManager.UserDeckData.SetDeckFileName(TroopDeployDefinitions.UserDeckKeyType.StoryEliteDeck, System.Data.User.userData.userId, _storyModel.CurrentStoryChapterType.ToString());
        } else {
            _deployInfoManager.UserDeckData.SetDeckFileName(TroopDeployDefinitions.UserDeckKeyType.StoryDeck, System.Data.User.userData.userId, _storyModel.CurrentTribe);
        }

		_deployInfoManager.UserDeckData.LoadCurDeckFile ();
	}

	protected override void MakeForceSortieUnit ()
	{
		int currentStoryStageIndex = _storyModel.CurrentStoryStageIndex;
		SheetStoryStageRow storyStageRow = System.Data.Sheet.SheetStoryStage [currentStoryStageIndex];

		_forceSortieUnit.ForceSortieUnitInfos.Clear ();
		if (storyStageRow.ForceHero != null && storyStageRow.ForceHero.Length > 0) {
			for (int i = 0; i < storyStageRow.ForceHero.Length; i++) {
				int posIndex = i;
				int unitIndex = storyStageRow.ForceHero [i];
				int unitLevel = storyStageRow.ForceHeroLevel [i];
				int unitDifficultyType = storyStageRow.ForceDifficulty [i];
				if (unitIndex != 0) {
					ForceSortieUnitInfo inputForceUnitInfo = new ForceSortieUnitInfo ();
					inputForceUnitInfo.UnitIndex = unitIndex;
					inputForceUnitInfo.Level = unitLevel;
					inputForceUnitInfo.DifficultyType = unitDifficultyType;

					_forceSortieUnit.ForceSortieUnitInfos.Add (posIndex, inputForceUnitInfo);
				}
			}
		}
	}

	protected override void RequestHelperList()
	{
		new FriendBattleSupportList(System.Data,
			(resParam) =>
		{
			_friendModelList.AddRange(System.Data.FriendContainer.FriendsSupport);
			_friendModelList.AddRange(System.Data.FriendContainer.RecommendsSupport);
			
			OnHireButton();
		}).Execute();
	}

	protected override void SetHireData()
	{
		SheetHero sheetHero = System.Data.Sheet.SheetHero;

		for (int i = 0; i < _friendModelList.Count; i++)
		{
			FriendModel model = _friendModelList[i];
			
			int userLevel = model.userLevel;
			string userName = model.userNickname;
			long userId = model.userId;
			
			bool recommend = System.Data.FriendContainer.RecommendsSupport.Contains(model);

			var heroModel = new HeroModel(System.Data, new HeroData
			{
				index =  model.representHeroIndex,
				id = model.representHeroId,
				lv = model.representHeroLevel,
				tier = model.representHeroTier
			});
			heroModel.FrameRanking = model.representHeroFrameRanking;

			_hiredHeroPopup.AddHiredHeroSlot(heroModel, userName, userId, userLevel, recommend);
		}
	}

    protected override void OnClickHireSelect( int x )
	{
		base.OnClickHireSelect(x);

		int index = x;

		if (index >= 0 && _friendModelList.Count > index)
		{
			int validIndex = _deployInfoManager.GetFriendUnitIndex(_deployInfoManager.CurWaveIndex);

			if (validIndex >= 0)
				NotifyDeckSetting(-1, TroopDeployDefinitions.DeckUnitType.Empty, _friendUnitInfo.UID, _friendUnitInfo, false);
			else
				validIndex = _deployInfoManager.GetValidPosIndex(_deployInfoManager.CurWaveIndex);

			if (validIndex >= 0)
			{
				FriendModel model = _friendModelList[index];

				if (_dicFriendInfo.ContainsKey(model.userId))
				{
					SetFriendInfo(validIndex, _dicFriendInfo[model.userId], model.userId, model.representHeroLevel, model.userNickname);
				}
				else
				{
					new HeroInfoTask(System.Data, model.userId, model.representHeroId, (res) =>
					{
						CompanyGrowthInfo companyGrowthInfo = new CompanyGrowthInfo(System.Data, res.heroGroup.companyGrowthInfo);
						
						HeroInfo info = new HeroInfo(System.Data.BattleContext, res.heroGroup.battleHeroList[0], companyGrowthInfo);
						_dicFriendInfo.Add(model.userId, info);
						SetFriendInfo(validIndex, info,  model.userId, model.representHeroLevel, model.userNickname);

					}).Execute();
				}
			}
		}
		else
		{
			NotifyDeckSetting( -1, TroopDeployDefinitions.DeckUnitType.Empty, _friendUnitInfo.UID, _friendUnitInfo, false);
			AutoAlignAllyFormationIcons(IsAutoMode);
		}
	}

	void SetFriendInfo( int validIndex, HeroInfo info, long userId, int level, string userName = "" )
	{
		_friendUnitInfo = info;
		_friendUnitInfo.IsFriendUnit = true;

		NotifyDeckSetting(validIndex, TroopDeployDefinitions.DeckUnitType.FriendUnit, _friendUnitInfo.UID, _friendUnitInfo, false);
		AutoAlignAllyFormationIcons(IsAutoMode);

		SheetHero sheetHero = System.Data.Sheet.SheetHero;

		FriendUnitBaseInfo friendUnitBaseInfo = new FriendUnitBaseInfo();
		friendUnitBaseInfo.UserID = userId;
		friendUnitBaseInfo.HeroID = info.UID;
		friendUnitBaseInfo.UserNickname = userName;
		friendUnitBaseInfo.UnitInfo = info;

		TroopDeployInfoManager.Instance.FriendUnitInfo = friendUnitBaseInfo;
	}

    protected override HeroInfo GetBestSelectHero(List<long> exceptList = null)
    {
        HeroInfo currentInfo = null;

        if (_storyModel.EliteBattleState == StoryController.StoryEliteModeState.Elite && _storyModel.CurrentStoryChapterType != StoryModel.StoryChapterType.Season2) {
            List<HeroModel> validHeroList = new List<HeroModel>();
            foreach (var each in _user.HeroInventory) {
                if(_storyModel.CurrentStoryChapterType == StoryModel.StoryChapterType.Hartz) {
                    if(each.Tribe == Tribe.Villain) {
                        validHeroList.Add(each);
                    }
                } else if (_storyModel.CurrentStoryChapterType == StoryModel.StoryChapterType.Union) {
                    if (each.Tribe == Tribe.Survival) {
                        validHeroList.Add(each);
                    }
                }
            }

            for(int i = 0;i< validHeroList.Count; i++) {
                HeroModel each = validHeroList[i];
                if (exceptList != null && exceptList.Contains(each.Info.UID))
                    continue;

                if (currentInfo == null) {
                    if (!GetCheckSelectedUnits(each.Info.UID)) {
                        currentInfo = each.Info;
                    }
                } else {
                    if (currentInfo.stat[(int)Stat.TotalCombatPower] < each.Info.stat[(int)Stat.TotalCombatPower]
                        && !GetCheckSelectedUnits(each.Info.UID)) {
                        currentInfo = each.Info;
                    }
                }
            }
        } else {
            foreach (var each in _user.HeroInventory) {
                if (exceptList != null && exceptList.Contains(each.Info.UID))
                    continue;

                if (currentInfo == null) {
                    if (!GetCheckSelectedUnits(each.Info.UID)) {
                        currentInfo = each.Info;
                    }
                } else {
                    if (currentInfo.stat[(int)Stat.TotalCombatPower] < each.Info.stat[(int)Stat.TotalCombatPower]
                        && !GetCheckSelectedUnits(each.Info.UID)) {
                        currentInfo = each.Info;
                    }
                }
            }
        }
        
        return currentInfo;
    }

    #endregion

    #region tutorial
    private void TutorialProcess()
    {
        Tutorial_First();
        Tutorial_ToEvent();
        Tutorial_ToGriselda();
    }

    private void Tutorial_First()
    {
        var tutorial = new Tutorial_StoryBattleReady_First(System, _battleReadyPanel, RemoveAllSelectedUnit);
        tutorial.Execute();
    }

    private void Tutorial_ToEvent()
    {
        var tutorial = new Tutorial_StoryBattleReady_Event(System, _battleReadyPanel, OnClickAutoSelectHero);
        tutorial.Execute();
    }

    private void Tutorial_ToGriselda()
    {
        var tutorial = new Tutorial_StoryBattleReady_Griselda(System, _battleReadyPanel, OnClickAutoSelectHero);
        tutorial.Execute();
    }

    #endregion
}
