using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Controller;
using System.Linq;

public abstract class BattleReadyBaseDataController : BaseController<GameSystem> 
{
    #region Definitions

    public enum EnemyTroopType
    {
        NormalSheet,
        BattleHeroData,
        AloneMissionEnemy,
    }

    #endregion

    #region Variables

    protected TroopDeployInfoManager _deployInfoManager;

	protected int[] _troopList = null;

	protected ForceSortieUnit _forceSortieUnit = new ForceSortieUnit();
	protected List<FormationFixedHeroInfo> _fixedUnitInfos = new List<FormationFixedHeroInfo> ();
	protected Dictionary<int /* Pos Index */, PreSettingDeckUnitInfo> _preSetttingDeckUnitInfos = new Dictionary<int, PreSettingDeckUnitInfo>();

	protected UnitDeckPosManager _unitDeckPosManager = new UnitDeckPosManager();

	protected bool _isFirstStart = false;
    protected EnemyTroopType _enemyType = EnemyTroopType.NormalSheet;
    protected SheetGameConfigRow _sheetGameConfigRow = null;

    #endregion

    #region Properties

    #endregion

    #region BaseController Methods

    protected override void OnInit()
	{
		base.OnInit ();

		TroopDeployInfoManager.Exit ();
		_deployInfoManager = TroopDeployInfoManager.Instance;
		_isFirstStart = false;
		_sheetGameConfigRow = System.Data.Sheet.SheetGameConfig[1];
	}

	#endregion

	#region Methods

	protected virtual void LoadAllyTroopData()
	{
		// Default User BattleDeck Setting
		_deployInfoManager.UserDeckData.UserBattleDecks.Clear();
		_deployInfoManager.UserDeckData.UserBattleDeckFormations.Clear();
		for (int i = 0; i < _deployInfoManager.FormaionMapInfo.WaveFormationInfos.Count; i++) {
			int waveIndex = i;
			WaveFormationInfo waveFormation = _deployInfoManager.FormaionMapInfo.GetWaveFormationInfo (waveIndex);
			UserBattleDeckFormation inputBattleDeckFormation = new UserBattleDeckFormation ();
			inputBattleDeckFormation.WaveIndex = waveIndex;
			inputBattleDeckFormation.AllyFormationName = waveFormation.AllyFormationName;

			int curFixedIndex = 0;
			for (int j = 0; j < waveFormation.allyTroopInfos.Count; j++) {
				int posIndex = j;
				// Battle Deck
				if (waveIndex == 0) {
					SetAllPreSettingUnit(posIndex, curFixedIndex, waveFormation.allyTroopInfos [j].FormationTroopKind);
				}

				if (waveFormation.allyTroopInfos[j].FormationTroopKind == FormationDefinitions.FormationTroopKind.FixedControllable)
					curFixedIndex++;

				// Battle Pos Info
				UserDeckPosInfo inputDeckPosInfo = new UserDeckPosInfo ();
				inputDeckPosInfo.PosIndex = posIndex;
				inputDeckPosInfo.UnitPosition = new Vector2(waveFormation.allyTroopInfos [j].UnitPosition.x, waveFormation.allyTroopInfos [j].UnitPosition.z);
				inputDeckPosInfo.OffsetPosition = Vector2.zero;

				inputBattleDeckFormation.UserDeckPositionInfos.Add (posIndex, inputDeckPosInfo);
			}
			_deployInfoManager.UserDeckData.SetUserBattleDeck (waveIndex, inputBattleDeckFormation);
		}
	}

