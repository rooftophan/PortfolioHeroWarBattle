using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Controller;
using Framework;
using Controller;
using System.Linq;
using System;
using NewBattleCore;

public class BattleReadyControllerNew : BattleReadyBaseDataController
{
    #region IconSort

    public class FormationIconComparer : IComparer<FormationDeckIconInfo>
    {
        public int Compare( FormationDeckIconInfo a, FormationDeckIconInfo b )
        {
            int aScore = a.AutoSortScore();
            int bScore = b.AutoSortScore();            
            if( aScore == bScore)
                return a.PosIndex.CompareTo( b.PosIndex );
            return bScore.CompareTo( aScore );
        }
    }
    
    public class IconSortData
    {
        public long unitID;
        public HeroInfo unitInfo;
        public bool add; 
		public int posIndex;
    }

    #endregion

	#region Variables

	protected ProceduralProcessor _processor;

	protected UserModel _user;
	protected UserAssetContainer _assetContainerModel;
	protected TextModel _textModel;

	protected RewardModel _rewardModel;
	protected HeroPossessionPassiveModel _passiveModel;
	
	protected UICommonBattleReadyNew _battleReadyPanel = null;
	protected TopMenuController _topMenuController;
	private TopMenuViewType _currentTopMenuViewType = TopMenuViewType.Default;
	private bool _initTopMenu = false;
	protected UICommonHiredHeroPopup _hiredHeroPopup = null;
	protected HeroInfo _friendUnitInfo = null;
	protected HeroListHandler _heroListHandler;
	private bool _reloadHelperList = true;

	protected BenefitIconGroupHandler _benefitIconGroupHandler = new BenefitIconGroupHandler();

	protected bool _isStartBattle = false;

	protected UnitCandidateManager _unitCandidateManager = new UnitCandidateManager ();

	protected List<IDeckSettingObserver> _deckSettingObs = new List<IDeckSettingObserver> ();

	protected bool _isClosed = false;

    FormationIconComparer _iconComparer = new FormationIconComparer();

    protected List<HeroModel> _heroModels = new List<HeroModel>();

    protected Dictionary<int, UIRewardIcon> _rewardIconList = new Dictionary<int, UIRewardIcon>();

	protected Action _onBattleResultStart = null;

	protected Action _onClosedBattleReadyController = null;

	protected Dictionary<long, HeroInfo> _dicFriendInfo = new Dictionary<long, HeroInfo>();

	public Tribe CurrentTribe => Tribe.None;

	Action _onBattleBackButton = null;

    protected IController<GameSystem> _fromController = null;

	protected UIUserLevelUpPopup _levelUpPopup = null;

	protected List<RewardData> _reward = new List<RewardData>();

    protected bool _isCenterGap = false;

    protected bool _emptyCheck = false;

    #endregion

    #region Properties

    protected bool IsStartBattle
	{
		get{ return _isStartBattle; }
        set{ _isStartBattle = value; }
	}

	protected bool IsAutoMode
    {
        get {
            if (_deployInfoManager.UserDeckData.UserDeckSave.CurUserDeckSave == null)
                return false;

            return _deployInfoManager.UserDeckData.UserDeckSave.CurUserDeckSave.AutoMode;
        }
    }

	public Action OnBattleResultStart
	{
		get{ return _onBattleResultStart; }
		set{ _onBattleResultStart = value; }
	}

	public Action OnClosedBattleReadyController
	{
		get{ return _onClosedBattleReadyController; }
		set{ _onClosedBattleReadyController = value; }
	}

	public Action OnBattleBackButton
	{
		get{ return _onBattleBackButton; }
		set{ _onBattleBackButton = value; }
	}

    public IController<GameSystem> FromController
    {
        get { return _fromController; }
        set { _fromController = value; }
    }

    #endregion

    #region BaseController Methods

    protected override void OnInit()
	{
		base.OnInit();

		_user = System.Data.User;
		_assetContainerModel = System.Data.Currency;
		_unitDeckPosManager.TextModel = _textModel = System.Data.Text;
		_rewardModel = System.Data.Reward;
		_passiveModel = System.Data.HeroPossessionPassiveModel;
		
		TroopDeployInfoManager.Exit ();

		_processor = new ProceduralProcessor();
		_isClosed = false;
	}

	protected override void OnEnable()
	{
		_processor.Resume();

		_processor.Add(() => SetEnableData());

		_processor.Add(new Procedure(Transition.Complete));

        _processor.Add(() => RefreshEnableData());

        _processor.Add(new Framework.Wait(() => _isStartBattle));

		_processor.Add(() => TransitBattleController());
	}

	protected override void OnReload()
	{
		_processor.Clear();
		_processor.Resume();

        //1Frame Delay. Defense BaseController Error.
        _processor.Add(new Framework.Wait(() => true));

        _processor.Add (() => InitBattleReadyData ());
        
		_processor.Add(new LoadUGUI<UICommonBattleReadyNew>(Res.PREFAB.UICommonBattleReadyNew, (view) => InitBattleReadyPanel(view)));
		
		_processor.Add (() => InitCandidateUnits ());

		_processor.Add(() =>
		{
			var userSaveData = System.Data.LocalSaveContainer.UserSaveData;
			InitHeroListHandler(new HeroListHandler(userSaveData.HeroSortSelectedIndex, userSaveData.IsHeroSortAscent));
		});

		_processor.Add (() => InitDeployDeckInfo ());
	}

	protected override void OnUpdate()
	{
		if (_processor.IsComplete)
			_processor.Terminate();
		else
			_processor.Execute();
	}

	protected override void OnUnload()
	{
		base.OnUnload();

		if (_battleReadyPanel != null) {
            _battleReadyPanel.Dispose();
        }

		ReleaseDeckSettingOb ();
		if (_unitDeckPosManager != null) {
			_unitDeckPosManager.AllyFormationHandler.ReleaseDeckChangePosOb ();
			_unitDeckPosManager.ReleaseCurIconList ();
		}
		
		if (_heroListHandler != null)
		{
			_heroListHandler.Dispose();
			_heroListHandler = null;
		}

        if(_hiredHeroPopup != null)
            _hiredHeroPopup.Dispose();

		if(_unitCandidateManager != null)
			_unitCandidateManager.ReleaseCandidateUnit ();

        if (_topMenuController != null)
        {
            _topMenuController.OnRefreshViewAction = null;
            _topMenuController = null;
        }
	}

	protected override void OnDisable()
	{
		base.OnDisable();

		_processor.Clear();

		if (_battleReadyPanel != null) _battleReadyPanel.Hide();
	}

	public override void Terminate()
	{
		base.Terminate ();

		if (_onClosedBattleReadyController != null)
			_onClosedBattleReadyController ();

        if (_deployInfoManager != null) {
            if (_deployInfoManager.IsRevisionIconPos)
                ResetAllyDeckOffsetPos();
        }
    }

	#endregion

	#region Methods

	protected virtual void InitBattleReadyPanel(UICommonBattleReadyNew view)
	{
		_battleReadyPanel = view;
		_battleReadyPanel.SetTitleText(GetStageTitle());

        if (view.StartButtonType == ReadyStartButtonType.Normal) {
			_battleReadyPanel.SetTextStart (_textModel.GetText (TextKey.Battle_Start));
		}


		_battleReadyPanel.onChangeClassTab = (x) => OnChangeClassTab(x);
		_battleReadyPanel.onChangeWaveTab = ((x) => 
			{
				OnChangeWave(x);
			});
		_battleReadyPanel.onChangeShowTargetToggle = ( x ) => _unitDeckPosManager.AllyFormationHandler.ChangeShowTarget( x, BattleReadyTargetAimData.ShowTargetType.Ally );
        _battleReadyPanel.onChagnedAutoMode = (x) => OnChangeAutoMode( x);


		_battleReadyPanel.SetStartAction(() => { return OnStartBattle (GetCalcActionPoint (), false); }, 
                                         () => { return OnStartBattle (GetCalcActionPoint (), true); },
                                         () =>
                                         {
                                             TransitToParent(typeof(MainController), () =>
                                             {
                                                 Popup.Normal.Show(System.Data.Text.GetText(TextKey.SearchPoint_Bonus_Battle_End_Popup));
                                             });
                                         });

		_battleReadyPanel.SetRequiredEnergySprite(GetEnergyIconPath());
		_battleReadyPanel.SetRequiredEnergyCount (GetCalcActionPoint());
        
        _battleReadyPanel.SetOnUpdateAim( _unitDeckPosManager.AllyFormationHandler.OnUpdateAim);
		_battleReadyPanel.onDefaultMode = () => AutoAlignAllyFormationIcons(true);
		
		_battleReadyPanel.SetAutoSelectButtonAction(OnClickAutoSelectHero);

		DrawBenefitLayout();
		
		OnInitPanel ();

		DisplayReward(GetRewardIndexs(), GetRewardCounts(), GetRewardRate());
		_battleReadyPanel.SetRewardText();
	}

