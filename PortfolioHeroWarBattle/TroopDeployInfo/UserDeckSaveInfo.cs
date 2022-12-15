using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.Linq;

public class UserDefaultDeck
{
	#region Variables

	protected int _posIndex;
	protected TroopDeployDefinitions.DeckUnitType _deckUnitType;
	protected long _unitID;

	#endregion

	#region Properties

	public int PosIndex
	{
		get{ return _posIndex; }
		set{ _posIndex = value; }
	}

	public TroopDeployDefinitions.DeckUnitType DeckUnitType
	{
		get{ return _deckUnitType; }
		set{ _deckUnitType = value; }
	}

	public long UnitID
	{
		get{ return _unitID; }
		set{ _unitID = value; }
	}

	#endregion
}

public class UserDeckPosInfo
{
	#region Variables

	int _posIndex;
	Vector2 _offsetPosition;
	Vector2 _unitPosition;

	#endregion

	#region Properties

	public int PosIndex
	{
		get{ return _posIndex; }
		set{ _posIndex = value; }
	}

	public Vector2 OffsetPosition
	{
		get{ return _offsetPosition; }
		set{
            _offsetPosition = value;
        }
	}

	public Vector2 UnitPosition
	{
		get{ return _unitPosition; }
		set{ _unitPosition = value; }
	}

	public Vector2 Position
	{
		get { return _unitPosition + _offsetPosition; }
	}

	#endregion
}

public class UserDeckPosition
{
	#region Variables

	string _formationName;
	Dictionary<int /* Pos Index */, UserDeckPosInfo> _userDeckPosInfos = new Dictionary<int, UserDeckPosInfo>();

	#endregion

	#region Properties

	public string FormationName
	{
		get{ return _formationName; }
		set{ _formationName = value; }
	}

	public Dictionary<int /* Pos Index */, UserDeckPosInfo> UserDeckPosInfos
	{
		get{ return _userDeckPosInfos; }
	}

	#endregion
}

public class UserDeckSaveGroup
{
    #region Variables

    bool _autoMode = true;

    Dictionary<int /* waveIndex */, Dictionary<int /* PosIndex */, UserDefaultDeck>> _userWaveDefaultDecks = new Dictionary<int, Dictionary<int, UserDefaultDeck>>();
    Dictionary<int /* PosIndex */, UserDefaultDeck> _userDefaultDecks = new Dictionary<int, UserDefaultDeck>();

    Dictionary<string /* FormationName */, UserDeckPosition> _userDeckPositions = new Dictionary<string, UserDeckPosition>();

    #endregion

    #region Properties

    public bool AutoMode
    {
        get { return _autoMode; }
        set { _autoMode = value; }
    }

    public Dictionary<int /* waveIndex */, Dictionary<int /* PosIndex */, UserDefaultDeck>> UserWaveDefaultDecks
    {
        get { return _userWaveDefaultDecks; }
        set { _userWaveDefaultDecks = value; }
    }

    public Dictionary<int /* PosIndex */, UserDefaultDeck> UserDefaultDecks
    {
        get { return _userDefaultDecks; }
    }

    public Dictionary<string /* FormationName */, UserDeckPosition> UserDeckPositions
    {
        get { return _userDeckPositions; }
    }

    #endregion
}

public class UserDeckSaveInfo : IDeckSettingObserver, IDeckChangePosObserver
{
    #region Variables

    Dictionary<string /* Contents Name */, Dictionary<string /* Group Key */, UserDeckSaveGroup>> _contentsUserDeckSaveGroups = new Dictionary<string, Dictionary<string, UserDeckSaveGroup>>();

    Dictionary<string /* Group Key */, UserDeckSaveGroup> _userDeckSaveGroups;

    UserDeckSaveGroup _curUserDeckSave;

    #endregion

    #region Properties

    public Dictionary<string /* Group Key */, UserDeckSaveGroup> UserDeckSaveGroups
    {
        get { return _userDeckSaveGroups; }
    }

    public UserDeckSaveGroup CurUserDeckSave
    {
        get { return _curUserDeckSave; }
    }

    public Dictionary<string /* Contents Name */, Dictionary<string /* Group Key */, UserDeckSaveGroup>> ContentsUserDeckSaveGroups
    {
        get { return _contentsUserDeckSaveGroups; }
    }

    #endregion

