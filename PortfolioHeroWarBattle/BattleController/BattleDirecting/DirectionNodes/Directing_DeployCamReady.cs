using System;
using System.Collections;
using NewBattleCore;
using UnityEngine.UI;
using Controller;
using UnityEngine;

public class Directing_DeployCamReady : BaseDirecting
{
    private string _text;
    //BattleScene scene;
    public Directing_DeployCamReady( BattleScene scene, Action complete = null) : base(complete)
    {
        //this.scene = scene;
    }

    public override void Execute()
    {
        //UIRootPreset.RootType Root = UIRootPreset.RootType.FX;        
        BattleManager.Instance.battleCamera.StartDeployWalking();
        Complete();
    }


}