	/// <summary>
	/// 좌상단, 추가효과에 대한 정의, 기본적으로 ApplyEffect를 표시해준다.
	/// 컨텐츠에 따라 표시해야하는 Benefit이 다른 경우, override하여 표시한다.
	/// </summary>
	protected virtual void DrawBenefitLayout()
	{
		// Apply Benefit Icons.
		var icon = Icon.BenefitApplyEffect.Create(_battleReadyPanel.BenefitRoot);
		Icon.BenefitApplyEffect.Show(icon, GetApplyEffectType());
		_benefitIconGroupHandler.Register(icon);
	}
	
	protected virtual void InitHeroListHandler(HeroListHandler heroListHandler)
	{
		_heroListHandler = heroListHandler;
		
		_heroListHandler.SetToStatic();
		_heroListHandler.Show();
	}

	protected virtual void OnInitPanel()
	{

    }

	private bool IsShowGuide()
	{
		bool isShow = true;

		if (MissionManager.Instance != null &&
		    MissionManager.Instance.CurrentMissionType == MissionContentType.PartyMission)
			isShow = false;

		return isShow;

	}

	protected virtual string GetTitleName()
	{
		return _textModel.GetText(TextKey.Battle_Ready_Name);
	}

	protected virtual string GetStageTitle()
	{
		return _textModel.GetText(TextKey.Battle_Ready_Name);
	}

	protected virtual void InitBattleReadyData()
	{
        if (!TroopDeployInfoManager.isLoadTroopDeploy) {
            _deployInfoManager = TroopDeployInfoManager.Instance;
            if (BattleHandler.Instance == null) {
                
                _onBattleResultStart = null;
            }
        }

        AttachDeckSettingOb ((IDeckSettingObserver)_unitDeckPosManager);
		AttachDeckSettingOb ((IDeckSettingObserver)_deployInfoManager.UserDeckData);
		AttachDeckSettingOb ((IDeckSettingObserver)_deployInfoManager.UserDeckData.UserDeckSave);

		if(!TroopDeployInfoManager.isLoadTroopDeploy) {
			_unitDeckPosManager.InitUnitDeckPosData ();
        }
		_unitDeckPosManager.AllyFormationHandler.AttachDeckChangePosOb ((IDeckChangePosObserver)_deployInfoManager.UserDeckData.UserDeckSave);

		InitTroopListData();

		if(TroopDeployInfoManager.isLoadTroopDeploy)
			RefreshDeckUnits ();
	}

	protected void RefreshDeckUnits()
	{
		UserModel userData = System.Data.User;

		foreach (var each in userData.HeroInventory)
		{
			List<int> userbattleDeckKeys = TroopDeployInfoManager.Instance.UserDeckData.GetCurUserBattleDecks().Keys.ToList ();
			for (int i = 0; i < userbattleDeckKeys.Count; i++) {
				UserBattleDeck battleDeck = TroopDeployInfoManager.Instance.UserDeckData.GetCurUserBattleDecks()[userbattleDeckKeys [i]];
				if (battleDeck != null) {
					if (battleDeck.UnitID == each.ID) {
						battleDeck.UnitInfo = each.Info;
						battleDeck.UnitInfo.Init ();
						break;
					}
				}
			}

			foreach (var eachCurIconInfo in _unitDeckPosManager.AllyFormationHandler.CurFormationIconInfos.Values)
			{
				if (eachCurIconInfo.UnitInfo == null)
					continue;
				
				if (eachCurIconInfo.UnitInfo.UID == each.Info.UID)
				{
					eachCurIconInfo.UnitInfo = each.Info;
					eachCurIconInfo.UnitInfo.Init();
					break;
				}
			}
		}
		
		TroopDeployInfoManager.Instance.RefreshUnitInfo ();
	}

	protected virtual void InitDeployDeckInfo()
	{
		if (!TroopDeployInfoManager.isLoadTroopDeploy) {
			_deployInfoManager.CurrentEnergyCount = GetCalcActionPoint ();
			_deployInfoManager.CurrentEnergyPath = GetEnergyIconPath ();

            if(_troopList == null || _troopList.Length == 0)
            {
                _deployInfoManager.FormaionMapInfo.AddWaveFormationInfo(0, GetAllyFormationPath(-1), GetEnemyFormationPath(-1), GetBattleEventPath(-1));
            }
            else
            {
                for(int i = 0; i < _troopList.Length; i++)
                {
                    _deployInfoManager.FormaionMapInfo.AddWaveFormationInfo(i, GetAllyFormationPath(_troopList[i]), GetEnemyFormationPath(_troopList[i]), GetBattleEventPath(_troopList[i]));
                }
            }
		}

		_troopList = GetValidHeroes (_troopList);

        _unitDeckPosManager.AllyFormationHandler.InitFormationData(_battleReadyPanel.AllyFormation);
        _unitDeckPosManager.EnemyFormationHandler.InitFormationData(_battleReadyPanel.EnemyFormation);

        _unitDeckPosManager.AllyFormationHandler.UIFormation.CurrentTribe = CurrentTribe;
              
        Vector3[] pos = new Vector3[4];

        _battleReadyPanel.DrawArea.GetWorldCorners( pos );
        Rect boundAreaRectInWorld = Rect.MinMaxRect( pos[1].x, pos[3].y, pos[3].x, pos[1].y );

        _unitDeckPosManager.AllyFormationHandler.SetRevisionBoundAreaRectInWorld(boundAreaRectInWorld);

        _unitDeckPosManager.EnemyFormationHandler.BoundAreaRectInWorld = boundAreaRectInWorld;
        _unitDeckPosManager.AllyFormationHandler.SetDragEndAction(OnChangeAutoMode);

        if(!TroopDeployInfoManager.isLoadTroopDeploy) {
			_deployInfoManager.MapMoveBoundInfos.Clear ();

			for (int i = 0; i < _deployInfoManager.FormaionMapInfo.WaveFormationInfos.Count; i++) {
				WaveFormationInfo waveFormation = _deployInfoManager.FormaionMapInfo.WaveFormationInfos [i];
				_unitDeckPosManager.AllyFormationHandler.AddFormationTroopInfo (i, waveFormation.allyTroopInfos, waveFormation.allyMoveDistance);
				_unitDeckPosManager.EnemyFormationHandler.AddFormationTroopInfo (i, waveFormation.enemyTroopInfos, waveFormation.enemyMoveDistance);
				_unitDeckPosManager.EnemyFormationHandler.AddFormationExtraTroopInfo (i, waveFormation.extraTroopInfos, waveFormation.extraMoveDistance);

				MapMoveBoundInfo inputMapMoveBound = new MapMoveBoundInfo ();
				inputMapMoveBound.UIBound = waveFormation.UIBound;
				inputMapMoveBound.Moveable = waveFormation.moveable;
				inputMapMoveBound.MoveableBound = waveFormation.moveableBound;

				Rect allyMoveAreaRect = inputMapMoveBound.GetMoveableAreaRect (_unitDeckPosManager.AllyFormationHandler.BoundAreaRectInWorld, _unitDeckPosManager.AllyFormationHandler.UIFormation);
				Rect enemyMoveAreaRect = inputMapMoveBound.GetMoveableAreaRect (boundAreaRectInWorld, _unitDeckPosManager.EnemyFormationHandler.UIFormation);

				_unitDeckPosManager.AllyFormationHandler.MoveableAreaRectInWorlds.Add (i, allyMoveAreaRect);
				_unitDeckPosManager.EnemyFormationHandler.MoveableAreaRectInWorlds.Add (i, enemyMoveAreaRect);

				_deployInfoManager.MapMoveBoundInfos.Add (i, inputMapMoveBound);
			}
		}

		MakeForceSortieUnit ();
		MakeFixedDeckData ();
		MakePreSettingUnitData ();
		MakeEnemyTroopData ();

		if (!TroopDeployInfoManager.isLoadTroopDeploy) {
			LoadAllyTroopData ();
			SetLoadAllyDeckData ();
			SetAddBattleDeckInfos ();
		}
		
		int maxWaveIndex = _deployInfoManager.EnemyBattleTroopInfos.Count - 1;
		OnChangeWave(maxWaveIndex);

		InitSelectedUnitData ();

		if(!_isFirstStart)
			_isFirstStart = true;

        if (!TroopDeployInfoManager.isLoadTroopDeploy)
            TroopDeployInfoManager.isLoadTroopDeploy = true;

        MapMoveBoundInfo mapMoveBound = _deployInfoManager.MapMoveBoundInfos[_deployInfoManager.CurWaveIndex];

		PreCheckStartButton();
		
		WaveFormationInfo curWaveFormation = TroopDeployInfoManager.Instance.FormaionMapInfo.WaveFormationInfos [TroopDeployInfoManager.Instance.CurWaveIndex];
		_battleReadyPanel.SetVisibleDefaultModeButton(!IsAutoMode);
		_unitDeckPosManager.AllyFormationHandler.UIFormation.ChangeAutoMode(!mapMoveBound.Moveable);
	}

	protected virtual void PreCheckStartButton()
	{
        _battleReadyPanel.ButtonEnable(CheckStartEnable());
	}