    #region Methods

    public void SaveUserDeck()
	{
        if (!TroopDeployInfoManager.isSaveDeck)
            return;

		JsonWriter writer = new JsonWriter(5);
        writer.Reset();
		writer.WriteObjectStart();
		{
            List<string> contentNameKeys = _contentsUserDeckSaveGroups.Keys.ToList();

            for(int i = 0;i< contentNameKeys.Count; i++) {
                Dictionary<string /* Group Key */, UserDeckSaveGroup> deckGroups = _contentsUserDeckSaveGroups[contentNameKeys[i]];
                writer.WritePropertyName(contentNameKeys[i]);
                writer.WriteObjectStart();
                {
                    List<string> saveGroupKeys = deckGroups.Keys.ToList();
                    for (int j = 0; j < saveGroupKeys.Count; j++) {
                        UserDeckSaveGroup userSaveGroup = deckGroups[saveGroupKeys[j]];
                        writer.WritePropertyName(saveGroupKeys[j]);
                        writer.WriteObjectStart();
                        {
                            writer.WritePropertyName("AutoMode");
                            writer.Write(userSaveGroup.AutoMode);

                            if (TroopDeployInfoManager.allyWaveListInfo.Contains(contentNameKeys[i])) {
                                writer.WritePropertyName("UserWaveDefaultDecks");
                                writer.WriteArrayStart();
                                {
                                    List<int> waveKeys = userSaveGroup.UserWaveDefaultDecks.Keys.ToList();
                                    for (int k = 0;k< waveKeys.Count; k++) {
                                        Dictionary<int /* PosIndex */, UserDefaultDeck> defaultDecks = userSaveGroup.UserWaveDefaultDecks[waveKeys[k]];
                                        writer.WriteObjectStart();
                                        {
                                            writer.WritePropertyName("waveIndex");
                                            writer.Write(waveKeys[k]);

                                            writer.WritePropertyName("UserDefaultDeck");
                                            writer.WriteArrayStart();
                                            {
                                                List<int> posIndexKeys = defaultDecks.Keys.ToList();
                                                for (int l = 0; l < posIndexKeys.Count; l++) {
                                                    UserDefaultDeck userDefault = defaultDecks[posIndexKeys[l]];
                                                    writer.WriteObjectStart();
                                                    {
                                                        writer.WritePropertyName("PosIndex");
                                                        writer.Write(userDefault.PosIndex);

                                                        writer.WritePropertyName("DeckUnitType");
                                                        writer.Write((int)userDefault.DeckUnitType);

                                                        writer.WritePropertyName("UnitID");
                                                        writer.Write(userDefault.UnitID);
                                                    }
                                                    writer.WriteObjectEnd();
                                                }
                                            }
                                            writer.WriteArrayEnd();
                                        }
                                        writer.WriteObjectEnd();
                                    }
                                }
                                writer.WriteArrayEnd();
                            } else {
                                writer.WritePropertyName("UserDefaultDeck");
                                writer.WriteArrayStart();
                                {
                                    List<int> posIndexKeys = userSaveGroup.UserDefaultDecks.Keys.ToList();
                                    for (int k = 0; k < posIndexKeys.Count; k++) {
                                        UserDefaultDeck userDefault = userSaveGroup.UserDefaultDecks[posIndexKeys[k]];
                                        writer.WriteObjectStart();
                                        {
                                            writer.WritePropertyName("PosIndex");
                                            writer.Write(userDefault.PosIndex);

                                            writer.WritePropertyName("DeckUnitType");
                                            writer.Write((int)userDefault.DeckUnitType);

                                            writer.WritePropertyName("UnitID");
                                            writer.Write(userDefault.UnitID);
                                        }
                                        writer.WriteObjectEnd();
                                    }
                                }
                                writer.WriteArrayEnd();
                            }

                            writer.WritePropertyName("UserDeckPositions");
                            writer.WriteObjectStart();
                            {
                                List<string> formationNameKeys = userSaveGroup.UserDeckPositions.Keys.ToList();
                                for (int k = 0; k < formationNameKeys.Count; k++) {
                                    UserDeckPosition userDeckPos = userSaveGroup.UserDeckPositions[formationNameKeys[k]];
                                    writer.WritePropertyName(formationNameKeys[k]);
                                    writer.WriteArrayStart();
                                    {
                                        List<int> posIndexKeys = userDeckPos.UserDeckPosInfos.Keys.ToList();
                                        for (int l = 0; l < posIndexKeys.Count; l++) {
                                            UserDeckPosInfo deckPosInfo = userDeckPos.UserDeckPosInfos[posIndexKeys[l]];
                                            writer.WriteObjectStart();
                                            {
                                                writer.WritePropertyName("PosIndex");
                                                writer.Write(deckPosInfo.PosIndex);

                                                writer.WritePropertyName("PosX");
                                                writer.Write(deckPosInfo.OffsetPosition.x);

                                                writer.WritePropertyName("PosY");
                                                writer.Write(deckPosInfo.OffsetPosition.y);
                                            }
                                            writer.WriteObjectEnd();
                                        }

                                    }
                                    writer.WriteArrayEnd();
                                }
                            }
                            writer.WriteObjectEnd();
                        }
                        writer.WriteObjectEnd();
                    }
                }
                writer.WriteObjectEnd();
            }

            
        }
		writer.WriteObjectEnd ();

        FileUtility.SaveAESEncryptData(GetUserDeckFileNameByUserId(), writer.ToString());
    }

