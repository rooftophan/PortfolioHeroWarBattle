using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NewBattleCore;
using DG.Tweening;

#region before_Refactorying
//public class Directing_FadeOutIn : BaseDirecting
//{
//    private BattleFadingManager _fading;
//    private float _fadeTime;
//    private float _fadeOutTime;
//    private bool _fadeIn;

//    public Directing_FadeOutIn(Action complete, float fadeTime = 0.3f, float fadeOutTime = 0f, Action onFadeChange = null) : base(complete)
//    {
//        _fading = new BattleFadingManager();
//        _fadeTime = fadeTime;
//        _fadeOutTime = fadeOutTime;
//        _fadeIn = false;
//        _fading.OnMiddleChangeFading = onFadeChange;
//        _fading.OnFinishedFading = OnFinishFading;
//    }

//    public override void Execute()
//    {
//        _fading.InitBattleFading(_fadeIn, _fadeTime);

//        var seq = DOTween.Sequence();
//        seq.SetUpdate(false);

//        seq.OnUpdate(()=> {

//            if (_fading.IsRelease)
//                return;

//            _fading.Update();
//            if (_fading.IsRelease)
//            {
//                seq.Kill(true);
//                Complete();
//            }

//        });
//        seq.AppendInterval(float.MaxValue);

//        seq.Play();
//    }

//    private void OnFinishFading()
//    {
//        _fading.ReleaseBattleFading();
//    }
//}
#endregion

public class Directing_FadeOutIn : Directing_Group
{
    public Directing_FadeOutIn(Action complete, float fadeOutTime = 0f, float fadeInTime = 0.3f, Action onFadeChange = null, Action onCompleteFadeOut = null, float delayTime = 0f) : base(complete)
    {
        EnqueDirecting(new Directing_Fade(onCompleteFadeOut, false, fadeOutTime));
        if(delayTime > 0f) {
            EnqueDirecting(new Directing_Wait(null, delayTime));
        }
        EnqueDirecting(new Directing_CallBack(null, onFadeChange));
        EnqueDirecting(new Directing_Fade(null, true, fadeInTime));
    }

}