	protected virtual void OnDrawHeroIcon(int index, UICommonHeroIcon icon)
	{
		var hero = _heroModels[index];
			
		Icon.Hero.Show(icon, hero, id => OnClickCandidateUnit(id, icon, System.Data.User));

		OnDrawFreeHeroIcon(hero.Index, icon);

		icon.ToggleSelect(GetCheckSelectedUnits(hero.ID), _heroListHandler.Expand);
	}

	protected void OnDrawFreeHeroIcon(int heroIndex, UICommonHeroIcon icon)
	{
		if (AddFreeAvailableHero())
		{
			int freeHeroIndex = _user.HeroContractAvailable.GetLast();

			Transform transFree = icon.GetTransFree();
			if (heroIndex == freeHeroIndex)
			{
				if( transFree.childCount == 0 )
					ResourceLoader.Instantiate(Res.PREFAB.UICommonHeroIconPart_Free, transFree);
				else
					transFree.GetChild(0).gameObject.SetActive(true);

				icon.SetTextLevel("");
				icon.SetButtonAction((long x) =>
				{
					UIFloatingMessagePopup.Show(_textModel.GetText(TextKey.Battle_Ready_Hero_Free_Contract));
				});
			}
			else
			{
				if (transFree.childCount > 0)
					transFree.GetChild(0).gameObject.SetActive(false);
			}
		}
	}

	protected virtual void OnReleaseHeroIcon(UICommonHeroIcon icon) { }
	
	protected virtual void RefreshHeroList()
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
		
		_heroModels.Clear();

		foreach (var each in _unitCandidateManager.CandidateUnits)
		{
			if (sortHandler.Tribe != Tribe.None && each.UnitModel.Tribe != sortHandler.Tribe)
				continue;

			_heroModels.Add(each.UnitModel);
		}
		
		_heroModels = GetSortedHeroModel(_heroModels);

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

	protected virtual List<HeroModel> GetSortedHeroModel(List<HeroModel> candidateUnits)
	{
		var sortedList = new List<HeroModel>(candidateUnits);
		sortedList.Sort(_heroListHandler.SortHandler.SortMethod);
		
		return sortedList;
	}
	
	protected virtual Tribe GetTribe()
	{
		return Tribe.None;
	}

	protected virtual bool AddFreeAvailableHero()
	{
		if (_user.HeroContractAvailable.GetLast() > 0)
			return true;
		
		return false;
	}
	
	protected virtual void InitSelectedUnitData()
	{
		RefreshHeroList();
		
		Dictionary<int /* Unit Index */ , FormationDeckIconInfo> checkDeckIconInfos = GetCheckSelectedUnits ();

		// Check ForceSortie Unit
		Dictionary<string /* SkillHeroID */ , ForceSortieUnitInfo> checkForceSortie = new Dictionary<string, ForceSortieUnitInfo> ();
		List<int> sortieKeys = _forceSortieUnit.ForceSortieUnitInfos.Keys.ToList ();
		for (int i = 0; i < sortieKeys.Count; i++) {
			SheetHeroRow heroRow = System.Data.Sheet.SheetHero [_forceSortieUnit.ForceSortieUnitInfos [sortieKeys [i]].UnitIndex];
			checkForceSortie.Add (heroRow.SkillHeroID, _forceSortieUnit.ForceSortieUnitInfos [sortieKeys [i]]);
		}

		for (int i = 0; i < _unitCandidateManager.CandidateUnits.Count; i++) {
			UnitCandidateInfo unitCandidate = _unitCandidateManager.CandidateUnits [i];

			if (checkDeckIconInfos.ContainsKey (unitCandidate.UnitModel.Index)) {
				_heroListHandler.ToggleHeroOnly(unitCandidate.UnitModel.ID, true);
			}

			SheetHeroRow heroRow = System.Data.Sheet.SheetHero [unitCandidate.UnitModel.Index];
			if (checkForceSortie.ContainsKey(heroRow.SkillHeroID))
			{
				_heroListHandler.ToggleHeroOnly(unitCandidate.UnitModel.ID, false);
			}
		}
	}

	Dictionary<int /* Unit Index */ , FormationDeckIconInfo> GetCheckSelectedUnits()
	{
		Dictionary<int /* Unit Index */ , FormationDeckIconInfo> checkDeckIconInfos = new Dictionary<int, FormationDeckIconInfo> ();

		List<int> formationIconKeys = _unitDeckPosManager.AllyFormationHandler.CurFormationIconInfos.Keys.ToList ();
		for (int i = 0; i < formationIconKeys.Count; i++) {
			FormationDeckIconInfo deckIconInfo = _unitDeckPosManager.AllyFormationHandler.CurFormationIconInfos [formationIconKeys [i]];
			if (deckIconInfo.UnitIndex != -1) {

				if (!checkDeckIconInfos.ContainsKey (deckIconInfo.UnitIndex)) {
					checkDeckIconInfos.Add (deckIconInfo.UnitIndex, deckIconInfo);
				} else {
					Debug.Log (string.Format ("!!!!! GetCheckSelectedUnits() OverLap UnitIndex : {0}", deckIconInfo.UnitIndex));
				}
			}
		}

		return checkDeckIconInfos;
	}

	protected bool GetCheckSelectedUnits(long id)
	{
		foreach (var each in _unitDeckPosManager.AllyFormationHandler.CurFormationIconInfos.Values)
		{
			if (each.UnitID == id)
				return true;
		}

		return false;
	}

    protected virtual void OnChangeAutoMode( bool autoMode )
    {
        _deployInfoManager.UserDeckData.SetAutoMode( autoMode);
        AutoAlignAllyFormationIcons( autoMode);
		_battleReadyPanel.SetVisibleDefaultModeButton(!IsAutoMode);		

        if( autoMode == false )
        {
            _unitDeckPosManager.AllyFormationHandler.ChangeShowTarget( true, BattleReadyTargetAimData.ShowTargetType.Ally );            
        }
        else
        {
            _unitDeckPosManager.AllyFormationHandler.ChangeShowTarget( false, BattleReadyTargetAimData.ShowTargetType.All );
        }
    }
    
	protected virtual void SetCurUserBattlePower()
	{
		List<float> combatPowerList = new List<float>();

		foreach (var each in _deployInfoManager.UserDeckData.GetCurUserBattleDecks().Values)
		{
			if (each.UnitInfo == null)
				continue;

			if (each.UnitInfo.Row.UnitType == "Truck")
				continue;

            combatPowerList.Add((float)each.UnitInfo.Power);
		}

		_unitDeckPosManager.UserBattlePower = GetTroopTotalPower(combatPowerList);
		_battleReadyPanel.UpdateAllyBattlePower( _unitDeckPosManager.UserBattlePower );

		SetDifficultyIcon(_unitDeckPosManager.UserBattlePower);
	}

	protected virtual void SetDifficultyIcon(int userBattlePower)
	{
		_battleReadyPanel.SetDifficultyTitle(userBattlePower, _sheetGameConfigRow.RecommendedPowerDifficulty);
	}

	protected virtual void SetCandidateUnits(){}

	protected virtual void InitCandidateUnits()
	{
		SetCandidateUnits();
	}

	protected virtual void SetLoadAllyDeckData()
	{
        for (int i = 0; i < _deployInfoManager.UserDeckData.UserBattleDeckFormations.Count; i++) {
			int waveIndex = i;
			UserBattleDeckFormation userBattleDeck = _deployInfoManager.UserDeckData.UserBattleDeckFormations [i];
			for (int j = 0; j < _deployInfoManager.UserDeckData.GetCurUserBattleDecks().Count; j++) {
				int posIndex = j;
				UserBattleDeck battleDeck = _deployInfoManager.UserDeckData.GetCurUserBattleDecks()[posIndex];
				UserDeckPosInfo deckPosInfo = userBattleDeck.UserDeckPositionInfos [posIndex];

				UserDefaultDeck userSaveDeck = _deployInfoManager.GetUserSaveDeck (waveIndex, posIndex);
				if (userSaveDeck != null && battleDeck.DeckUnitType != TroopDeployDefinitions.DeckUnitType.ForcedUnit) {
					if (CheckValidSaveUnitID (userSaveDeck.UnitID)) {
						HeroModel unitModel = _user.HeroInventory[userSaveDeck.UnitID];
						battleDeck.DeckUnitType = TroopDeployDefinitions.DeckUnitType.UserHave;
						battleDeck.UnitInfo = unitModel.Info;
						battleDeck.UnitInfo.FormationIndex = (sbyte)posIndex;
						battleDeck.UnitID = userSaveDeck.UnitID;
					}
				}

				UserDeckPosInfo userSaveDeckPos = _deployInfoManager.GetUserSaveDeckPosInfo (waveIndex, posIndex);
				if (userSaveDeckPos != null && battleDeck.DeckUnitType != TroopDeployDefinitions.DeckUnitType.FixedUnit) {
                    deckPosInfo.OffsetPosition = new Vector2(userSaveDeckPos.OffsetPosition.x, userSaveDeckPos.OffsetPosition.y);
                } else {
                    deckPosInfo.OffsetPosition = new Vector2(deckPosInfo.OffsetPosition.x, deckPosInfo.OffsetPosition.y);
                }
			}

            if(_deployInfoManager.UserDeckData.GetCurUserBattleDecks().Count < userBattleDeck.UserDeckPositionInfos.Count) {
                int startIndex = _deployInfoManager.UserDeckData.GetCurUserBattleDecks().Count;
                for(int j = startIndex;j< userBattleDeck.UserDeckPositionInfos.Count;j++) {
                    int posIndex = j;
                    UserDeckPosInfo deckPosInfo = userBattleDeck.UserDeckPositionInfos[posIndex];

                    deckPosInfo.OffsetPosition = new Vector2(deckPosInfo.OffsetPosition.x, deckPosInfo.OffsetPosition.y);
                }
            }
		}
	}