    public void ClearCurDeck(string contentKey, string clearGroupID)
    {
        if (_contentsUserDeckSaveGroups.ContainsKey(contentKey))
        {
            var saveGroup = _contentsUserDeckSaveGroups[contentKey];

            if (saveGroup.ContainsKey(clearGroupID))
            {
                saveGroup.Remove(clearGroupID);
                SaveUserDeck();    
            }
        }
    }

    static public bool JsonDataContainsKey(JsonData data, string key)
    {
        bool result = false;
        if( data == null )
            return result;
        if( !data.IsObject )
        {
            return result;
        }
        IDictionary tdictionary = data as IDictionary;
        if( tdictionary == null )
            return result;
        if( tdictionary.Contains( key ) )
        {
            result = true;
        }
        return result;
    }

    public void LoadUserDeck(params string[] fileNames)
	{
        _contentsUserDeckSaveGroups.Clear();
        _curUserDeckSave = null;
        if(_userDeckSaveGroups != null) {
            _userDeckSaveGroups.Clear();
            _userDeckSaveGroups = null;
        }

        string contentName = fileNames[0];
        string groupID = fileNames[1];

        bool isExistPrefs = false;
        if (PlayerPrefs.HasKey(contentName)) {
            string prefSaveData = PlayerPrefs.GetString(contentName);

            if (prefSaveData.Length >= 5) {
                _userDeckSaveGroups = new Dictionary<string, UserDeckSaveGroup>();
                _contentsUserDeckSaveGroups.Add(contentName, _userDeckSaveGroups);
                AddContentUserDeck(prefSaveData, groupID);
                isExistPrefs = true;
            } else {
                _curUserDeckSave = new UserDeckSaveGroup();
                _userDeckSaveGroups = new Dictionary<string, UserDeckSaveGroup>();
                _contentsUserDeckSaveGroups.Add(contentName, _userDeckSaveGroups);
                AddCurUserDeckGroup(groupID, _curUserDeckSave);
            }

            PlayerPrefs.DeleteKey(contentName);
        }

        string loadUserFileData = "";
        loadUserFileData = FileUtility.LoadAESEncryptData(GetUserDeckFileNameByUserId());
        if (string.IsNullOrEmpty(loadUserFileData)) {
            loadUserFileData = FileUtility.LoadAESEncryptData(GetUserDeckFileName());
        }

        if(string.IsNullOrEmpty(loadUserFileData) || loadUserFileData.Length < 5) {
            if (!_contentsUserDeckSaveGroups.ContainsKey(contentName)) {
                _curUserDeckSave = new UserDeckSaveGroup();
                _userDeckSaveGroups = new Dictionary<string, UserDeckSaveGroup>();
                _contentsUserDeckSaveGroups.Add(contentName, _userDeckSaveGroups);
                AddCurUserDeckGroup(groupID, _curUserDeckSave);
            } else {
                if (isExistPrefs) {
                    SaveUserDeck();
                }
            }
            
            return;
        }

		JsonData jData = JsonMapper.ToObject(loadUserFileData);

        foreach (DictionaryEntry entryContent in jData as IDictionary) {
            string contentKey = (string)entryContent.Key;

            Dictionary<string /* Group Key */, UserDeckSaveGroup> userDeckGroup = null;
            if (_contentsUserDeckSaveGroups.ContainsKey(contentKey)) {
                if (isExistPrefs) {
                    continue;
                } else {
                    userDeckGroup = _contentsUserDeckSaveGroups[contentKey];
                }
            } else {
                userDeckGroup = new Dictionary<string, UserDeckSaveGroup>();
                _contentsUserDeckSaveGroups.Add(contentKey, userDeckGroup);
            }

            JsonData jSubData = (JsonData)entryContent.Value;

            foreach (DictionaryEntry entryGroup in jSubData as IDictionary) {
                string groupKeyValue = (string)entryGroup.Key;
                JsonData jGroupDeck = (JsonData)entryGroup.Value;
                UserDeckSaveGroup userSaveGroup = new UserDeckSaveGroup();
                userSaveGroup.AutoMode = true;
                if (JsonDataContainsKey(jGroupDeck, "AutoMode") == true) {
                    userSaveGroup.AutoMode = (bool)jGroupDeck["AutoMode"];
                }

                if (TroopDeployInfoManager.allyWaveListInfo.Contains(contentKey)) {
                    for (int i = 0; i < jGroupDeck["UserWaveDefaultDecks"].Count; i++) {
                        JsonData jUserWaveDefaultDeck = jGroupDeck["UserWaveDefaultDecks"][i];
                        Dictionary<int, UserDefaultDeck> inputUserDecks = new Dictionary<int, UserDefaultDeck>();
                        int waveIndex = (int)jUserWaveDefaultDeck["waveIndex"];
                        for(int j = 0;j< jUserWaveDefaultDeck["UserDefaultDeck"].Count; j++) {
                            JsonData jUserDefaultDeck = jUserWaveDefaultDeck["UserDefaultDeck"][j];
                            UserDefaultDeck inputUserDeck = new UserDefaultDeck();
                            inputUserDeck.PosIndex = (int)jUserDefaultDeck["PosIndex"];
                            inputUserDeck.DeckUnitType = (TroopDeployDefinitions.DeckUnitType)(int)jUserDefaultDeck["DeckUnitType"];
                            if (jUserDefaultDeck["UnitID"].IsInt) {
                                inputUserDeck.UnitID = (long)(int)jUserDefaultDeck["UnitID"];
                            } else if (jUserDefaultDeck["UnitID"].IsLong) {
                                inputUserDeck.UnitID = (long)jUserDefaultDeck["UnitID"];
                            }
                            inputUserDecks.Add(inputUserDeck.PosIndex, inputUserDeck);
                        }
                        userSaveGroup.UserWaveDefaultDecks.Add(waveIndex, inputUserDecks);
                    }
                } else {
                    for (int i = 0; i < jGroupDeck["UserDefaultDeck"].Count; i++) {
                        JsonData jUserDefaultDeck = jGroupDeck["UserDefaultDeck"][i];
                        UserDefaultDeck inputUserDeck = new UserDefaultDeck();
                        inputUserDeck.PosIndex = (int)jUserDefaultDeck["PosIndex"];
                        inputUserDeck.DeckUnitType = (TroopDeployDefinitions.DeckUnitType)(int)jUserDefaultDeck["DeckUnitType"];
                        if (jUserDefaultDeck["UnitID"].IsInt) {
                            inputUserDeck.UnitID = (long)(int)jUserDefaultDeck["UnitID"];
                        } else if (jUserDefaultDeck["UnitID"].IsLong) {
                            inputUserDeck.UnitID = (long)jUserDefaultDeck["UnitID"];
                        }
                        userSaveGroup.UserDefaultDecks.Add(inputUserDeck.PosIndex, inputUserDeck);
                    }
                }

                userSaveGroup.UserDeckPositions.Clear();
                foreach (DictionaryEntry entry in jGroupDeck["UserDeckPositions"] as IDictionary) {
                    string keyValue = (string)entry.Key;
                    JsonData jUserDeckPos = (JsonData)entry.Value;

                    UserDeckPosition inputUserDeckPos = new UserDeckPosition();
                    inputUserDeckPos.FormationName = keyValue;
                    for (int i = 0; i < jUserDeckPos.Count; i++) {
                        UserDeckPosInfo inputDeckPosInfo = new UserDeckPosInfo();
                        JsonData jUserDeckPosInfo = jUserDeckPos[i];
                        float posX = 0f;
                        float posY = 0f;
                        inputDeckPosInfo.PosIndex = (int)jUserDeckPosInfo["PosIndex"];

                        if (jUserDeckPosInfo["PosX"].IsInt) {
                            posX = (float)(int)jUserDeckPosInfo["PosX"];
                        } else if (jUserDeckPosInfo["PosX"].IsDouble) {
                            posX = (float)(double)jUserDeckPosInfo["PosX"];
                        }

                        if (jUserDeckPosInfo["PosY"].IsInt) {
                            posY = (float)(int)jUserDeckPosInfo["PosY"];
                        } else if (jUserDeckPosInfo["PosY"].IsDouble) {
                            posY = (float)(double)jUserDeckPosInfo["PosY"];
                        }

                        inputDeckPosInfo.OffsetPosition = new Vector2(posX, posY);
                        inputUserDeckPos.UserDeckPosInfos.Add(inputDeckPosInfo.PosIndex, inputDeckPosInfo);
                    }

                    userSaveGroup.UserDeckPositions.Add(keyValue, inputUserDeckPos);
                }

                if(contentName == contentKey) {
                    _userDeckSaveGroups = userDeckGroup;

                    if (groupKeyValue == groupID) {
                        _curUserDeckSave = userSaveGroup;
                    }
                }
                
                AddUserDeckGroup(userDeckGroup, groupKeyValue, userSaveGroup);
            }
        }

        if (!_contentsUserDeckSaveGroups.ContainsKey(contentName)) {
            _curUserDeckSave = new UserDeckSaveGroup();
            _userDeckSaveGroups = new Dictionary<string, UserDeckSaveGroup>();
            _contentsUserDeckSaveGroups.Add(contentName, _userDeckSaveGroups);
            AddUserDeckGroup(_userDeckSaveGroups, groupID, _curUserDeckSave);
        } else {
            if (_curUserDeckSave == null) {
                _curUserDeckSave = new UserDeckSaveGroup();
                if (_userDeckSaveGroups == null) {
                    _userDeckSaveGroups = _contentsUserDeckSaveGroups[contentName];
                }
                AddCurUserDeckGroup(groupID, _curUserDeckSave);
            }
        }

        if (isExistPrefs) {
            SaveUserDeck();
        }
    }

