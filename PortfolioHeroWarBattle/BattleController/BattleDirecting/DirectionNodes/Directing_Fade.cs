using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Directing_Fade : BaseDirecting
{
    private float _fadeTime;
    private bool _fadeIn;

    static private GameObject _s_fadeObject = null;
    private Image _fadeImg = null;

    #region before_Refactorying
    
    //private BattleFadingManager _fading;

    //public Directing_Fade(Action complete, bool fadein, float fadeTime = 0.3f) : base(complete)
    //{
    //    _fading = new BattleFadingManager();

    //    _fadeTime = fadeTime;
    //    _fadeIn = fadein;

    //}

    //public override void Execute()
    //{
    //    _fading.InitBattleFading(_fadeIn, _fadeTime);

    //    var seq = DOTween.Sequence();
    //    seq.SetUpdate(false);

    //    seq.OnUpdate(() =>
    //    {
    //        if (_fading.IsRelease)
    //            return;

    //        if (_fading.UpdateFading() == 1)
    //        {
    //            _fading.ReleaseBattleFading();
    //            seq.Kill(true);
    //            Complete();
    //        }

    //    });


    //    seq.AppendInterval(float.MaxValue);

    //    seq.Play();
    //}

    #endregion


    public Directing_Fade(Action complete, bool fadein, float fadeTime = 0.3f) : base(complete)
    {
        _fadeTime = fadeTime;
        _fadeIn = fadein;
    }

    public override void Execute()
    {
        CreateFadeObj();

        float fadeValue = (_fadeIn) ? 0f : 1f;
     //   _fadeImg.color = new Color(0f, 0f, 0f, 1f - fadeValue);

        var seq = DOTween.Sequence();
        seq.SetUpdate(false);
        var fadeSeq = _fadeImg.DOFade(fadeValue, _fadeTime);
        seq.Append(fadeSeq);
        seq.AppendCallback(Complete);

        seq.Play();
    }

    private void CreateFadeObj()
    {
        if(_s_fadeObject == null)
            _s_fadeObject = ResourceLoader.Instantiate(BattleConfig.DirectingFadeInOut, typeof(GameObject), null) as GameObject;

        _fadeImg = _s_fadeObject.GetComponentInChildren<Image>();
    }

    protected override void Complete()
    {
        base.Complete();

        if (_fadeIn)
        {
            GameObject.Destroy(_s_fadeObject);
            _s_fadeObject = null;
        }
    }
}
