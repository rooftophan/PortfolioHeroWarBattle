using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleBaseDeckInfo
{
	#region Variables

	protected int _posIndex;
	protected HeroInfo _unitInfo = null;

	#endregion

	#region Properties

	public int PosIndex
	{
		get{ return _posIndex; }
		set{ _posIndex = value; }
	}

	public HeroInfo UnitInfo
	{
		get{ return _unitInfo; }
		set{ _unitInfo = value; }
	}

	#endregion
}

public class UserBattleDeck : BattleBaseDeckInfo
{
	#region Variables

	TroopDeployDefinitions.DeckUnitType _deckUnitType;
	long _unitID;
	bool _isHideIcon = false;
	bool _isEnableTouch = true;

	#endregion

	#region Properties

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

	public bool IsHideIcon
	{
		get{ return _isHideIcon; }
		set{ _isHideIcon = value; }
	}

	public bool IsEnableTouch
	{
		get{ return _isEnableTouch; }
		set{ _isEnableTouch = value; }
	}

	#endregion

	#region Methods

	public void CopyUserBattleDeck(UserBattleDeck userBattle)
	{
		this._posIndex = userBattle.PosIndex;
		this._unitInfo = userBattle.UnitInfo;
		this._deckUnitType = userBattle.DeckUnitType;
		this._unitID = userBattle.UnitID;
		this._isHideIcon = userBattle.IsHideIcon;
		this._isEnableTouch = userBattle.IsEnableTouch;
	}

	#endregion
}

public class UserBattleDeckFormation
{
	#region Variables

	int _waveIndex;

	string _allyFormationName;

	Dictionary<int /* Pos Index */, UserDeckPosInfo> _userDeckPositionInfos = new Dictionary<int, UserDeckPosInfo> ();

	Dictionary<int /* Pos Index */, UserBattleDeck> _addBattleDecks = new Dictionary<int, UserBattleDeck>();

	#endregion

	#region Properties

	public int WaveIndex
	{
		get{ return _waveIndex; }
		set{ _waveIndex = value; }
	}

	public string AllyFormationName
	{
		get{ return _allyFormationName; }
		set{ _allyFormationName = value; }
	}

	public Dictionary<int /* Pos Index */, UserDeckPosInfo> UserDeckPositionInfos
	{
		get{ return _userDeckPositionInfos; }
	}

	public Dictionary<int /* Pos Index */, UserBattleDeck> AddBattleDecks
	{
		get{ return _addBattleDecks; }
	}

	#endregion
}