    void AddContentUserDeck(string loadUserFileData, string groupID)
    {
        JsonData jData = JsonMapper.ToObject(loadUserFileData);

        foreach (DictionaryEntry entryGroup in jData as IDictionary) {
            string groupKeyValue = (string)entryGroup.Key;
            JsonData jGroupDeck = (JsonData)entryGroup.Value;
            UserDeckSaveGroup userSaveGroup = new UserDeckSaveGroup();
            userSaveGroup.AutoMode = true;
            if (JsonDataContainsKey(jGroupDeck, "AutoMode") == true) {
                userSaveGroup.AutoMode = (bool)jGroupDeck["AutoMode"];
            }

            for (int i = 0; i < jGroupDeck["UserDefaultDeck"].Count; i++) {
                JsonData jUserDefaultDeck = jGroupDeck["UserDefaultDeck"][i];
                UserDefaultDeck inputUserDeck = new UserDefaultDeck();
                inputUserDeck.PosIndex = (int)jUserDefaultDeck["PosIndex"];
                inputUserDeck.DeckUnitType = (TroopDeployDefinitions.DeckUnitType)(int)jUserDefaultDeck["DeckUnitType"];
                if (jUserDefaultDeck["UnitID"].IsInt) {
                    inputUserDeck.UnitID = (long)(int)jUserDefaultDeck["UnitID"];
                } else if (jUserDefaultDeck["UnitID"].IsLong) {
                    inputUserDeck.UnitID = (long)jUserDefaultDeck["UnitID"];
                }
                userSaveGroup.UserDefaultDecks.Add(inputUserDeck.PosIndex, inputUserDeck);
            }

            userSaveGroup.UserDeckPositions.Clear();
            foreach (DictionaryEntry entry in jGroupDeck["UserDeckPositions"] as IDictionary) {
                string keyValue = (string)entry.Key;
                JsonData jUserDeckPos = (JsonData)entry.Value;

                UserDeckPosition inputUserDeckPos = new UserDeckPosition();
                inputUserDeckPos.FormationName = keyValue;
                for (int i = 0; i < jUserDeckPos.Count; i++) {
                    UserDeckPosInfo inputDeckPosInfo = new UserDeckPosInfo();
                    JsonData jUserDeckPosInfo = jUserDeckPos[i];
                    float posX = 0f;
                    float posY = 0f;
                    inputDeckPosInfo.PosIndex = (int)jUserDeckPosInfo["PosIndex"];

                    if (jUserDeckPosInfo["PosX"].IsInt) {
                        posX = (float)(int)jUserDeckPosInfo["PosX"];
                    } else if (jUserDeckPosInfo["PosX"].IsDouble) {
                        posX = (float)(double)jUserDeckPosInfo["PosX"];
                    }

                    if (jUserDeckPosInfo["PosY"].IsInt) {
                        posY = (float)(int)jUserDeckPosInfo["PosY"];
                    } else if (jUserDeckPosInfo["PosY"].IsDouble) {
                        posY = (float)(double)jUserDeckPosInfo["PosY"];
                    }

                    inputDeckPosInfo.OffsetPosition = new Vector2(posX, posY);
                    inputUserDeckPos.UserDeckPosInfos.Add(inputDeckPosInfo.PosIndex, inputDeckPosInfo);
                }


                userSaveGroup.UserDeckPositions.Add(keyValue, inputUserDeckPos);
            }

            if (groupKeyValue == groupID) {
                _curUserDeckSave = userSaveGroup;
            }
            AddCurUserDeckGroup(groupKeyValue, userSaveGroup);
        }

        if (_curUserDeckSave == null) {
            _curUserDeckSave = new UserDeckSaveGroup();
            AddCurUserDeckGroup(groupID, _curUserDeckSave);
        }
    }