    protected void ResetAllyDeckOffsetPos()
    {
        if (_deployInfoManager != null)
            _deployInfoManager.IsRevisionIconPos = false;

        for (int i = 0; i < _deployInfoManager.UserDeckData.UserBattleDeckFormations.Count; i++) {
            //int waveIndex = i;
            UserBattleDeckFormation userBattleDeck = _deployInfoManager.UserDeckData.UserBattleDeckFormations[i];
            for(int j = 0;j< userBattleDeck.UserDeckPositionInfos.Count;j++) {
                int posIndex = j;
                UserDeckPosInfo deckPosInfo = userBattleDeck.UserDeckPositionInfos[posIndex];
                deckPosInfo.OffsetPosition = new Vector2(deckPosInfo.OffsetPosition.x, deckPosInfo.OffsetPosition.y);
            }
        }
    }

    protected virtual bool CheckValidSaveUnitID(long unitID)
	{
		for (int i = 0; i < _unitCandidateManager.CandidateUnits.Count; i++) {
			if (_unitCandidateManager.CandidateUnits [i].UnitModel.ID == unitID) {
				if (_forceSortieUnit.ForceSortieUnitInfos.Count > 0) {
					List<int> forceKeys = _forceSortieUnit.ForceSortieUnitInfos.Keys.ToList ();
					for (int j = 0; j < forceKeys.Count; j++) {
						SheetHeroRow forceHeroRow = System.Data.Sheet.SheetHero [_forceSortieUnit.ForceSortieUnitInfos [forceKeys [j]].UnitIndex];
						SheetHeroRow heroRow = System.Data.Sheet.SheetHero [_unitCandidateManager.CandidateUnits [i].UnitModel.Index];
						if (forceHeroRow.SkillHeroID == heroRow.SkillHeroID) {
							return false;
						}
					}
					return true;
				} else {
					return true;
				}
			}
		}

		return false;
	}

	protected override List<int> MakeEnemyTroopData()
	{
		List<int> enemyBattlePowers = base.MakeEnemyTroopData ();

		_battleReadyPanel.SetWave (_troopList.Length, enemyBattlePowers);

		return enemyBattlePowers;
	}

    protected List<int> MakeBaseEnemyTroopData()
    {
        return base.MakeEnemyTroopData();
    }

	protected void OnInitFormation()
	{
		UserBattleDeckFormation curDeckFormation = _deployInfoManager.UserDeckData.UserBattleDeckFormations [_deployInfoManager.CurWaveIndex];
		for (int i = 0; i < _deployInfoManager.UserDeckData.UserBattleDeckFormations.Count; i++) {
			UserBattleDeckFormation battleDeckFormation = _deployInfoManager.UserDeckData.UserBattleDeckFormations [i];
			if (battleDeckFormation.AllyFormationName == curDeckFormation.AllyFormationName) {
				for (int j = 0; j < battleDeckFormation.UserDeckPositionInfos.Count; j++) {
					battleDeckFormation.UserDeckPositionInfos [j].OffsetPosition = Vector2.zero;
				}
			}
		}

		_unitDeckPosManager.AllyFormationHandler.InitIconPosition(_isCenterGap);

		_deployInfoManager.UserDeckData.SaveCurDeckFile ();
	}

	[System.Obsolete("Obsoleted Method")]
    protected virtual void DisplayReward(int[] rewardIndexs)
    {
        if (rewardIndexs == null)
            return;

		_rewardIconList.Clear();

        var items = System.Data.Reward.GetRewardItemIndexCounts(rewardIndexs);
        var itemDrawer = new ItemDrawer(System.Data);
        var iconScale = Vector3.one * 0.75f;

        if (GetRewardExp() > 0)
        {
            var obj = Icon.Material.Create(_battleReadyPanel.GetTransformRewardIconBG());
            Icon.Material.ShowEXPReward(obj, GetRewardExp());
            obj.SetScale(iconScale);
        }

        foreach (var itemIndex in items.Keys)
        {
			int value = items[itemIndex];
			var obj = Icon.Material.Create(_battleReadyPanel.GetTransformRewardIconBG());
	        Icon.Material.ShowReward(obj, itemIndex, value);
	        obj.SetScale(iconScale);
        }

        if (MissionManager.Instance.CurrentMissionType == MissionContentType.SpecialMission)
        {
			if (System.Data.MissionListManager.CurrentMission != null)
			{
				var itemIndex = System.Data.MissionListManager.CurrentMission.rewardIndex;
				if (itemIndex != 0)
				{
					var obj = Icon.Material.Create(_battleReadyPanel.GetTransformRewardIconBG());
					Icon.Material.ShowReward(obj, itemIndex);
					obj.SetScale(iconScale);
				}
			}
        }
    }

    protected virtual void DisplayReward(int[] rewardIndexs, int[] rewardCounts, int[] rewardRate)
    {
	    if (rewardIndexs == null || rewardCounts == null || rewardIndexs.Length != rewardCounts.Length)
		    return;
	    if (rewardRate == null || rewardIndexs.Length != rewardRate.Length)
		    return;

	    _rewardIconList.Clear();
	    
	    var itemDrawer = new ItemDrawer(System.Data);
	    var iconScale = Vector3.one * 0.75f;

	    if (GetRewardExp() > 0)
	    {
		    var obj = Icon.Material.Create(_battleReadyPanel.GetTransformRewardIconBG());
		    Icon.Material.ShowEXPReward(obj, GetRewardExp());
		    obj.SetScale(iconScale);
	    }
	    
	    // Gold
	    int goldIndex = (int) CurrencyType.Gold;
	    if (rewardIndexs.Contains(goldIndex))
	    {
		    int arrayIndex = rewardIndexs.IndexOf(goldIndex);
		    int goldAmount = rewardCounts[arrayIndex];
		    var obj = Icon.Material.Create(_battleReadyPanel.GetTransformRewardIconBG());
		    Icon.Material.ShowReward(obj, goldIndex, goldAmount);
		    obj.SetScale(iconScale);
	    }
	    
	    // NeoStone
	    int neoStoneIndex = (int) CurrencyType.NeoStone;
	    if (rewardIndexs.Contains(neoStoneIndex))
	    {
		    int arrayIndex = rewardIndexs.IndexOf(neoStoneIndex);
		    int neoStoneAmount = rewardCounts[arrayIndex];
		    var obj = Icon.Material.Create(_battleReadyPanel.GetTransformRewardIconBG());
		    Icon.Material.ShowReward(obj, neoStoneIndex, neoStoneAmount);
		    obj.SetScale(iconScale);
	    }
	    
	    // Guild Point
	    int guildPointIndex = 5040;
	    if (rewardIndexs.Contains(guildPointIndex))
	    {
		    int arrayIndex = rewardIndexs.IndexOf(guildPointIndex);
		    int guildPointAmount = rewardCounts[arrayIndex];
		    var obj = Icon.Material.Create(_battleReadyPanel.GetTransformRewardIconBG());
		    Icon.Material.ShowReward(obj, guildPointIndex, guildPointAmount);
		    obj.SetScale(iconScale);
	    }

	    for (int i = 0; i < rewardIndexs.Length; i++)
	    {
		    if (rewardIndexs[i] == 0)
			    continue;

		    if (rewardRate[i] == 0)
			    continue;

		    if (rewardIndexs[i] == goldIndex || rewardIndexs[i] == neoStoneIndex || rewardIndexs[i] == guildPointIndex)
			    continue;

		    TypeUnit.Kind kind = _rewardModel.GetRewardType(rewardIndexs[i]);

		    if (kind == TypeUnit.Kind.Currency || kind == TypeUnit.Kind.Material)
		    {
			    var obj = Icon.Material.Create(_battleReadyPanel.GetTransformRewardIconBG());
			    Icon.Material.ShowReward(obj, rewardIndexs[i], rewardCounts[i]);
			    obj.SetScale(iconScale);
		    }
		    else if (kind == TypeUnit.Kind.Equipment)
		    {
			    var obj = Icon.Equipment.Create(_battleReadyPanel.GetTransformRewardIconBG());
			    Icon.Equipment.ShowReward(obj, rewardIndexs[i]);
			    obj.SetScale(iconScale);
		    }
            else if (kind == TypeUnit.Kind.ExclusiveEquipment)
		    {
			    var obj = Icon.ExclusiveEquipment.Create(_battleReadyPanel.GetTransformRewardIconBG());
			    Icon.ExclusiveEquipment.ShowReward(obj, rewardIndexs[i]);
			    obj.SetScale(iconScale);
		    }
		    else if (kind == TypeUnit.Kind.RandomEquipmentBox
		             || kind == TypeUnit.Kind.SelectEquipmentBox
		             || kind == TypeUnit.Kind.FixEquipmentBox
                     || kind == TypeUnit.Kind.RandomExclusiveEquipmentBox 
                     || kind == TypeUnit.Kind.FixExclusiveEquipmentBox)
		    {
			    var obj = Icon.Box.Create(_battleReadyPanel.GetTransformRewardIconBG());
			    Icon.Box.ShowReward(obj, rewardIndexs[i], rewardCounts[i]);
			    obj.SetScale(iconScale);
		    }
		    else
		    {
			    var itemIcon = itemDrawer.Draw(_battleReadyPanel.GetTransformRewardIconBG(), rewardIndexs[i], rewardCounts[i], false).target as UICommonBaseItemIcon;
			    itemIcon.SetScale(iconScale);
		    }
	    }

	    if (MissionManager.Instance.CurrentMissionType == MissionContentType.SpecialMission)
	    {
		    if (System.Data.MissionListManager.CurrentMission != null)
		    {
			    var itemIndex = System.Data.MissionListManager.CurrentMission.rewardIndex;
			    if (itemIndex != 0)
			    {
				    var obj = Icon.Material.Create(_battleReadyPanel.GetTransformRewardIconBG());
				    Icon.Material.ShowReward(obj, itemIndex);
				    obj.SetScale(iconScale);
			    }
		    }
	    }
    }