	protected void SetAllPreSettingUnit(int posIndex, int curFixedIndex, FormationDefinitions.FormationTroopKind formationTroopKind)
	{
		ForceSortieUnit curForceSortie = _forceSortieUnit;

		UserBattleDeck inputUserBattle = new UserBattleDeck ();
		inputUserBattle.PosIndex = posIndex;
		switch (formationTroopKind) {
		case FormationDefinitions.FormationTroopKind.Fixed:
			{
				inputUserBattle.DeckUnitType = TroopDeployDefinitions.DeckUnitType.FixedUnit;
				if (_fixedUnitInfos.Count > curFixedIndex) {
					inputUserBattle.UnitID = 0;
					inputUserBattle.UnitInfo = new HeroInfo(System.Data.BattleContext, _fixedUnitInfos[curFixedIndex].HeroIndex, 
						_fixedUnitInfos[curFixedIndex].HeroLevel, _fixedUnitInfos[curFixedIndex].DifficultyType);
					inputUserBattle.UnitInfo.FormationIndex = (sbyte)posIndex;
					inputUserBattle.IsEnableTouch = false;
                    OnFixedUnitMaked( inputUserBattle.UnitInfo);

					curFixedIndex++;
				}
			}
			break;
		case FormationDefinitions.FormationTroopKind.FixedControllable:
		{
			inputUserBattle.DeckUnitType = TroopDeployDefinitions.DeckUnitType.FixedUnitControllable;
			
			if (_fixedUnitInfos.Count > curFixedIndex) {
				inputUserBattle.UnitID = 0;
				inputUserBattle.UnitInfo = new HeroInfo(System.Data.BattleContext, _fixedUnitInfos[curFixedIndex].HeroIndex, 
					_fixedUnitInfos[curFixedIndex].HeroLevel, _fixedUnitInfos[curFixedIndex].DifficultyType);
				inputUserBattle.UnitInfo.FormationIndex = (sbyte)posIndex;
				inputUserBattle.IsEnableTouch = true;
				OnFixedUnitMaked( inputUserBattle.UnitInfo);

				curFixedIndex++;
			}
			break;
		}
		default:
			inputUserBattle.DeckUnitType = TroopDeployDefinitions.DeckUnitType.Empty;
			break;
		}

		// PreSetting Unit
		if (_preSetttingDeckUnitInfos.Count > 0) {
			if (_preSetttingDeckUnitInfos.ContainsKey (posIndex)) {
				PreSettingDeckUnitInfo preSetDeckUnit = _preSetttingDeckUnitInfos [posIndex];
				inputUserBattle.DeckUnitType = preSetDeckUnit.DeckUnitType;
				inputUserBattle.UnitID = 0;
				inputUserBattle.UnitInfo = preSetDeckUnit.UnitInfo;

				switch (preSetDeckUnit.DeckUnitType) {
				case TroopDeployDefinitions.DeckUnitType.PreSetSpawnUnit:
					inputUserBattle.UnitInfo.UnitType = UnitType.Spawner;
					break;
				case TroopDeployDefinitions.DeckUnitType.PreSetReinforcementUnit:
					inputUserBattle.UnitInfo.IsReinforcement = true;
					inputUserBattle.UnitInfo.UnitType = UnitType.Reinforcement;
					break;
				}

				switch (preSetDeckUnit.DeckUnitType) {
				case TroopDeployDefinitions.DeckUnitType.PreSetNPCUnit:
				case TroopDeployDefinitions.DeckUnitType.PreSetReinforcementUnit:
				case TroopDeployDefinitions.DeckUnitType.PreSetSpawnUnit:
					inputUserBattle.IsEnableTouch = false;
					break;
				}
			}
		}

		if (curForceSortie != null && curForceSortie.ForceSortieUnitInfos.Count > 0) {
			if (curForceSortie.ForceSortieUnitInfos.ContainsKey (posIndex)) {
				inputUserBattle.DeckUnitType = TroopDeployDefinitions.DeckUnitType.ForcedUnit;
				inputUserBattle.UnitID = 0;
				inputUserBattle.UnitInfo = new HeroInfo(System.Data.BattleContext, curForceSortie.ForceSortieUnitInfos[posIndex].UnitIndex, 
					curForceSortie.ForceSortieUnitInfos[posIndex].Level, curForceSortie.ForceSortieUnitInfos[posIndex].DifficultyType);

				inputUserBattle.UnitInfo.FormationIndex = (sbyte)posIndex;
				inputUserBattle.UnitInfo.IsReinforcement = true;
				inputUserBattle.UnitInfo.UnitType = UnitType.Reinforcement;
				inputUserBattle.IsEnableTouch = false;
			}
		}

		_deployInfoManager.UserDeckData.UserBattleDecks.Add (posIndex, inputUserBattle);
	}

