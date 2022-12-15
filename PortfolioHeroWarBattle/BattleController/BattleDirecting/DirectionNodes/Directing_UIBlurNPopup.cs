using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Directing_UIBlurNPopup : BaseDirecting
{
    private string _descriptionText;
    private string _buttonText;
    
    public Directing_UIBlurNPopup(Action onComplete, string descriptionText, string buttonText) : base(onComplete)
    {
        _complete = onComplete;
        _descriptionText = descriptionText;
        _buttonText = buttonText;
    }


    public override void Execute()
    {
        new Directing_UIBlur().Execute();

        //PrefabLoad
        var popup = ResourceLoader.Instantiate<UITutorialSystemPopup>(Res.PREFAB.UITutorialSystemPopup);
        popup.SetConfirmAction(Complete);
        popup.SetDescriptionText(_descriptionText);
        popup.SetButtonText(_buttonText);
    }

    protected override void Complete()
    {
        new Directing_UIBlurRemove().Execute();
        base.Complete();
    }

}