    protected virtual int[] GetRewardIndexs()
    {
        return null;
    }

    protected virtual int[] GetRewardCounts()
    {
	    return null;
    }

    protected virtual int[] GetRewardRate()
    {
	    return null;
    }

	protected virtual int GetRewardExp()
	{
		return 0;
	}

	protected virtual int GetRewardGold()
	{
		return 0;
	}

    protected virtual int GetRewardNeoStone()
    {
        return 0;
    }

    protected virtual int GetRewardGuildPoint()
    {
	    return 0;
    }

	protected virtual int GetCalcActionPoint()
	{
		return 0;
	}

	protected virtual string GetEnergyIconPath()
	{
		return System.Data.Currency.Get (CurrencyType.Energy).IconPath;
	}

	protected virtual void RequestStartBattlePacket()
	{
		SetPreRequestData ();
	}

	protected virtual void TransitBattleController()
	{
		_processor.Add(() =>
			{
                Transit<ForwardBattleLoadingController<EmptyTransitionBattleController>>(TransitionMethod.Forward, _fromController, () =>
                {
                    Kill();
                });
            });
	}

	protected virtual void SetEnableData()
	{
    
	}

    protected virtual void RefreshEnableData()
    {
        if (_topMenuController == null)
            _topMenuController = System.GetService<TopMenuController>();
        _topMenuController.Enable();
        _topMenuController.SetBackAction(OnClose);
        _topMenuController.SetTitle(GetTitleName());

        if (IsShowGuide())
            _topMenuController.SetGuideAction(OnStartGuide);

        _topMenuController.OnRefreshViewAction = () =>
        {
            InitDeployDeckInfo();
            RefreshDeckUnits();
            SetCurUserBattlePower();
        };

        if (!_initTopMenu)
        {
	        _currentTopMenuViewType = _topMenuController.CurrentMenuType;
	        _initTopMenu = true;
        }
        else
        {
	        _topMenuController.ChangeTopMenuType(_currentTopMenuViewType);
        }
    }

    public virtual void SetPreRequestData ()
	{

	}

	protected virtual int[] GetValidHeroes(int[] heroes){ return heroes; }

    protected virtual Type GetBattleBackwardType()
    {
        return null;
    }

    protected bool CheckPartyRegistered()
    {

        return false;
    }

    #endregion

    #region IDeckSettingObserver Methods

    protected void AttachDeckSettingOb(IDeckSettingObserver inputDeckSetting)
	{
		if(!_deckSettingObs.Contains(inputDeckSetting))
			_deckSettingObs.Add(inputDeckSetting);
	}

	protected void DetachDeckSettingOb(IDeckSettingObserver deckSettingOb)
	{
		if(_deckSettingObs.Contains(deckSettingOb))
			_deckSettingObs.Remove(deckSettingOb);
	}

	protected void ReleaseDeckSettingOb()
	{
		_deckSettingObs.Clear ();
	}

	protected void NotifyDeckSetting(int posIndex, TroopDeployDefinitions.DeckUnitType deckUnitType, long unitID, HeroInfo unitInfo, bool isSave = true)
	{
        for (int i = 0; i < _deckSettingObs.Count; i++) {
			_deckSettingObs [i].OnSetDeckInfo (_deployInfoManager.CurWaveIndex, posIndex, deckUnitType, unitID, unitInfo);
		}

		if(isSave)
			_deployInfoManager.UserDeckData.SaveCurDeckFile ();
		
        if( unitInfo != null) {
            SetCurUserBattlePower();
		}
	}

    public virtual void AutoAlignAllyFormationIcons( bool autoMode)
    {
        MapMoveBoundInfo mapMoveBound = _deployInfoManager.MapMoveBoundInfos[_deployInfoManager.CurWaveIndex];
        if( mapMoveBound.Moveable == false )
        {
            autoMode = false;
        }

        FormationDeckIconInfo[] sorted = _unitDeckPosManager.AllyFormationHandler.CurFormationIconInfos.Values.ToArray<FormationDeckIconInfo>();            
        IconSortData[] sorted2 = new IconSortData[sorted.Length];
        Dictionary<long, TroopDeployDefinitions.DeckUnitType> unitDeckTypes= new Dictionary<long, TroopDeployDefinitions.DeckUnitType>();
        if( autoMode == true )
        {
            Array.Sort(sorted, _iconComparer);
        }
        
        for( int i = 0; i < sorted.Length; i++ )
        {
            if( sorted[i] == null ) continue;

            sorted2[i] = new IconSortData();
            
            sorted2[i].unitID = sorted[i].UnitID;
            sorted2[i].unitInfo = sorted[i].UnitInfo;
			sorted2[i].posIndex = sorted[i].PosIndex;
            sorted2[i].add = false;
            
        }
        
        if( autoMode == true )
        {
            foreach( KeyValuePair<int, FormationDeckIconInfo> kvp in _unitDeckPosManager.AllyFormationHandler.CurFormationIconInfos )
            {
                
                TroopDeployDefinitions.DeckUnitType dUnitType = TroopDeployInfoManager.Instance.UserDeckData.GetCurUserBattleDecks()[kvp.Value.PosIndex].DeckUnitType;
                if( unitDeckTypes.ContainsKey(kvp.Value.UnitID) == false )
                    unitDeckTypes.Add( kvp.Value.UnitID, dUnitType );

                if( kvp.Value.UnitInfo == null )
                    continue;

                int waveIndex = TroopDeployInfoManager.Instance.CurWaveIndex;
                WaveFormationInfo waveFormation = TroopDeployInfoManager.Instance.FormaionMapInfo.GetWaveFormationInfo(waveIndex);
                FormationDefinitions.FormationTroopKind formationKind = waveFormation.allyTroopInfos[kvp.Value.PosIndex].FormationTroopKind;

                if ( dUnitType == TroopDeployDefinitions.DeckUnitType.FixedUnit ||  
                    dUnitType == TroopDeployDefinitions.DeckUnitType.PreSetSpawnUnit ||
					dUnitType == TroopDeployDefinitions.DeckUnitType.PreSetNPCUnit ||
					dUnitType == TroopDeployDefinitions.DeckUnitType.PreSetReinforcementUnit ||
                    dUnitType == TroopDeployDefinitions.DeckUnitType.FixedUnitControllable ||
					dUnitType == TroopDeployDefinitions.DeckUnitType.ForcedUnit ||
                    formationKind == FormationDefinitions.FormationTroopKind.Fixed)                
                    continue;

                for( int i =0 ; i < sorted2.Length; i++ )
                {
                    if( sorted2[i].unitInfo == kvp.Value.UnitInfo )
                    {
						sorted2[i].add = true;
                        break;
                    }
                }

                NotifyDeckSetting( -1, TroopDeployDefinitions.DeckUnitType.Empty, kvp.Value.UnitID, kvp.Value.UnitInfo, false );
            }
        }

        int meleeCount = 0;
        int rangeCount = 0;
        int emptyCount = 0;
        for( int i = 0; i < sorted.Length; i++ )
        {
            if( sorted2[i].unitInfo == null )
            {
                emptyCount++;
                continue;
            }
            
            if( autoMode == true )
            {
                if( sorted2[i].add == true )
                {
                    int validPosIndex = _deployInfoManager.GetValidPosIndex( _deployInfoManager.CurWaveIndex );
                    TroopDeployDefinitions.DeckUnitType deckUnitType;
                    if( unitDeckTypes.TryGetValue(sorted2[i].unitID, out deckUnitType) == false )
                    {
                        deckUnitType = TroopDeployDefinitions.DeckUnitType.UserHave;
                    }

                    NotifyDeckSetting( validPosIndex, deckUnitType, sorted2[i].unitID, sorted2[i].unitInfo, false );
                }
            }

            if(!TroopDeployInfoManager.Instance.UserDeckData.GetCurUserBattleDecks().ContainsKey(sorted[i].PosIndex))
                continue;

            int waveIndex = TroopDeployInfoManager.Instance.CurWaveIndex;
            WaveFormationInfo waveFormation = TroopDeployInfoManager.Instance.FormaionMapInfo.GetWaveFormationInfo(waveIndex);
            FormationDefinitions.FormationTroopKind formationKind = waveFormation.allyTroopInfos[sorted[i].PosIndex].FormationTroopKind;

            TroopDeployDefinitions.DeckUnitType dUnitType = TroopDeployInfoManager.Instance.UserDeckData.GetCurUserBattleDecks()[sorted[i].PosIndex].DeckUnitType;
            if( dUnitType == TroopDeployDefinitions.DeckUnitType.FixedUnit ||  
                dUnitType == TroopDeployDefinitions.DeckUnitType.PreSetSpawnUnit ||
                formationKind == FormationDefinitions.FormationTroopKind.Fixed)                
                continue;

            if( sorted2[i].unitInfo.IsMelee == true ) 
                meleeCount++;
            else 
                rangeCount++;
        }
        
        int emptyIndex = 0;
        int meleeIndex = 0;
        int rangeIndex = 0;
        foreach( KeyValuePair<int , FormationDeckIconInfo> kvp in _unitDeckPosManager.AllyFormationHandler.CurFormationIconInfos )
        {
            if( kvp.Value.UnitInfo == null )
            {
                
                if( autoMode == true )
                {
                    _unitDeckPosManager.AllyFormationHandler.SetUnitIconPositionByType(  kvp.Value.PosIndex, ref meleeIndex, ref rangeIndex, ref emptyIndex, meleeCount, rangeCount, emptyCount );
                }
                else
                {
                }
            }
            else
            {
                if (!TroopDeployInfoManager.Instance.UserDeckData.GetCurUserBattleDecks().ContainsKey(kvp.Value.PosIndex))
                    continue;

                int waveIndex = TroopDeployInfoManager.Instance.CurWaveIndex;
                WaveFormationInfo waveFormation = TroopDeployInfoManager.Instance.FormaionMapInfo.GetWaveFormationInfo(waveIndex);
                FormationDefinitions.FormationTroopKind formationKind = waveFormation.allyTroopInfos[kvp.Value.PosIndex].FormationTroopKind;

                TroopDeployDefinitions.DeckUnitType dUnitType = TroopDeployInfoManager.Instance.UserDeckData.GetCurUserBattleDecks()[kvp.Value.PosIndex].DeckUnitType;
                if( dUnitType == TroopDeployDefinitions.DeckUnitType.FixedUnit ||  
                    dUnitType == TroopDeployDefinitions.DeckUnitType.PreSetSpawnUnit ||
                    formationKind == FormationDefinitions.FormationTroopKind.Fixed)
                {
                }
                else
                {
                    if( autoMode == true )            
                        _unitDeckPosManager.AllyFormationHandler.SetUnitIconPositionByType(  kvp.Value.PosIndex, ref meleeIndex, ref rangeIndex, ref emptyIndex, meleeCount, rangeCount, emptyCount );
                }
            }
        }

        if (autoMode)
	        _unitDeckPosManager.AllyFormationHandler.UpdateIconPosition();
	
        RefreshIconOnClickEvent();
		TroopDeployInfoManager.Instance.UserDeckData.RefreshBattleDeckPosition (TroopDeployInfoManager.Instance.CurWaveIndex);
        _deployInfoManager.UserDeckData.SaveCurDeckFile ();
    }

