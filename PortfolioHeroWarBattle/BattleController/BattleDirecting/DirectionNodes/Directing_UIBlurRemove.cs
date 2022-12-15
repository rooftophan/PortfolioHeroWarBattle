using System;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class Directing_UIBlurRemove : Directing_UIBlur
{
    public Directing_UIBlurRemove(Action complete = null) : base(complete) { }

    protected override void CamProcessing(Camera camera)
    {
        if (camera == null)
            return;
        var blur = camera.gameObject.GetComponent<BlurOptimized>();

        if (blur != null)
            GameObject.DestroyImmediate(blur);
    }
}