    public void AddCurUserDeckGroup(string groupID, UserDeckSaveGroup inputSaveGroup)
    {
        if (_userDeckSaveGroups.ContainsKey(groupID)) {
            _userDeckSaveGroups[groupID] = inputSaveGroup;
            return;
        }

        if(_userDeckSaveGroups.Count > 10) {
            List<string> groupKeys = _userDeckSaveGroups.Keys.ToList();
            _userDeckSaveGroups.Remove(groupKeys[0]);
        }

        _userDeckSaveGroups.Add(groupID, inputSaveGroup);
    }

    public void AddUserDeckGroup(Dictionary<string /* Group Key */, UserDeckSaveGroup> userDeckGroup, string groupID, UserDeckSaveGroup inputSaveGroup)
    {
        if (userDeckGroup.ContainsKey(groupID)) {
            userDeckGroup[groupID] = inputSaveGroup;
            return;
        }

        if (userDeckGroup.Count > 10) {
            List<string> groupKeys = userDeckGroup.Keys.ToList();
            userDeckGroup.Remove(groupKeys[0]);
        }

        userDeckGroup.Add(groupID, inputSaveGroup);
    }

    public UserDefaultDeck GetUserDefaultDeck(int posIndex)
	{
        if (_curUserDeckSave.UserDefaultDecks.ContainsKey(posIndex))
            return _curUserDeckSave.UserDefaultDecks[posIndex];

        return null;
	}