    #endregion

    #region CallBack Methods

    protected virtual void OnClickCandidateUnit(long heroID, UICommonHeroIcon icon, UserModel user)
    {
	    if (!GetCheckSelectedUnits(heroID)) {
		    int validPosIndex = _deployInfoManager.GetValidPosIndex(_deployInfoManager.CurWaveIndex);
		    if (validPosIndex != -1) {
			    HeroInfo unitInfo = user.HeroInventory[heroID].Info;

			    _heroListHandler.ToggleHeroOnly(heroID);
			    
			    NotifyDeckSetting (validPosIndex, TroopDeployDefinitions.DeckUnitType.UserHave, heroID, unitInfo);
		    }
	    } else {
		    _heroListHandler.ToggleHeroOnly(heroID);

		    HeroInfo unitInfo = user.HeroInventory[heroID].Info;
		    NotifyDeckSetting (-1, TroopDeployDefinitions.DeckUnitType.Empty, heroID, unitInfo);
	    }
	    AutoAlignAllyFormationIcons( IsAutoMode );

	    _battleReadyPanel.ButtonEnable(CheckStartEnable());
    }

    protected virtual void OnClickAutoSelectHero()
    {
	    int validPosIndex = _deployInfoManager.GetValidPosIndex(_deployInfoManager.CurWaveIndex);

	    if (validPosIndex == -1)
	    {
		    UIFloatingMessagePopup.Show(_textModel.GetText(TextKey.Battle_Ready_Auto_Formation_Error_1));
	    }

	    while (validPosIndex != -1)
	    {
		    HeroInfo heroInfo = GetBestSelectHero();

		    if (heroInfo != null)
		    {
			    UICommonHeroIcon icon = _heroListHandler.GetHeroIcon(heroInfo.UID);
			    
				OnClickCandidateUnit(heroInfo.UID, icon, _user);
			    
				validPosIndex = _deployInfoManager.GetValidPosIndex(_deployInfoManager.CurWaveIndex);
		    }
		    else
		    {
			    break;
		    }
        }
    }

    protected virtual HeroInfo GetBestSelectHero(List<long> exceptList = null)
    {
	    HeroInfo currentInfo = null;
	    
	    foreach (var each in _user.HeroInventory)
	    {
		    if( exceptList != null && exceptList.Contains(each.Info.UID))
			    continue;

		    if (currentInfo == null )
		    {
			    if (!GetCheckSelectedUnits(each.Info.UID))
			    {
				    currentInfo = each.Info;
			    }
		    }
		    else
		    {
			    if (currentInfo.stat[(int) Stat.TotalCombatPower] < each.Info.stat[(int) Stat.TotalCombatPower] 
			        && !GetCheckSelectedUnits(each.Info.UID))
			    {
				    currentInfo = each.Info;
			    }    
		    }
	    }

	    return currentInfo;
    }

    protected virtual bool CheckStartEnable()
    {
        return _deployInfoManager.IsHaveUnit();
    }

	protected virtual void OnClickSelectedUnit(long unitID, int posIndex)
	{
		TroopDeployDefinitions.DeckUnitType deckUnitType = _deployInfoManager.UserDeckData.GetCurUserBattleDecks()[posIndex].DeckUnitType;
		if (deckUnitType == TroopDeployDefinitions.DeckUnitType.FixedUnit)
			return;
		
		int unitIndex = _unitDeckPosManager.AllyFormationHandler.CurFormationIconInfos [posIndex].UnitIndex;
		if (unitIndex != -1 && unitID != -1) {
			HeroInfo unitInfo = null;
			for (int i = 0; i < _unitCandidateManager.CandidateUnits.Count; i++) {
				if (_unitCandidateManager.CandidateUnits [i].UnitModel.Index == unitIndex) {
					unitInfo = _unitCandidateManager.CandidateUnits [i].UnitModel.Info;
					break;
				}
			}

			_heroListHandler.ToggleHeroOnly(unitID);

			if (deckUnitType == TroopDeployDefinitions.DeckUnitType.FriendUnit)
			{
				if (_hiredHeroPopup != null && _friendUnitInfo != null)
				{
					_hiredHeroPopup.UnSelectHiredHeroSlot();
					_hiredHeroPopup.ResetFocus();
					_friendUnitInfo = null;
				}
			}

			NotifyDeckSetting (-1, TroopDeployDefinitions.DeckUnitType.Empty, unitID, unitInfo);
		}

        AutoAlignAllyFormationIcons( IsAutoMode);

        _battleReadyPanel.ButtonEnable(CheckStartEnable());
    }

	protected void RemoveAllSelectedUnit()
	{
		var battleDecks = _deployInfoManager.UserDeckData.UserBattleDecks;
		int count = battleDecks.Count;

		for (int i = 0; i < count; i++)
		{
			for (int j = 0; j < battleDecks.Count; j++)
			{
				if (battleDecks[j].UnitID > 0)
				{
					OnClickSelectedUnit(battleDecks[j].UnitID, battleDecks[j].PosIndex);
					break;
				}
			}
		}
	}

