using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public interface IDeckSettingObserver
{
	void OnSetDeckInfo (int waveIndex, int posIndex, TroopDeployDefinitions.DeckUnitType deckUnitType, long unitID, HeroInfo unitInfo);    
}

public interface IDeckChangePosObserver
{
	void OnChangeDeckPosition (int waveIndex, int posIndex, string formationName, Vector2 offsetPosition);
}

public class TroopDeployInfoManager
{
	static TroopDeployInfoManager _instance;
    public static bool isLoadTroopDeploy = false;

    public static bool isAllyWave = false;
    public static bool isSaveDeck = true;

    public static HashSet<string> allyWaveListInfo = new HashSet<string>();

	public static TroopDeployInfoManager Instance
	{
		get{
            if (_instance == null) {
                InitTroopDeploy();
            }
			
			return _instance;
		}
	}

    public static void InitTroopDeploy()
    {
        _instance = new TroopDeployInfoManager();
        allyWaveListInfo.Add(TroopDeployDefinitions.UserDeckKeyType.GuildWarDeck.ToString());
    }

	public static void Exit()
	{
        if (isLoadTroopDeploy)
            isLoadTroopDeploy = false;

        if (isAllyWave)
            isAllyWave = false;

        if (!isSaveDeck)
            isSaveDeck = true;

        _instance = null;

        allyWaveListInfo.Clear();
    }

	#region Variables

	int _curWaveIndex = 0;

    FormationMap _formaionMapInfo = new FormationMap();

	UserDeckData _userDeckData = new UserDeckData();

	Dictionary<int /* Wave Index */, List<EnemyBattleTroopInfo>> _enemyBattleTroopInfos = new Dictionary<int, List<EnemyBattleTroopInfo>>();
	Dictionary<int /* WaveIndex */, MapMoveBoundInfo> _mapMoveBoundInfos = new Dictionary<int, MapMoveBoundInfo>();

	int _currentEnergyCount = 0;
	string _currentEnergyPath = "";

    FriendUnitBaseInfo _friendUnitInfo = null;
    bool _isRevisionIconPos = false;

    BattleMapInformation _battleMapInfo;
    int _curPositionIndex = 0;
    List<PlayingMissionHeroInfo> _heroBattleHPList = new List<PlayingMissionHeroInfo>();
    List<AloneMissionEnemyParty> _enemyBattleHPList = new List<AloneMissionEnemyParty>();
    List<PlayingMissionHeroInfo> _supportHeroBattleHPList = new List<PlayingMissionHeroInfo>();

    bool _isCurBattleLose = false;

    #endregion

    #region Properties

    public int CurWaveIndex
	{
		get{ return _curWaveIndex; }
		set{ _curWaveIndex = value; }
	}

	public FormationMap FormaionMapInfo
	{
		get{ return _formaionMapInfo; }
		set{ _formaionMapInfo = value; }
	}

	public UserDeckData UserDeckData
	{
		get{ return _userDeckData; }
		set{ _userDeckData = value; }
	}

	public Dictionary<int /* Wave Index */, List<EnemyBattleTroopInfo>> EnemyBattleTroopInfos
	{
		get{ return _enemyBattleTroopInfos; }
	}

	public Dictionary<int /* WaveIndex */, MapMoveBoundInfo> MapMoveBoundInfos
	{
		get{ return _mapMoveBoundInfos; }
	}

	public int CurrentEnergyCount
	{
		get{ return _currentEnergyCount; }
		set{ _currentEnergyCount = value; }
	}

	public string CurrentEnergyPath
	{
		get{ return _currentEnergyPath; }
		set{ _currentEnergyPath = value; }
	}

    public FriendUnitBaseInfo FriendUnitInfo
    {
        get { return _friendUnitInfo; }
        set { _friendUnitInfo = value; }
    }

    public bool IsRevisionIconPos
    {
        get { return _isRevisionIconPos; }
        set { _isRevisionIconPos = value; }
    }

