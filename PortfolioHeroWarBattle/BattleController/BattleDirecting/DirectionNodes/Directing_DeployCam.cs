using System;
using System.Collections;
using NewBattleCore;
using UnityEngine.UI;
using Controller;
using UnityEngine;

public class Directing_DeployCam : BaseDirecting
{
    //BattleScene scene;
    public Directing_DeployCam(BattleScene scene, Action complete = null) : base(complete)
    {
        //this.scene = scene;
    }

    public override void Execute()
    {
        Debug.Log( "DeployCam" );
        BattleManager.Instance.battleCamera.StartDeployWalking();
        Complete();
    }


}