	protected virtual void OnChangeWave(int waveIndex)
	{
		_deployInfoManager.CurWaveIndex = waveIndex;
		
		_unitDeckPosManager.AllyFormationHandler.InitChangeWaveData ();

		// UI Ally Formation Setting
		UIFormation allyUIFormation = _unitDeckPosManager.AllyFormationHandler.UIFormation;
		
		Rect allyMoveableAreaRectInWorld = _unitDeckPosManager.AllyFormationHandler.MoveableAreaRectInWorlds[waveIndex];
		MapMoveBoundInfo mapMoveBound = _deployInfoManager.MapMoveBoundInfos [waveIndex];

		if (mapMoveBound.Moveable) {
			allyUIFormation.OnDragAction = _battleReadyPanel.SetVisibleMoveArea;
			
			Vector3 moveMin = allyUIFormation.transform.InverseTransformPoint( allyMoveableAreaRectInWorld.min.x, allyMoveableAreaRectInWorld.min.y, 0 );
			Vector3 moveMax = allyUIFormation.transform.InverseTransformPoint( allyMoveableAreaRectInWorld.max.x, allyMoveableAreaRectInWorld.max.y, 0 );
			Rect localMoveableRect = Rect.MinMaxRect( moveMin.x, moveMin.y, moveMax.x, moveMax.y );
			_battleReadyPanel.DrawAreaHero.position = new Vector3( allyMoveableAreaRectInWorld.center.x, allyMoveableAreaRectInWorld.center.y, 0 );
			_battleReadyPanel.DrawAreaHero.sizeDelta = new Vector2( localMoveableRect.size.x, localMoveableRect.size.y );

			_battleReadyPanel.MoeveableRect.position = new Vector3( allyMoveableAreaRectInWorld.center.x, allyMoveableAreaRectInWorld.center.y, 0 );
			_battleReadyPanel.MoeveableRect.sizeDelta = new Vector2( localMoveableRect.size.x + 70, localMoveableRect.size.y + 70 );
		} else {
			_battleReadyPanel.MoeveableRect.gameObject.SetActive (false);
		}

		_unitDeckPosManager.SetDefaultDeckInfos (waveIndex, _deployInfoManager.EnemyBattleTroopInfos[waveIndex]);
		_unitDeckPosManager.SetCurrentWaveAllyUIDeckInfo (_deployInfoManager.UserDeckData.UserBattleDecks, _deployInfoManager.UserDeckData.UserBattleDeckFormations[waveIndex]);
		
		_unitDeckPosManager.AllyFormationHandler.InitIconPosition(!mapMoveBound.Moveable && _isCenterGap);
		
		_unitDeckPosManager.AllyFormationHandler.SetButtonAction(OnClickSelectedUnit, OnLongTouched);
		_unitDeckPosManager.EnemyFormationHandler.SetButtonAction(null, OnLongTouched);

        RefreshIconOnClickEvent();

		SetCurUserBattlePower ();

        AutoAlignAllyFormationIcons(IsAutoMode);

        _unitDeckPosManager.AllyFormationHandler.RefreshAllAim();
	}

    void RefreshIconOnClickEvent()
    {
    }

    protected virtual bool EnoughInventorySpace()
    {
        if (System.Data.StorageModel.IsEquipmentMaxCheck())
            return false;
        else if (System.Data.StorageModel.IsCardMaxCheck())
            return false;
        else if (System.Data.StorageModel.IsExclusiveEquipmentMaxCheck())
            return false;

        return true;
    }

    protected virtual bool OnStartBattle(int calcActionPoint, bool onlyForCheck)
	{
        if (CheckPartyRegistered()) {
            return false;
        }

        if (!EnoughInventorySpace())
            return false;

        var checker = new AssetChargeHandler(CurrencyType.Energy, GetCalcActionPoint());
	    if (!checker.Execute()) return false;

        if( _deployInfoManager.IsHaveUnit() || _deployInfoManager.IsTutorialUnit() )
		{
            if( onlyForCheck == false ) {
			    RequestStartBattlePacket();
            }
		}
		else
        {
			UIFloatingMessagePopup.Show(_textModel.GetText(TextKey.Popup_Select_Hero));
            return false;
        }

		if (!CheckFriendUnit())
		{
			return false;
		}

		if (!CheckEmptySlot(onlyForCheck))
		{
			return false;
		}

		return true;
	}
    
	protected virtual void OnDisableStartBattle()
	{
		Popup.Normal.Show( TextKey.Popup_Select_Hero);
	}

	protected virtual HeroInfo GetUnitInfoFriendUnit(int index)
	{
		if (_friendUnitInfo.Index == index)
			return _friendUnitInfo;
		
		return null;
	}

    public virtual void OnLongTouched(GameObject obj)
    {
        HeroInfo unitInfo = _unitDeckPosManager.FindAnyHeroInfo(obj);
        if (unitInfo == null)
            return;

		string heroType = unitInfo.Row.HeroType;
		if (heroType == "Truck")
			return;

        HeroModel hero = new HeroModel (System.Data, unitInfo);

        Debug.Log("[OnLongTouched] : " + hero.Name);

		ShowSimpleInfo(hero);
    }

	protected void ShowSimpleInfo( HeroModel hero, bool hideCombatPower = false )
	{
        var simpleInfo = new HeroSimpleInfoHandler(System.Data, false, hideCombatPower);
        simpleInfo.Show(hero);
        
#if USE_CHEAT
        simpleInfo.SetCombatDetailButton(() =>
        {
	        HeroInfoCombatView combatView = new HeroInfoCombatView();
	        combatView.Init(hero.Info);
        });
#endif
	}

	protected virtual void OnChangeClassTab(FormationClass formationClass)
	{
		
	}

	protected virtual void OnClose()
	{
		if (_isClosed)
			return;

		TroopDeployInfoManager.Exit ();

		_isClosed = true;

        if (OnBattleBackButton != null) {
            TransitToParent(GetBattleBackwardType(), Kill);

            OnBattleBackButton();
        } else {
            Terminate();

            Transition.Method = TransitionMethod.Backward;
            Transit(Transition, Kill);
        }
    }

    protected virtual void OnSuccessBattleStart(ResponseParam resParam)
    {
        if (_onBattleResultStart != null) {
            Terminate();
            Unload();
            _onBattleResultStart();
        } else {
            IsStartBattle = true;
			_battleReadyPanel.EnableStartButton();
        }
        
        MapMoveBoundInfo mapMoveBound = _deployInfoManager.MapMoveBoundInfos[_deployInfoManager.CurWaveIndex];
        if (mapMoveBound.Moveable)
	        _unitDeckPosManager.AllyFormationHandler.CancelRevisionPos();
    }

	protected virtual void OnFailBattleStart(ResponseParam resParam)
	{
		SetReloadHelperList();
	}

    public void RefreshStartButton()
    {
        PreCheckStartButton();
    }

    void OnPartyCancelRegister()
    {
        new ProtocolPartyWaitingCancel(System.Data, UITopDepthNoticeMessage.Instance.PartyTopNoticeInfo.SelectDifficulty, OnPartyWaitingCancelComplete).Execute();
    }

    void OnPartyWaitingCancelComplete(PartyWaitingCancelResponse res)
    {
        UITopDepthNoticeMessage.Instance.PartyTopNoticeInfo.CloseTopNotice();

        _battleReadyPanel.ButtonEnable(CheckStartEnable());
    }

    #endregion

    #region HiredHeroPopup

    protected virtual void OnHireButton()
	{
		if (_friendUnitInfo == null)
		{
			if (_deployInfoManager.GetValidPosIndex(_deployInfoManager.CurWaveIndex) < 0)
			{
				string message = _textModel.GetText(TextKey.Company_Mercenary_Employment_Information_4);
				UIFloatingMessagePopup.Show(message);

				return;
			}
		}

		if (_reloadHelperList)
		{
			_reloadHelperList = false;
			RequestHelperList();

			return;
		}

		if (_hiredHeroPopup == null)
		{
			_hiredHeroPopup = ResourceLoader.Instantiate<UICommonHiredHeroPopup>(Res.PREFAB.UICommonHiredHeroPopup);
            UIContainer.Instance.Add(_hiredHeroPopup);

			_hiredHeroPopup.SetLongPressAction(OnLongTouchedHireSlot);

			SetHireData();
			_hiredHeroPopup.SetButton(_textModel);
			_hiredHeroPopup.RefreshHiredHeroSlotList();

            _hiredHeroPopup.SetSelectAction(OnClickHireSelect);

            OnShowHirePopup();
		}
		else
		{
			_hiredHeroPopup.RefreshHiredHeroSlotList();
			OnShowHirePopup();
		}
	}

	protected virtual void RequestHelperList() {}
	protected virtual void SetHireData() {}

	protected virtual void SetReloadHelperList()
	{
		if (_friendUnitInfo != null)
		{
			NotifyDeckSetting(-1, TroopDeployDefinitions.DeckUnitType.Empty, _friendUnitInfo.UID, _friendUnitInfo, false);
			AutoAlignAllyFormationIcons(IsAutoMode);

			_friendUnitInfo = null;
			TroopDeployInfoManager.Instance.FriendUnitInfo = null;
		}

		_reloadHelperList = true;
		_dicFriendInfo.Clear();
	}

	protected virtual void OnClickHireSelect( int x )
	{
		_hiredHeroPopup.Hide();

		// Remove Previous Info
		if (_friendUnitInfo != null)
		{
			NotifyDeckSetting( -1, TroopDeployDefinitions.DeckUnitType.Empty, _friendUnitInfo.UID, _friendUnitInfo, false);
			_friendUnitInfo = null;
		}	
	}
	
