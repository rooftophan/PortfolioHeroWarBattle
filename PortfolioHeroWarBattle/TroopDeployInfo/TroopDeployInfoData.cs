using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormationFixedHeroInfo
{
	#region Variables

	int _heroIndex;
	int _heroLevel;
	int _difficultyType = 1;
	sbyte _formationIndex;
	Vector3 _heroPosition;
	int _heroPower;

	#endregion

	#region Properties

	public int HeroIndex
	{
		get{ return _heroIndex; }
		set{ _heroIndex = value; }
	}

	public int HeroLevel
	{
		get{ return _heroLevel; }
		set{ _heroLevel = value; }
	}

	public int DifficultyType
	{
		get{ return _difficultyType; }
		set{ _difficultyType = value; }
	}

	public sbyte FormationIndex
	{
		get{ return _formationIndex; }
		set{ _formationIndex = value; }
	}

	public Vector3 HeroPosition
	{
		get{ return _heroPosition; }
		set{ _heroPosition = value; }
	}

	public int HeroPower
	{
		get{ return _heroPower; }
		set{ _heroPower = value; }
	}

	#endregion
}

public class TroopDeployPositionInfo
{
	#region Variables

	int _posIndex;
	float _offsetPosX;
	float _offsetPosY;

	#endregion

	#region Properties

	public int PosIndex
	{
		get{ return _posIndex; }
		set{ _posIndex = value; }
	}

	public float OffsetPosX
	{
		get{ return _offsetPosX; }
		set{ _offsetPosX = value; }
	}

	public float OffsetPosY
	{
		get{ return _offsetPosY; }
		set{ _offsetPosY = value; }
	}

	#endregion
}
