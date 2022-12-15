using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NewBattleCore;
using UnityEngine.UI;

public class FriendUnitBaseInfo
{
    #region Variables

    long _userID;
    string _userNickname;
    long _heroID;
    HeroInfo _unitInfo = null;

    #endregion

    #region Properties

    public long UserID
    {
        get { return _userID; }
        set { _userID = value; }
    }

    public string UserNickname
    {
        get { return _userNickname; }
        set { _userNickname = value; }
    }

    public long HeroID
    {
        get { return _heroID; }
        set { _heroID = value; }
    }

    public HeroInfo UnitInfo
    {
        get { return _unitInfo; }
        set { _unitInfo = value; }
    }

    #endregion
}

public class CompanyMercenaryInfo : FriendUnitBaseInfo
{
    #region Variables

    long _companyID;

    #endregion

    #region Properties

    public long CompanyID
    {
        get { return _companyID; }
        set { _companyID = value; }
    }

    #endregion
}