	protected virtual void OnLongTouchedHireSlot( long userId, long heroId )
	{
		if (_dicFriendInfo.ContainsKey(userId))
		{
			HeroModel hero = new HeroModel (System.Data, _dicFriendInfo[userId]);
			ShowSimpleInfo(hero);
		}
		else
		{
			new HeroInfoTask(System.Data, userId, heroId, (res) =>
			{
				CompanyGrowthInfo companyGrowthInfo = new CompanyGrowthInfo(System.Data, res.heroGroup.companyGrowthInfo);
				
				HeroInfo info = new HeroInfo(System.Data.BattleContext, res.heroGroup.battleHeroList[0], companyGrowthInfo);
				_dicFriendInfo.Add(userId, info);


				HeroModel hero = new HeroModel (System.Data, info);
				ShowSimpleInfo(hero);

			}).Execute();
		}
	}

	protected virtual void OnShowHirePopup()
	{
		_hiredHeroPopup.Show();
	}

	protected virtual bool CheckFriendUnit()
	{
		return true;
	}

	protected virtual bool CheckEmptySlot(bool onlyForCheck)
	{
		if (_emptyCheck)
			return true;
		
		int validIndex = _deployInfoManager.GetValidPosIndex(_deployInfoManager.CurWaveIndex);
        int remainHeroCount = _heroModels.Count - _deployInfoManager.GetDeckCount() - GetUnusedCount();

        if (AddFreeAvailableHero())
	        remainHeroCount--;

        if (validIndex >= 0 && remainHeroCount > 0)
		{
			if (onlyForCheck)
			{
				Popup.Select.Show(_textModel.GetText(TextKey.Battle_Ready_Hero_Shortage_Cirfirm_Popup),
					_textModel.GetText(TextKey.UI_Text_181),
					_textModel.GetText(TextKey.UI_Text_2),
					null,
					() =>
					{
						_emptyCheck = true;
						_battleReadyPanel.ButtonStart.onClick.Invoke();
					});
				
				return false;
			}
		}

		return true;
	}

	protected virtual int GetUnusedCount()
	{
		return 0;
	}

    protected virtual int OnAddPowerValue(HeroInfo heroInfo)
    {
        return 0;
    }

    protected virtual float OnAddAttackValue(HeroInfo heroInfo)
    {
        return 0f;
    }

    #endregion

    #region ApplyEffect

    public virtual int GetApplyEffectType()
	{
		return 1;
	}

	#endregion

    #region ClearResultDirecting

	private void CreateResultPopup()
	{
		var popup = ResourceLoader.Instantiate<UIAutoClearRewardPopup>(Res.PREFAB.UIAutoClearRewardPopup);
		UIContainer.Instance.Add(popup);
		
		int exp = _user.userData.exp - _user.GetPrevUserExp();
		string titleText = GetTitleName();
		string expString = "";
		if (exp > 0)
			expString = string.Format(_textModel.GetText(TextKey.UI_Text_160), exp);
		int gold = GetRewardGold();
		
		popup.Init(titleText, gold, expString, _textModel.GetText(TextKey.UI_Text_2), null, _reward.ToArray());
	}

	protected virtual void LevelupPopupDirecting()
	{
		var user = System.Data.User;
		var userLevelModel = System.Data.UserLevel;

		var originExp = user.GetPrevUserExp();
		var originLevel = userLevelModel.GetLevel(originExp);
		var currentExp = user.userData.exp;
		var currentLevel = userLevelModel.GetLevel(currentExp);

		if (currentLevel <= originLevel)
		{
            CreateResultPopup();
            return;
		}

		new Directing_Wait(
			() => {
			var sheetExp = System.Sheet.SheetUserExp;
			var textModel = System.Data.Text;
			_levelUpPopup = UIUserLevelUpPopup.Create();

			string currentLevelText = string.Format(textModel.GetText(TextKey.Company_User_Level), currentLevel);

			_levelUpPopup.SetCurrentLevelText(currentLevelText);

			int originEnergy = sheetExp.MaxEnergy[originLevel];
			int currentEnergy = sheetExp.MaxEnergy[currentLevel];
			string increaseEnergyText = string.Format(textModel.GetText(TextKey.Battle_Result_UserLv_Up_Energy_Increase_Text), (currentEnergy - originEnergy));
			_levelUpPopup.SetIncreaseEnergyText(increaseEnergyText);

			var drawer = new ItemDrawer(System.Data);
			drawer.Draw(_levelUpPopup.ItemTrnRoot, System.Data.User.userLevelupReward);

			_levelUpPopup.SetButtonAction(() => {
                CreateResultPopup();
            });
			_levelUpPopup.Show();

			Camera camera = _levelUpPopup.GetComponentInChildren<Camera>();
			if( camera != null )
			{
				camera.depth = 80;
			}

			System.Data.User.TakeLevelupRewardToStorage();

		}, 0.5f).Execute();
	}

	protected virtual void SearchPointAppearDirecting()
	{

	}

	private void OnActionRedrawReward(RewardData rewards)
	{

	}

    #endregion

    #region Guide Process

	public virtual void OnStartGuide()
	{
		NotificationWidgetController.Instance?.ShowQuestNotiWidget(false);
		
		Step1_HeroList();
	}

	public void Step1_HeroList()
	{
		RectTransform rectTrans = _heroListHandler.Guide_HeroList.rectTransform();
		
		var highlighter = Directing_UIHighlight.ProcessFocus(rectTrans,false, () =>
		{
			Step2_AudoSelect();
		});

		var textModel = System.Data.Text;
		highlighter.AddText(rectTrans, new Vector2(600, 70), BoxEdge.Right, textModel.GetText(TextKey.SceneGuide_BattleReady_01));
	}

	public void Step2_AudoSelect()
	{
		if (_battleReadyPanel.Guide_AutoSelect == null)
		{
			Step3_HeroArea();
			return;
		}
		
		RectTransform rectTrans = _battleReadyPanel.Guide_AutoSelect.rectTransform();
		
		var highlighter = Directing_UIHighlight.ProcessFocus(rectTrans,false, () =>
		{
			Step3_HeroArea();
		});

		var textModel = System.Data.Text;
		highlighter.AddText(rectTrans, new Vector2(600, 70), BoxEdge.RightTEdge, textModel.GetText(TextKey.SceneGuide_BattleReady_03));
	}

	public void Step3_HeroArea()
	{
		RectTransform rectTrans = _battleReadyPanel.Guide_HeroArea.rectTransform();
		
		var highlighter = Directing_UIHighlight.ProcessFocus(rectTrans, false, (() => { Step4_AllyMoveableArea(); }));
		var textModel = System.Data.Text;
		highlighter.AddText(rectTrans, new Vector2(600, 70), BoxEdge.Bottom, textModel.GetText(TextKey.SceneGuide_BattleReady_02));
	}
	
	public void Step4_AllyMoveableArea()
	{
		MapMoveBoundInfo mapMoveBound = _deployInfoManager.MapMoveBoundInfos[_deployInfoManager.CurWaveIndex];

		if (mapMoveBound.Moveable)
		{
			RectTransform rectTrans = _battleReadyPanel.Guide_AllyMoveableArea.rectTransform();
			var highlighter = Directing_UIHighlight.ProcessFocus(rectTrans, false, (() => { Step5_PossessionPassive(); }));
			var textModel = System.Data.Text;
			highlighter.AddText(rectTrans, new Vector2(500, 70), BoxEdge.Bottom, textModel.GetText(TextKey.SceneGuide_BattleReady_07));
		}
		else
		{
			RectTransform rectTrans = _battleReadyPanel.Guide_PossessionPassive.rectTransform();
			var highlighter = Directing_UIHighlight.ProcessFocus(rectTrans, false, (() => { Step6_Reward(); }));
			var textModel = System.Data.Text;
			highlighter.AddText(rectTrans, new Vector2(500, 70), BoxEdge.BottomLEdge, textModel.GetText(TextKey.SceneGuide_BattleReady_09));
		}
	}
	
	public void Step5_PossessionPassive()
	{
		RectTransform rectTrans = _battleReadyPanel.Guide_PossessionPassive.rectTransform();
             
		var highlighter = Directing_UIHighlight.ProcessFocus(rectTrans, false, (() =>
		{
			Step6_Reward();
		}));
     
		var textModel = System.Data.Text;
		highlighter.AddText(rectTrans, new Vector2(500, 70), BoxEdge.BottomLEdge, textModel.GetText(TextKey.SceneGuide_BattleReady_09));
	}
	
	public void Step6_Reward()
	{
		RectTransform rectTrans = _battleReadyPanel.Guide_Reward.rectTransform();

		var highlighter = Directing_UIHighlight.ProcessFocus(rectTrans, false, () =>
		{
			NotificationWidgetController.Instance?.ShowQuestNotiWidget(true);
		});
     
		var textModel = System.Data.Text;
		highlighter.AddText(rectTrans, new Vector2(500, 70), BoxEdge.TopLEdge, textModel.GetText(TextKey.SceneGuide_BattleReady_10));
	}
	

    #endregion
	
	#region IBackKeyMethod

	public override void ExecuteBackKey()
	{
		_topMenuController.Back();
	}

	#endregion
}
