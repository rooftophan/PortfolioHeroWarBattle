using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UserDeckData : IDeckSettingObserver
{
    #region Variables

    TroopDeployDefinitions.UserDeckKeyType _curUserDeckKey;

	string _curDeckFileName;
    string _groupName = "";

    Dictionary<int /* waveIndex */, Dictionary<int /* Pos Index */, UserBattleDeck>> _userWaveBattleDecks = new Dictionary<int, Dictionary<int, UserBattleDeck>>();
    Dictionary<int /* Pos Index */, UserBattleDeck> _userBattleDecks = new Dictionary<int, UserBattleDeck>();
	Dictionary<int /* Wave Index */, UserBattleDeckFormation> _userBattleDeckFormations = new Dictionary<int, UserBattleDeckFormation>();

	UserDeckSaveInfo _userDeckSave = new UserDeckSaveInfo();

	#endregion

	#region Properties

	public string CurDeckFileName {
		get { return _curDeckFileName; }
	}

	public TroopDeployDefinitions.UserDeckKeyType CurUserDeckKey
	{
		get{ return _curUserDeckKey; }
		set{ _curUserDeckKey = value; }
	}

    public Dictionary<int /* waveIndex */, Dictionary<int /* Pos Index */, UserBattleDeck>> UserWaveBattleDecks
    {
        get { return _userWaveBattleDecks; }
    }

    public Dictionary<int /* Pos Index */, UserBattleDeck> UserBattleDecks
	{
		get{ return _userBattleDecks; }
	}

	public Dictionary<int /* Wave Index */, UserBattleDeckFormation> UserBattleDeckFormations
	{
		get{ return _userBattleDeckFormations; }
	}

	public UserDeckSaveInfo UserDeckSave
	{
		get{ return _userDeckSave; }
	}

	#endregion

	#region Methods

	public void SetDeckFileName(TroopDeployDefinitions.UserDeckKeyType deckKeyType, long userID, params object[] values)
	{
		_curUserDeckKey = deckKeyType;
        switch (_curUserDeckKey) {
            case TroopDeployDefinitions.UserDeckKeyType.StoryDeck: {
                    Tribe tribe = (Tribe)values[0];
                    _curDeckFileName = string.Format("{0}_{1}", _curUserDeckKey.ToString(), userID);
                    _groupName = _curDeckFileName;
                }
                break;
            case TroopDeployDefinitions.UserDeckKeyType.CompanyMissionDeck: {
                    long companyID = (long)values[0];
                    int missionKind = (int)values[1];
                    _curDeckFileName = _curUserDeckKey.ToString();
                    _groupName = string.Format("{0}_{1}_{2}_{3}", _curUserDeckKey.ToString(), userID, companyID, missionKind);
                }
                break;
            case TroopDeployDefinitions.UserDeckKeyType.UserArenaDeck: {
                    Tribe tribe = (Tribe)values[0];
                    int deckIndex = (int)values[1];

                    if (deckIndex >= 1)
                        _curDeckFileName = string.Format("{0}_{1}_{2}_{3}", _curUserDeckKey.ToString(), userID, tribe.ToString(), deckIndex.ToString());
                    else
                        _curDeckFileName = string.Format("{0}_{1}_{2}", _curUserDeckKey.ToString(), userID, tribe.ToString());
                    _groupName = _curDeckFileName;
                }
                break;
            case TroopDeployDefinitions.UserDeckKeyType.UserArenaDefenseDeck: {
                    Tribe tribe = (Tribe)values[0];

                    if (values.Length > 1)
                        _curDeckFileName = string.Format("{0}_{1}_{2}_{3}", _curUserDeckKey.ToString(), userID, tribe.ToString(), values[1]);
                    else
                        _curDeckFileName = string.Format("{0}_{1}_{2}", _curUserDeckKey.ToString(), userID, tribe.ToString());

                    _groupName = _curDeckFileName;
                }
                break;
            case TroopDeployDefinitions.UserDeckKeyType.UserArenaDefensePracticeDeck: {
                    Tribe tribe = (Tribe)values[0];

                    if (values.Length > 1)
                        _curDeckFileName = string.Format("{0}_{1}_{2}_{3}", _curUserDeckKey.ToString(), userID, tribe.ToString(), values[1]);
                    else
                        _curDeckFileName = string.Format("{0}_{1}_{2}", _curUserDeckKey.ToString(), userID, tribe.ToString());

                    _groupName = _curDeckFileName;
                }
                break;
            case TroopDeployDefinitions.UserDeckKeyType.SearchPointDeck: {
                    Tribe tribe = (Tribe)values[0];
                    int mode = (int)values[1];
                    _curDeckFileName = string.Format("{0}_{1}_{2}_{3}", _curUserDeckKey.ToString(), userID, tribe.ToString(), mode);
                    _groupName = _curDeckFileName;
                }
                break;
            case TroopDeployDefinitions.UserDeckKeyType.SearchPointBonusStageDeck: {
                    Tribe tribe = (Tribe)values[0];
                    int mode = (int)values[1];
                    _curDeckFileName = string.Format("{0}_{1}_{2}_{3}", _curUserDeckKey.ToString(), userID, tribe.ToString(), mode);
                    _groupName = _curDeckFileName;
                }
                break;
            case TroopDeployDefinitions.UserDeckKeyType.TraceDeck: {
                    long companyID = (long)values[0];
                    int missionKind = (int)values[1];
                    _curDeckFileName = string.Format("{0}_{1}_{2}_{3}", _curUserDeckKey.ToString(), userID, companyID, missionKind);
                    _groupName = _curDeckFileName;
                }
                break;
            case TroopDeployDefinitions.UserDeckKeyType.TrainingCenterDeck: {
                    string formation = (string)values[0];
                    int missionKind = (int)values[1];
                    _curDeckFileName = string.Format("{0}_{1}_{2}_{3}", _curUserDeckKey.ToString(), userID, formation, missionKind);
                    _groupName = _curDeckFileName;
                }
                break;
            case TroopDeployDefinitions.UserDeckKeyType.TrainingArenaDeck: {
                    long companyID = (long)values[0];
                    _curDeckFileName = string.Format("{0}_{1}", _curUserDeckKey.ToString(), userID);
                    _groupName = _curDeckFileName;
                }
                break;
            case TroopDeployDefinitions.UserDeckKeyType.AloneMissionDeck: {
                    int missionKind = (int)values[0];
                    int mode = 0;

                    if (values.Length > 1)
                        mode = (int)values[1];

                    _curDeckFileName = string.Format("{0}_{1}_{2}_{3}", _curUserDeckKey.ToString(), userID, missionKind, mode);
                    _groupName = _curDeckFileName;
                }
                break;
            case TroopDeployDefinitions.UserDeckKeyType.ExpansionDeck: {
                    int missionKind = (int)values[0];
                    _curDeckFileName = string.Format("{0}_{1}_{2}", _curUserDeckKey.ToString(), userID, missionKind.ToString());
                    _groupName = _curDeckFileName;
                }
                break;
            case TroopDeployDefinitions.UserDeckKeyType.PartyMissionDeck: {
                    long partyId = (long)values[0];
                    int missionKind = (int)values[1];
                    _curDeckFileName = _curUserDeckKey.ToString();
                    _groupName = string.Format("{0}_{1}_{2}_{3}", _curUserDeckKey.ToString(), userID, partyId, missionKind);
                }
                break;
            case TroopDeployDefinitions.UserDeckKeyType.DailyDungeon: {
                    int missionKind = (int)values[0];
                    _curDeckFileName = string.Format("{0}_{1}_{2}", _curUserDeckKey.ToString(), userID, missionKind);
                    _groupName = _curDeckFileName;
                    break;
                }
            case TroopDeployDefinitions.UserDeckKeyType.UndergroundLaboratoryDeck: {
                    _curDeckFileName = string.Format("{0}_{1}", _curUserDeckKey.ToString(), userID);
                    _groupName = _curDeckFileName;
                    break;
                }
            case TroopDeployDefinitions.UserDeckKeyType.HeroChallenge: {
                    int heroIndex = (int)values[0];
                    int level = (int)values[1];

                    _curDeckFileName = _curUserDeckKey.ToString();
                    _groupName = string.Format("{0}_{1}_{2}_{3}", _curUserDeckKey.ToString(), userID, heroIndex, level);

                    break;
                }
            case TroopDeployDefinitions.UserDeckKeyType.StoryEliteDeck: {
                    _curDeckFileName = string.Format("{0}_{1}_{2}", _curUserDeckKey.ToString(), userID, (string)values[0]);
                    _groupName = _curDeckFileName;
                }
                break;
            case TroopDeployDefinitions.UserDeckKeyType.GuildRaidDeck: {
                    int missionKind = (int)values[0];
                    int rotationNum = (int)values[1];

                    _curDeckFileName = _curUserDeckKey.ToString();
                    _groupName = string.Format("{0}_{1}_{2}_{3}", _curUserDeckKey.ToString(), userID, missionKind, rotationNum);
                }
                break;
            case TroopDeployDefinitions.UserDeckKeyType.GuildRaidDeckBoss: {
                    int missionKind = (int)values[0];
                    int rotationNum = (int)values[1];

                    _curDeckFileName = _curUserDeckKey.ToString();
                    _groupName = string.Format("{0}_{1}_{2}_{3}", _curUserDeckKey.ToString(), userID, missionKind, rotationNum);
                }
                break;
            case TroopDeployDefinitions.UserDeckKeyType.GuildWarDeck: {
                    int curFieldType = (int)values[0];

                    _curDeckFileName = _curUserDeckKey.ToString();
                    _groupName = string.Format("{0}_{1}_{2}", _curUserDeckKey.ToString(), userID, curFieldType);
                }
                break;

        }
    }

	public void RefreshBattleDeckPosition(int waveIndex)
	{
		UserBattleDeckFormation deckFormation = _userBattleDeckFormations [waveIndex];
		for (int i = 0; i < _userBattleDeckFormations.Count; i++) {
			if (i == waveIndex)
				continue;

			if (_userBattleDeckFormations [i].AllyFormationName == deckFormation.AllyFormationName) {
				for (int j = 0; j < _userBattleDeckFormations [i].UserDeckPositionInfos.Count; j++) {
					_userBattleDeckFormations [i].UserDeckPositionInfos [j].OffsetPosition = deckFormation.UserDeckPositionInfos [j].OffsetPosition;
				}
			}
		}
	}

    public Dictionary<int /* Pos Index */, UserBattleDeck> GetUserBattleDecks(int waveIndex)
    {
        if (!_userWaveBattleDecks.ContainsKey(waveIndex)) {
            Dictionary<int /* Pos Index */, UserBattleDeck> inputUserBattleDecks = new Dictionary<int, UserBattleDeck>();
            _userWaveBattleDecks.Add(waveIndex, inputUserBattleDecks);
        }

        return _userWaveBattleDecks[waveIndex];
    }

    public Dictionary<int /* Pos Index */, UserBattleDeck> GetCurUserBattleDecks()
    {
        if (TroopDeployInfoManager.isAllyWave) {
            return GetUserBattleDecks(TroopDeployInfoManager.Instance.CurWaveIndex);
        } else {
            return _userBattleDecks;
        }
    }

    public bool CheckAllWaveBattleDeployed()
    {
        List<int> waveKeys = _userWaveBattleDecks.Keys.ToList();
        for(int i = 0;i< waveKeys.Count; i++) {
            Dictionary<int /* Pos Index */, UserBattleDeck> battleDecks = _userWaveBattleDecks[waveKeys[i]];
            List<int> posKeys = battleDecks.Keys.ToList();
            for(int j = 0;j< posKeys.Count; j++) {
                UserBattleDeck battleDeck = battleDecks[posKeys[j]];
                if(battleDeck.UnitID <= 0) {
                    return false;
                }
            }
        }

        return true;
    }

    public bool CheckWaveBattleDeployed()
    {
        List<int> waveKeys = _userWaveBattleDecks.Keys.ToList();
        bool[] isWaveDeployStates = new bool[_userWaveBattleDecks.Count];
        for (int i = 0; i < waveKeys.Count; i++) {
            Dictionary<int /* Pos Index */, UserBattleDeck> battleDecks = _userWaveBattleDecks[waveKeys[i]];
            List<int> posKeys = battleDecks.Keys.ToList();
            isWaveDeployStates[i] = false;
            for (int j = 0; j < posKeys.Count; j++) {
                UserBattleDeck battleDeck = battleDecks[posKeys[j]];
                if (battleDeck.UnitID > 0) {
                    isWaveDeployStates[i] = true;
                    break;
                }
            }
        }

        for(int i = 0;i< isWaveDeployStates.Length; i++) {
            if (!isWaveDeployStates[i]) {
                return false;
            }
        }

        return true;
    }

    public void SetAutoMode(bool autoMode)
    {
        _userDeckSave.CurUserDeckSave.AutoMode = autoMode;
    }

	public void SaveCurDeckFile()
	{
		_userDeckSave.SaveUserDeck();
	}

	public void LoadCurDeckFile()
	{
		_userDeckSave.LoadUserDeck(_curDeckFileName, _groupName);
    }

    public bool IsExistDeckGroupID()
    {
        if (_userDeckSave.ContentsUserDeckSaveGroups.ContainsKey(_curDeckFileName)) {
            Dictionary<string /* Group Key */, UserDeckSaveGroup> deckSaveGroups = _userDeckSave.ContentsUserDeckSaveGroups[_curDeckFileName];
            if (deckSaveGroups.ContainsKey(_groupName)) {
                return true;
            }
        }

        return false;
    }

    public bool IsExistDeckFileName()
    {
        if (_userDeckSave.ContentsUserDeckSaveGroups.ContainsKey(_curDeckFileName)) {
            Dictionary<string /* Group Key */, UserDeckSaveGroup> deckSaveGroups = _userDeckSave.ContentsUserDeckSaveGroups[_curDeckFileName];
            if(deckSaveGroups.Count > 0) {
                return true;
            }
        }

        return false;
    }

	public void ClearCurDeckFile()
	{
		_userDeckSave.ClearCurDeck(_curDeckFileName, _groupName);
	}

	public void LoadCurDeckArena(ArenaBattleLocation[] arenaBattleLocation)
	{
		if (arenaBattleLocation == null)
			return;
		
		_userDeckSave.CurUserDeckSave.UserDefaultDecks.Clear();
		
		for (int i = 0; i < arenaBattleLocation.Length; i++)
		{
			var location = arenaBattleLocation[i];

			if (location.heroId > 0)
			{
				UserDefaultDeck inputUserDeck = new UserDefaultDeck();
				inputUserDeck.PosIndex = i;
				inputUserDeck.UnitID = location.heroId;
				inputUserDeck.DeckUnitType = TroopDeployDefinitions.DeckUnitType.UserHave;

				_userDeckSave.CurUserDeckSave.UserDefaultDecks.Add(inputUserDeck.PosIndex, inputUserDeck);
			}			

			if (_userBattleDeckFormations.Count > 0 && _userBattleDeckFormations[0].UserDeckPositionInfos.ContainsKey(i))
				_userBattleDeckFormations[0].UserDeckPositionInfos[i].OffsetPosition = new Vector2(location.x, location.y);
		}
	}

	public UserBattleDeck GetUserBattleDeck(int posIndex)
	{
        Dictionary<int, UserBattleDeck> battleDecks = null;
        if (TroopDeployInfoManager.isAllyWave) {
            battleDecks = GetUserBattleDecks(TroopDeployInfoManager.Instance.CurWaveIndex);
        } else {
            battleDecks = _userBattleDecks;
        }

        if (battleDecks.ContainsKey (posIndex)) {
			return battleDecks[posIndex];
		}

		return null;
	}

	public void SetUserBattleDeck(int waveIndex, UserBattleDeckFormation userBattleDeck)
	{
		if (_userBattleDeckFormations.ContainsKey (waveIndex)) {
			_userBattleDeckFormations [waveIndex] = userBattleDeck;
		} else {
			_userBattleDeckFormations.Add (waveIndex, userBattleDeck);
		}
	}

	public UserDeckPosInfo GetUserBattleDeckPosInfo(int waveIndex, int posIndex)
	{
		UserBattleDeckFormation userBattleFormation = null;
		if (_userBattleDeckFormations.ContainsKey (waveIndex)) {
			userBattleFormation = _userBattleDeckFormations [waveIndex];
		}

		if (userBattleFormation == null)
			return null;

		if (userBattleFormation.UserDeckPositionInfos.ContainsKey (posIndex)) {
			return userBattleFormation.UserDeckPositionInfos [posIndex];
		}

		return null;
	}

    public Vector2 GetUserBattleDeckStartPosForAutoSort(int waveIndex)
	{
		UserBattleDeckFormation userBattleFormation = null;
		if (_userBattleDeckFormations.ContainsKey (waveIndex)) {
			userBattleFormation = _userBattleDeckFormations [waveIndex];
		}

		if (userBattleFormation == null)
			return Vector2.zero;
        int count = 0;
        float ySum = 0;
        UserDeckPosInfo info =  userBattleFormation.UserDeckPositionInfos[0];
        float baseX = info.UnitPosition.x;
        foreach( var kvp in userBattleFormation.UserDeckPositionInfos )
        {
            if( baseX == kvp.Value.UnitPosition.x )
            {
                ySum += kvp.Value.UnitPosition.y;
                count++;
            }
            
        }	
        
		return new Vector2( baseX, ySum/count);
	}    

	public UserDeckPosInfo GetBattleDeckPosInfoByUnitID(int waveIndex, long unitID)
	{
		UserBattleDeckFormation userBattleFormation = null;
		if (_userBattleDeckFormations.ContainsKey (waveIndex)) {
			userBattleFormation = _userBattleDeckFormations [waveIndex];
		}

		if (userBattleFormation == null)
			return null;

        Dictionary<int, UserBattleDeck> battleDecks = null;

        if (TroopDeployInfoManager.isAllyWave) {
            battleDecks = GetUserBattleDecks(TroopDeployInfoManager.Instance.CurWaveIndex);
        } else {
            battleDecks = _userBattleDecks;
        }

        int curPosIndex = -1;
		List<int> userDeckKeys = battleDecks.Keys.ToList ();
		for (int i = 0; i < battleDecks.Count; i++) {
			if (battleDecks[userDeckKeys [i]].UnitID == unitID) {
				curPosIndex = userDeckKeys [i];
				break;
			}
		}

		if (curPosIndex != -1) {
			return userBattleFormation.UserDeckPositionInfos [curPosIndex];
		}

		return null;
	}

	public long[] GetDesrvingHeroIDs()
	{
		List<long> ids = new List<long>();

        Dictionary<int, UserBattleDeck> battleDecks = null;

        if (TroopDeployInfoManager.isAllyWave) {
            battleDecks = GetUserBattleDecks(TroopDeployInfoManager.Instance.CurWaveIndex);
        } else {
            battleDecks = _userBattleDecks;
            //List<int> deckKeys = _userBattleDecks.Keys.ToList();
            //for (int i = 0; i < deckKeys.Count; i++) {
            //    if (_userBattleDecks[deckKeys[i]].DeckUnitType == TroopDeployDefinitions.DeckUnitType.Empty
            //        || _userBattleDecks[deckKeys[i]].DeckUnitType == TroopDeployDefinitions.DeckUnitType.FriendUnit)
            //        continue;

            //    if (_userBattleDecks[deckKeys[i]].UnitID > 0) {
            //        ids.Add(_userBattleDecks[deckKeys[i]].UnitID);
            //    }
            //}
        }

        List<int> deckKeys = battleDecks.Keys.ToList();
        for (int i = 0; i < deckKeys.Count; i++) {
            if (battleDecks[deckKeys[i]].DeckUnitType == TroopDeployDefinitions.DeckUnitType.Empty
                || battleDecks[deckKeys[i]].DeckUnitType == TroopDeployDefinitions.DeckUnitType.FriendUnit)
                continue;

            if (battleDecks[deckKeys[i]].UnitID > 0) {
                ids.Add(battleDecks[deckKeys[i]].UnitID);
            }
        }

        return ids.ToArray();
	}

	#endregion

	#region IDeckSettingObserver

	void IDeckSettingObserver.OnSetDeckInfo (int waveIndex, int posIndex, TroopDeployDefinitions.DeckUnitType deckUnitType, long unitID, HeroInfo unitInfo)
	{
        Dictionary<int, UserBattleDeck> battleDecks = null;

        if (TroopDeployInfoManager.isAllyWave) {
            battleDecks = GetUserBattleDecks(waveIndex);
        } else {
            battleDecks = _userBattleDecks;
        }

        if (posIndex != -1) {
            UserBattleDeck battleDeck = battleDecks[posIndex];
            battleDeck.DeckUnitType = deckUnitType;
            battleDeck.UnitID = unitID;
            battleDeck.UnitInfo = unitInfo;
            battleDeck.UnitInfo.FormationIndex = (sbyte)posIndex;
        } else {
            for (int i = 0; i < battleDecks.Count; i++) {
                if (battleDecks[i].UnitID == unitID) {
                    battleDecks[i].DeckUnitType = deckUnitType;
                    battleDecks[i].UnitID = -1;
                    battleDecks[i].UnitInfo = null;
                    break;
                }
            }
        }
    }

	#endregion
}
