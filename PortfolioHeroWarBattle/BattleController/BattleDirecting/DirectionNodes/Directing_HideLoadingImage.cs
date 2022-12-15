using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Directing_HideLoadingImage : BaseDirecting
{
    public Directing_HideLoadingImage(Action complete = null) : base(complete)
    {
    }

    public override void Execute()
    {
        UIHelpScreenUGUI.DestroySelf();
        Complete();
    }
}