	protected virtual List<int> MakeEnemyTroopData()
	{
		List<int> enemyBattlePowers = new List<int> ();
		if (!TroopDeployInfoManager.isLoadTroopDeploy) {
			_deployInfoManager.EnemyBattleTroopInfos.Clear ();

			if(_troopList == null || _troopList.Length == 0)
			{
				_deployInfoManager.EnemyBattleTroopInfos.Add(0, new List<EnemyBattleTroopInfo>(){ });
			}

			for (int i = 0; i < _troopList.Length; i++)
			{
				int levelOffset = GetTroopLevelOffset (i); 
				int difficultyOffset = GetTroopDifficultyTypeOffset (i);

				int curTroopIndex = _troopList [i];

				BattleHeroGroupData battleHeroGroupData = GetBattleHeroGroupData(i);
				int unitCount = 0;
				HeroInfo[] inputUnitInfos = null;
				WaveFormationInfo curWaveFormation = _deployInfoManager.FormaionMapInfo.WaveFormationInfos [i];

				Dictionary<int /* PosIndex */, TroopDeployPositionInfo> battleUnitPositions = null;
				if (battleHeroGroupData != null) {
					unitCount = battleHeroGroupData.battleHeroList.Length;
					battleUnitPositions = GetBattleUnitPosition (i);
				} else {
                    int[] unitIndex = null;
                    int[] unitLevels = null;
                    int[] unitDifficultyTypes = null;
                    if (TroopDeployInfoManager.isAllyWave) {
                        unitIndex = GetWaveTroopHeroes(i);
                        unitLevels = GetWaveTroopUnitLevels(i);
                        unitDifficultyTypes = GetWaveTroopUnitDifficultyTypes(i);
                    } else {
                        unitIndex = GetCurTroopHeroes(curTroopIndex);
                        unitLevels = GetCurTroopUnitLevels(curTroopIndex);
                        unitDifficultyTypes = GetCurTroopUnitDifficultyTypes(curTroopIndex);
                    }

                    inputUnitInfos = GetUnitInfos (unitIndex, unitLevels, unitDifficultyTypes, levelOffset, difficultyOffset);
					unitCount = inputUnitInfos.Length;

					if (curWaveFormation.EnemyAutoFormation)
						battleUnitPositions = GetEnemyAutoPosition(inputUnitInfos, curWaveFormation);
				}

				List<EnemyBattleTroopInfo> battleTroopInfos = new List<EnemyBattleTroopInfo> ();

				int unitTotalPower = 0;
				List<float> combatPowerList = new List<float>();

				for (int j = 0; j < unitCount; j++) {
					EnemyBattleTroopInfo inputBattleTroop = new EnemyBattleTroopInfo ();
					HeroInfo unitInfo = null;
					if (battleHeroGroupData == null) {
						unitInfo = inputUnitInfos[j];
					} else
					{
						CompanyGrowthInfo companyGrowthInfo = new CompanyGrowthInfo(System.Data, battleHeroGroupData.companyGrowthInfo);
						unitInfo = new HeroInfo (System.Data.BattleContext, battleHeroGroupData.battleHeroList [j], companyGrowthInfo);
					}

					combatPowerList.Add((float)unitInfo.Power);

					inputBattleTroop.PosIndex = j;
					inputBattleTroop.UnitInfo = unitInfo;
					inputBattleTroop.UnitInfo.FormationIndex = (sbyte)inputBattleTroop.PosIndex;

					FormationTroopInfo formationTroop = curWaveFormation.enemyTroopInfos [j];

					inputBattleTroop.UnitPosition = formationTroop.UnitPosition;
					inputBattleTroop.UnitPositionTo = formationTroop.UnitPositionTo;
					
					if (battleUnitPositions != null && battleUnitPositions.Count > j)
					{
						var battlePos = battleUnitPositions.Values.ElementAt(j);

						inputBattleTroop.UnitInfo.FormationIndex = (sbyte)inputBattleTroop.PosIndex;
						inputBattleTroop.OffsetPosition = new Vector2(battlePos.OffsetPosX, battlePos.OffsetPosY);

						FormationTroopInfo formationTroopInfoCustom = curWaveFormation.enemyTroopInfos[battlePos.PosIndex];
						inputBattleTroop.UnitPosition = formationTroopInfoCustom.UnitPosition;
						inputBattleTroop.UnitPositionTo = formationTroopInfoCustom.UnitPositionTo;
						
						Debug.Log(string.Format("battleUnitPositions[j].OffsetPosX : {0}", battlePos.OffsetPosX));
					}
					
					battleTroopInfos.Add (inputBattleTroop);
				}

				unitTotalPower = GetTroopTotalPower(combatPowerList);
				enemyBattlePowers.Add (unitTotalPower);

				_deployInfoManager.EnemyBattleTroopInfos.Add (i, battleTroopInfos);
			}
		}
		else
		{
			for (int i = 0; i < _troopList.Length; i++)
			{
				int levelOffset = GetTroopLevelOffset(i);
				int difficultyOffset = GetTroopDifficultyTypeOffset(i);

				int curTroopIndex = _troopList[i];

				BattleHeroGroupData battleHeroGroupData = GetBattleHeroGroupData(i);
				int unitCount = 0;
				HeroInfo[] inputUnitInfos = null;
				if (battleHeroGroupData != null)
				{
					unitCount = battleHeroGroupData.battleHeroList.Length;
				}
				else
				{
					int[] unitIndex = GetCurTroopHeroes(curTroopIndex);
					int[] unitLevels = GetCurTroopUnitLevels(curTroopIndex);
					int[] unitDifficultyTypes = GetCurTroopUnitDifficultyTypes(curTroopIndex);
					inputUnitInfos = GetUnitInfos(unitIndex, unitLevels, unitDifficultyTypes, levelOffset, difficultyOffset);
					unitCount = inputUnitInfos.Length;
				}

				WaveFormationInfo curWaveFormation = _deployInfoManager.FormaionMapInfo.WaveFormationInfos[i];

				int unitTotalPower = 0;
				List<float> combatPowerList = new List<float>();

				for (int j = 0; j < unitCount; j++)
				{
					HeroInfo unitInfo = null;
					if (battleHeroGroupData == null)
					{
						unitInfo = inputUnitInfos[j];
					}
					else
					{
						CompanyGrowthInfo companyGrowthInfo = new CompanyGrowthInfo(System.Data, battleHeroGroupData.companyGrowthInfo);
						unitInfo = new HeroInfo(System.Data.BattleContext, battleHeroGroupData.battleHeroList[j], companyGrowthInfo);
					}

					combatPowerList.Add((float) unitInfo.Power);
				}

				unitTotalPower = GetTroopTotalPower(combatPowerList);

				enemyBattlePowers.Add(unitTotalPower);
			}
		}

		float[] weight = _sheetGameConfigRow.RecommendedPowerWeight;
		int weightIndex = Mathf.Min(enemyBattlePowers.Count - 1, weight.Length - 1);
		weightIndex = Mathf.Max(weightIndex, 0);
		int sumBattlePower = 0;
		foreach (var each in enemyBattlePowers)
		{
			sumBattlePower += each;
		}

		float revisionCombatPower = (float)sumBattlePower * weight[weightIndex];

		for (int i = 0; i < enemyBattlePowers.Count; i++)
			enemyBattlePowers[i] = (int)revisionCombatPower;

#if USE_CHEAT
		if (enemyBattlePowers != null && enemyBattlePowers.Count > 0)
		{
			NewBattleCore.FormulaManager.Instance.FormulaLog = true;
			NewBattleCore.FormulaManager.Instance.PrintToChatting("EnemyBattlePower " + enemyBattlePowers[0]);
			NewBattleCore.FormulaManager.Instance.FormulaLog = false;
		}
#endif
		return enemyBattlePowers;
	}