    public BattleMapInformation BattleMapInfo
    {
        get { return _battleMapInfo; }
        set { _battleMapInfo = value; }
    }

    public int CurPositionIndex
    {
        get { return _curPositionIndex; }
        set {
            _curPositionIndex = value;
            if(_battleMapInfo != null)
                _battleMapInfo.SetEnablePositionInfo(_curPositionIndex);
        }
    }

    public List<PlayingMissionHeroInfo> HeroBattleHPList
    {
        get { return _heroBattleHPList; }
        set { _heroBattleHPList = value; }
    }

    public List<AloneMissionEnemyParty> EnemyBattleHPList
    {
        get { return _enemyBattleHPList; }
        set { _enemyBattleHPList = value; }
    }

    public List<PlayingMissionHeroInfo> SupportHeroBattleHPList
    {
        get { return _supportHeroBattleHPList; }
        set { _supportHeroBattleHPList = value; }
    }

    public bool IsCurBattleLose
    {
        get { return _isCurBattleLose; }
        set { _isCurBattleLose = value; }
    }

    #endregion

    #region Methods

    public UserDefaultDeck GetUserSaveDeck(int waveIndex, int posIndex)
	{
        if (TroopDeployInfoManager.isAllyWave) {
            return _userDeckData.UserDeckSave.GetUserWaveDefaultDeck(waveIndex, posIndex);
        } else {
            return _userDeckData.UserDeckSave.GetUserDefaultDeck(posIndex);
        }
	}

	public UserDeckPosInfo GetUserSaveDeckPosInfo(int waveIndex, int posIndex)
	{
		WaveFormationInfo waveFormation = _formaionMapInfo.GetWaveFormationInfo (waveIndex);

		if (_userDeckData.UserDeckSave.CurUserDeckSave.UserDeckPositions.ContainsKey (waveFormation.AllyFormationName)) {
			UserDeckPosition userDeckPos = _userDeckData.UserDeckSave.CurUserDeckSave.UserDeckPositions[waveFormation.AllyFormationName];
			if (userDeckPos.UserDeckPosInfos.ContainsKey (posIndex))
				return userDeckPos.UserDeckPosInfos [posIndex];
		}

		return null;
	}

	public int GetValidPosIndex(int waveIndex)
	{
        Dictionary<int /* Pos Index */, UserBattleDeck> battleDecks = null;
        if (TroopDeployInfoManager.isAllyWave) {
            battleDecks = _userDeckData.GetUserBattleDecks(waveIndex);
        } else {
            battleDecks = _userDeckData.UserBattleDecks;
        }

		for (int i = 0; i < battleDecks.Count; i++) {
			int posIndex = i;
			if (battleDecks[posIndex].DeckUnitType == TroopDeployDefinitions.DeckUnitType.Empty && 
				!battleDecks[posIndex].IsHideIcon)
				return posIndex;
		}

		return -1;
	}

    public int GetValidWavePosIndex(ref int waveIndex)
    {
        List<int> waveKeys = _userDeckData.UserWaveBattleDecks.Keys.ToList();
        for(int i = 0;i< waveKeys.Count; i++) {
            int curWave = waveKeys[i];
            Dictionary<int /* Pos Index */, UserBattleDeck> battleDecks = _userDeckData.UserWaveBattleDecks[waveKeys[i]];

            for (int j = 0; j < battleDecks.Count; j++) {
                int posIndex = j;
                if (battleDecks[posIndex].DeckUnitType == TroopDeployDefinitions.DeckUnitType.Empty &&
                    !battleDecks[posIndex].IsHideIcon) {
                    waveIndex = curWave;
                    return posIndex;
                }
            }
        }

        return -1;
    }

