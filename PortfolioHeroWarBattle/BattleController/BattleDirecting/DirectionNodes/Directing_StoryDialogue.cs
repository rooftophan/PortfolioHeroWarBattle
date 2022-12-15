using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Framework;
using Framework.Controller;
using Controller;
using System;


public class Directing_StoryDialogue : BaseDirecting
{
    private GameSystem _system;
    private string _res;
    private bool _skipButtonOn;
    private float _endDelay;

    private string _restoreBGM;
    
    public Directing_StoryDialogue(string res, bool skipButtonOn, Action complete = null, float endDelay = 0f) : base(complete)
    {
        _system = GameSystem.Instance;
        _res = res;
        _skipButtonOn = skipButtonOn;
        _endDelay = endDelay;

        if ( _res != null )
        {
            if (_res.Equals("Tutorial/BT_Tutorial_01"))
                UserLogSystem.Instance.SetLog(DWTutorialType.Start_Scenario_1);
//            else if( _res.Equals("Tutorial/BT_Tutorial_02"))
//                UserLogSystem.Instance.SetLog(DWTutorialType.Start_Guide_Equipment);
            else if( _res.Equals("Tutorial/BT_Tutorial_03"))
                UserLogSystem.Instance.SetLog(DWTutorialType.Start_Guide_Hero_Contract);
//            else if( _res.Equals("Tutorial/BT_Tutorial_04"))
//                UserLogSystem.Instance.SetLog(DWTutorialType.Start_Guide_FuryCard);
            else if( _res.Equals("Tutorial/BT_Tutorial_06"))
                UserLogSystem.Instance.SetLog(DWTutorialType.Start_Event_Guide);
            else if( _res.Equals("Tutorial/BT_Tutorial_07"))
                UserLogSystem.Instance.SetLog(DWTutorialType.Start_Card_Gacha);
            else if( _res.Equals("Tutorial/BT_Tutorial_09"))
                UserLogSystem.Instance.SetLog(DWTutorialType.Guide_race_Trivia);
            // else if( _res.Equals("Tutorial/BT_Tutorial_08"))
            //     UserLogSystem.Instance.SetLog(DWTutorialType.Start_Card_Disassemble);
//            else if( _res.Equals("Tutorial/BT_Tutorial_10"))
//                UserLogSystem.Instance.SetLog(DWTutorialType.Start_Card_Enhancement);
            else if( _res.Equals("Tutorial/BT_Tutorial_SearchPoint"))
                UserLogSystem.Instance.SetLog(DWTutorialType.Guide_Trace_Croman);
            else if( _res.Equals("Tutorial/BT_Tutorial_BattleCenter"))
                UserLogSystem.Instance.SetLog(DWTutorialType.Guide_BattleCenter_Open);
            else if( _res.Equals("Tutorial/BT_Tutorial_Arena"))
                UserLogSystem.Instance.SetLog(DWTutorialType.Guide_UserArena);
            else if(_res.Equals("Tutorial/BT_Tutorial_Party"))
                UserLogSystem.Instance.SetLog(DWTutorialType.Guide_Party);
        }
    }

    public override void Execute()
    {
        if (_res != null)
        {
            _restoreBGM = _system.AudioController.BgmAudioController.CurrentClipName;
            new LoadDialogue(_system, _res, _skipButtonOn, () =>
            {
                new UnloadDialogue(() =>
                {
                    if (_endDelay <= 0f)
                        Complete();
                    else
                        new Directing_Wait(Complete, _endDelay).Execute();
                    
                }).Execute();
            }).Execute();
        }
        else
            Complete();
    }
    
    protected override void Complete()
    {
        var bgmController = _system.AudioController.BgmAudioController;
        if(_restoreBGM != _system.AudioController.BgmAudioController.CurrentClipName)
            bgmController.Play(BGMAudioController.ChangeBgmType.FadeStopNPlay, _restoreBGM);
        base.Complete();
    }

}