    public UserDefaultDeck GetUserWaveDefaultDeck(int waveIndex, int posIndex)
    {
        if (_curUserDeckSave.UserWaveDefaultDecks.ContainsKey(waveIndex) && _curUserDeckSave.UserWaveDefaultDecks[waveIndex].ContainsKey(posIndex)) {
            return _curUserDeckSave.UserWaveDefaultDecks[waveIndex][posIndex];
        }

        return null;
    }

    public string GetUserDeckFileName()
    {
        return string.Format("UserDeck_{0}_{1}.dat", GameSystem.Instance.Data.User.userData.nickname, GameSystem.Instance.SystemData.SystemOption.LocalServer.ToString());
    }

    public string GetUserDeckFileNameByUserId()
    {
        return string.Format("UserDeck_{0}_{1}.dat", ChatHelper.PlayerId, GameSystem.Instance.SystemData.SystemOption.LocalServer.ToString());
    }

    #endregion

    #region IDeckSettingObserver

    void IDeckSettingObserver.OnSetDeckInfo (int waveIndex, int posIndex, TroopDeployDefinitions.DeckUnitType deckUnitType, long unitID, HeroInfo unitInfo)
	{
        Dictionary<int /* PosIndex */, UserDefaultDeck> defaultDecks = null;
        if (TroopDeployInfoManager.isAllyWave) {
            if (_curUserDeckSave.UserWaveDefaultDecks.ContainsKey(waveIndex)) {
                defaultDecks = _curUserDeckSave.UserWaveDefaultDecks[waveIndex];
            } else {
                defaultDecks = new Dictionary<int, UserDefaultDeck>();
                _curUserDeckSave.UserWaveDefaultDecks.Add(waveIndex, defaultDecks);
            }
        } else {
            defaultDecks = _curUserDeckSave.UserDefaultDecks;
        }
        
        UserDefaultDeck userDeck = null;
		bool isExist = false;
        if (defaultDecks.ContainsKey(posIndex)) {
            userDeck = defaultDecks[posIndex];
            isExist = true;
		}

		if (unitID != -1) {
            List<int> deckKeys = defaultDecks.Keys.ToList();
            for (int i = 0; i < deckKeys.Count; i++) {
                if(defaultDecks[deckKeys[i]].UnitID == unitID) {
                    defaultDecks.Remove(deckKeys[i]);
                    break;
				}
			}
		}

        switch (deckUnitType) {
            case TroopDeployDefinitions.DeckUnitType.UserHave: {
                    if (userDeck == null)
                        userDeck = new UserDefaultDeck();
                    userDeck.PosIndex = posIndex;
                    userDeck.DeckUnitType = deckUnitType;
                    userDeck.UnitID = unitID;

                    if (!isExist) {
                        defaultDecks.Add(posIndex, userDeck);
                    }
                }
                break;
            case TroopDeployDefinitions.DeckUnitType.Empty: {
                    if (isExist) {
                        defaultDecks.Remove(posIndex);
                    }
                }
                break;
        }
    }

