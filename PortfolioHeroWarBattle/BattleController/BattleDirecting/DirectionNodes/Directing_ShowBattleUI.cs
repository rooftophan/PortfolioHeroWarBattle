using System;
using NewBattleCore;

public class Directing_ShowBattleUI : BaseDirecting
{
    public Directing_ShowBattleUI(Action complete = null) : base(complete)
    {
    }

    public override void Execute()
    {
        UIBattle.This.ShowBattle( true );
        UIBattle.This.SetTopButtonEnable(true);
        //BattleManager.Instance.scene.UI.gameObject.SetActive(true);
        
        Complete();
    }
}