	protected PreSettingDeckUnitInfo AddPreSetUnits(int posIndex, TroopDeployDefinitions.DeckUnitType deckUnitType, int unitIndex, int unitLevel, int difficultyType)
	{
		PreSettingDeckUnitInfo inputPreSetDeckUnit = new PreSettingDeckUnitInfo ();
		inputPreSetDeckUnit.DeckUnitType = deckUnitType;
		inputPreSetDeckUnit.PosIndex = posIndex;

		inputPreSetDeckUnit.UnitIndex = unitIndex;

		inputPreSetDeckUnit.Level = unitLevel;
		inputPreSetDeckUnit.DifficultyType = difficultyType;
        inputPreSetDeckUnit.UnitInfo = new HeroInfo (System.Data.BattleContext, unitIndex, inputPreSetDeckUnit.Level, inputPreSetDeckUnit.DifficultyType);

		_preSetttingDeckUnitInfos.Add (posIndex, inputPreSetDeckUnit);

		return inputPreSetDeckUnit;
	}

	protected PreSettingDeckUnitInfo AddPreSetUnits(int posIndex, TroopDeployDefinitions.DeckUnitType deckUnitType, HeroInfo unitInfo)
	{
		PreSettingDeckUnitInfo inputPreSetDeckUnit = new PreSettingDeckUnitInfo ();
		inputPreSetDeckUnit.DeckUnitType = deckUnitType;
		inputPreSetDeckUnit.PosIndex = posIndex;

		inputPreSetDeckUnit.UnitIndex = unitInfo.Index;

		inputPreSetDeckUnit.Level = unitInfo.Level;
		inputPreSetDeckUnit.DifficultyType = unitInfo.DifficultyType;
		inputPreSetDeckUnit.UnitInfo = unitInfo;

		_preSetttingDeckUnitInfos.Add (posIndex, inputPreSetDeckUnit);

		return inputPreSetDeckUnit;
	}