    public int GetFixedPosIndex(int waveIndex)
	{
		WaveFormationInfo waveFormation = _formaionMapInfo.GetWaveFormationInfo (waveIndex);

		for (int i = 0; i < waveFormation.allyTroopInfos.Count; i++)
		{
			if (waveFormation.allyTroopInfos[i].FormationTroopKind == FormationDefinitions.FormationTroopKind.Fixed)
				return i;
		}

		return -1;
	}

	public int GetFriendUnitIndex(int waveIndex)
	{
		for (int i = 0; i < _userDeckData.GetCurUserBattleDecks().Count; i++)
		{
			int posIndex = i;
			if (_userDeckData.GetCurUserBattleDecks()[posIndex].DeckUnitType == TroopDeployDefinitions.DeckUnitType.FriendUnit)
				return posIndex;
		}

		return -1;
	}

	public bool IsHaveUnit()
	{
		List<int> deckKeys = _userDeckData.GetCurUserBattleDecks().Keys.ToList ();
		for (int i = 0; i < deckKeys.Count; i++) {
			if (_userDeckData.GetCurUserBattleDecks()[deckKeys [i]].DeckUnitType == TroopDeployDefinitions.DeckUnitType.UserHave)
				return true;
		}

		return false;
	}

    public bool IsTutorialUnit()
    {
        List<int> deckKeys = _userDeckData.GetCurUserBattleDecks().Keys.ToList ();
        for (int i = 0; i < deckKeys.Count; i++) {
            if (_userDeckData.GetCurUserBattleDecks()[deckKeys [i]].DeckUnitType == TroopDeployDefinitions.DeckUnitType.TutorialUnit)
                return true;
        }

        return false;
    }

	public int GetDeckCount()
	{
		int deckCount = 0;
		List<int> battleDeckKeys = _userDeckData.GetCurUserBattleDecks().Keys.ToList ();
		for (int i = 0; i < battleDeckKeys.Count; i++) {
			if (_userDeckData.GetCurUserBattleDecks()[i].DeckUnitType != TroopDeployDefinitions.DeckUnitType.Empty) {
				deckCount++;
			}
		}

		return deckCount;
	}

	public void RefreshUnitInfo()
	{
        InitAllyUnitInfo();

        List<int> enemyTroopKeys = _enemyBattleTroopInfos.Keys.ToList ();
		for (int i = 0; i < enemyTroopKeys.Count; i++) {
			List<EnemyBattleTroopInfo> troops = _enemyBattleTroopInfos [enemyTroopKeys [i]];
			for (int j = 0; j < troops.Count; j++) {
				EnemyBattleTroopInfo troopInfo = troops [j];
				if (troopInfo.UnitInfo != null) {
					troopInfo.UnitInfo.Init ();
				}
			}
		}
	}

    public void InitAllyUnitInfo()
    {
        List<int> userDeckKeys = _userDeckData.GetCurUserBattleDecks().Keys.ToList();
        for (int i = 0; i < userDeckKeys.Count; i++) {
            UserBattleDeck battleDeck = _userDeckData.GetCurUserBattleDecks()[userDeckKeys[i]];
            if (battleDeck.UnitInfo != null) {
                battleDeck.UnitInfo.Init();
            }
        }
    }

    public List<long> GetUserBattleUnits()
    {
        List<long> retValue = new List<long>();

        List<int> userDeckKeys = _userDeckData.GetCurUserBattleDecks().Keys.ToList();
        for(int i = 0;i< userDeckKeys.Count;i++) {
            if(_userDeckData.GetCurUserBattleDecks()[userDeckKeys[i]].UnitID != -1 && _userDeckData.GetCurUserBattleDecks()[userDeckKeys[i]].DeckUnitType == TroopDeployDefinitions.DeckUnitType.UserHave && _userDeckData.GetCurUserBattleDecks()[userDeckKeys[i]].UnitID > 0)
                retValue.Add(_userDeckData.GetCurUserBattleDecks()[userDeckKeys[i]].UnitID);
        }

        return retValue;
    }

	#endregion
}
