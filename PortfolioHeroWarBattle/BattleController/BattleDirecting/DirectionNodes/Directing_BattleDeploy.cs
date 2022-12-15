using System;
using System.Collections;
using System.Collections.Generic;
using NewBattleCore;

public class Directing_BattleDeploy : BaseDirecting
{

    private bool _moveAlly;
    private bool _moveAllyMinion;
    private bool _moveEnemy;
    private bool _moveEnemyMinion;

    public Directing_BattleDeploy(Action complete, bool moveAlly, bool moveAllyMinion, bool moveEnemy, bool moveEnemyMinion) : base(complete)
    {
        _moveAlly = moveAlly;
        _moveAllyMinion = moveAllyMinion;
        _moveEnemy = moveEnemy;
        _moveEnemyMinion = moveEnemyMinion;
    }

    public override void Execute()
    {
        BattleManager.Instance.scene.BCHandler.DeployUnits(_moveAlly, _moveAllyMinion, _moveEnemy, _moveEnemyMinion, Complete);
    }
}