	protected virtual int GetTroopTotalPower( List<float> combatPowerList )
	{
		if (combatPowerList.Count == 0)
			return 0;

		float sum = 0;

		for (int i = 0; i < combatPowerList.Count; i++)
			sum += combatPowerList[i];

		return Mathf.RoundToInt(sum);
	}

	protected virtual void InitTroopListData()
	{

	}

	protected virtual void MakeForceSortieUnit()
	{

	}

	protected virtual void MakeFixedDeckData()
	{

	}

	protected virtual void MakePreSettingUnitData()
	{

	}

	protected virtual int[] GetCurTroopHeroes(int troopIndex)
	{
		return null;
	}

	protected virtual int[] GetCurTroopUnitLevels(int troopIndex)
	{
		return null;
	}

	protected virtual int[] GetCurTroopUnitDifficultyTypes(int troopIndex)
	{
		return null;
	}

    protected virtual int[] GetWaveTroopHeroes(int waveIndex)
    {
        return null;
    }

    protected virtual int[] GetWaveTroopUnitLevels(int waveIndex)
    {
        return null;
    }

    protected virtual int[] GetWaveTroopUnitDifficultyTypes(int waveIndex)
    {
        return null;
    }

    protected virtual BattleHeroGroupData GetBattleHeroGroupData(int waveIndex)
	{
		return null;
	}

	// PVP Position
	protected virtual Dictionary<int /* PosIndex */, TroopDeployPositionInfo> GetBattleUnitPosition(int waveIndex)
	{
		return null;
	}

	// PVE Position
	protected virtual Dictionary<int /* PosIndex */, TroopDeployPositionInfo> GetEnemyAutoPosition(HeroInfo[] heroInfo, WaveFormationInfo waveformationInfo)
	{
		Dictionary<int /* PosIndex */, TroopDeployPositionInfo> retValues = new Dictionary<int, TroopDeployPositionInfo> ();

		int meleeCount = 0;
		int rangeCount = 0;

		for (int i = 0; i < heroInfo.Length; i++)
		{
			if (heroInfo[i].IsMelee)
				meleeCount++;
			else
				rangeCount++;
		}

		int emptyIndex = 0;
		int meleeIndex = 0;
		int rangeIndex = 0;

		for (int i = 0; i < heroInfo.Length; i++)
		{
			if (_unitDeckPosManager.EnemyFormationHandler == null)
				continue;

			Vector3 pos = _unitDeckPosManager.EnemyFormationHandler.GetUnitIconOffsetPositionByType(heroInfo[i].IsMelee, i, ref meleeIndex, ref rangeIndex, ref emptyIndex, meleeCount, rangeCount, 0);

			Vector3 unitPosition = waveformationInfo.enemyTroopInfos[i].UnitPosition;
			pos.x -= unitPosition.x;
			pos.z -= unitPosition.z;

			TroopDeployPositionInfo inputPosition = new TroopDeployPositionInfo();
			inputPosition.PosIndex = i;
			inputPosition.OffsetPosX = pos.x;
			inputPosition.OffsetPosY = pos.z;

			retValues.Add(i, inputPosition);
		}

		return retValues;
	}

	protected virtual int GetTroopLevelOffset(int waveIndex)
	{
		return 0;
	}

	protected virtual int GetTroopDifficultyTypeOffset(int waveIndex)
	{
		return 0;
	}

	protected virtual string GetAllyFormationPath( int troopIndex )
	{
		return "";
	}

	protected virtual string GetEnemyFormationPath( int troopIndex )
	{
		return "";
	}

	protected virtual string GetBattleEventPath( int troopIndex )
	{
		return "";
	}

	protected virtual HeroInfo[] GetUnitInfos(int[] unitIndex, int[] levels, int[] difficultyTypes, int levelOffset, int difficultyOffset)
	{
		HeroInfo[] retValues = null;

		retValues = new HeroInfo[unitIndex.Length];

		for (int i = 0; i < unitIndex.Length; i++) {
			retValues [i] = new HeroInfo (System.Data.BattleContext, unitIndex [i], levels [i] + levelOffset, difficultyTypes [i] + difficultyOffset);
		}

		return retValues;
	}

	protected virtual void SetAddBattleDeckInfos()
	{

	}

    public virtual void OnFixedUnitMaked( HeroInfo info)
    {

    }

    #endregion

    public abstract override void ExecuteBackKey();
}
