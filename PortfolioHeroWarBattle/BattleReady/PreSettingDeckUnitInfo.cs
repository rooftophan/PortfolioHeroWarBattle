using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreSettingDeckUnitInfo
{
	#region Variables

	int _posIndex;
	TroopDeployDefinitions.DeckUnitType _deckUnitType;
	HeroInfo _unitInfo;
	int _unitIndex;
	int _level = 1;
	int _difficultyType = 1;

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

	public HeroInfo UnitInfo
	{
		get{ return _unitInfo; }
		set{ _unitInfo = value; }
	}

	public int UnitIndex
	{
		get{ return _unitIndex; }
		set{ _unitIndex = value; }
	}

	public int Level
	{
		get{ return _level; }
		set{ _level = value; }
	}

	public int DifficultyType
	{
		get{ return _difficultyType; }
		set{ _difficultyType = value; }
	}

	#endregion
}