	#endregion

	#region IDeckChangePosObserver

	void IDeckChangePosObserver.OnChangeDeckPosition (int waveIndex, int posIndex, string formationName, Vector2 offsetPosition)
	{
        if (_curUserDeckSave.UserDeckPositions.ContainsKey(formationName)) {
            UserDeckPosition userDeckPos = _curUserDeckSave.UserDeckPositions[formationName];
            if (userDeckPos.UserDeckPosInfos.ContainsKey (posIndex)) {
                if (TroopDeployInfoManager.Instance.IsRevisionIconPos) {
                    userDeckPos.UserDeckPosInfos[posIndex].OffsetPosition = new Vector2(offsetPosition.x, offsetPosition.y);
                } else {
                    userDeckPos.UserDeckPosInfos [posIndex].OffsetPosition = offsetPosition;
                }
			} else {
				UserDeckPosInfo inputDeckPosInfo = new UserDeckPosInfo ();
				inputDeckPosInfo.PosIndex = posIndex;
                if (TroopDeployInfoManager.Instance.IsRevisionIconPos) {
                    Vector2 preOffsetPos = inputDeckPosInfo.OffsetPosition;
                    inputDeckPosInfo.OffsetPosition = new Vector2(offsetPosition.x, offsetPosition.y);
                } else {
                    inputDeckPosInfo.OffsetPosition = offsetPosition;
                }
                
				userDeckPos.UserDeckPosInfos.Add (posIndex, inputDeckPosInfo);
			}

		} else {
			UserDeckPosition userDeckPos = new UserDeckPosition ();

			UserDeckPosInfo inputDeckPosInfo = new UserDeckPosInfo ();
			inputDeckPosInfo.PosIndex = posIndex;
            if (TroopDeployInfoManager.Instance.IsRevisionIconPos) {
                Vector2 preOffsetPos = inputDeckPosInfo.OffsetPosition;
                inputDeckPosInfo.OffsetPosition = new Vector2(offsetPosition.x, offsetPosition.y);
            } else {
                inputDeckPosInfo.OffsetPosition = offsetPosition;
            }
            userDeckPos.UserDeckPosInfos.Add (posIndex, inputDeckPosInfo);

            _curUserDeckSave.UserDeckPositions.Add(formationName, userDeckPos);
        }

	}

	#endregion
}
