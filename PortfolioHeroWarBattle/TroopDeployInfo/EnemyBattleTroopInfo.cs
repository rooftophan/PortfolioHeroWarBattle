using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBattleTroopInfo : BattleBaseDeckInfo
{
	#region Variables

	Vector3 _unitPosition;
	Vector3 _unitPositionTo;
	Vector2 _offsetPosition = Vector2.zero;

	#endregion

	#region Properties

	public Vector3 UnitPosition
	{
		get{ return _unitPosition; }
		set{ _unitPosition = value; }
	}

	public Vector3 UnitPositionTo
	{
		get{ return _unitPositionTo; }
		set{ _unitPositionTo = value; }
	}

	public Vector2 OffsetPosition
	{
		get{ return _offsetPosition; }
		set{ _offsetPosition = value; }
	}

	public Vector2 Position
	{
		get{ return new Vector2 (_unitPosition.x + _offsetPosition.x, _unitPosition.z + _offsetPosition.y); }
	}

	#endregion

	#region Methods

	#endregion
}
