using System;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class Directing_UIBlur : BaseDirecting
{
    public Directing_UIBlur(Action complete = null) : base(complete) { }

    public override void Execute()
    {
        var cams = GetCams();
        //Blur 
        for (int i = 0; i < cams.Count; i++)
            CamProcessing(cams[i]);

        Complete();
    }

    private List<Camera> GetCams()
    {
        var allCanvas = GameObject.FindObjectsOfType<Canvas>();

        var result = new List<Camera>();

        //Blur 
        for (int i = 0; i < allCanvas.Length; i++)
        {
            var camera = allCanvas[i].GetComponentInChildren<Camera>();
            result.Add(camera);
        }

        return result;

    }

    protected virtual void CamProcessing(Camera camera)
    {
        if (camera == null)
            return;

        var blur = camera.gameObject.AddMissingComponent<BlurOptimized>();
        blur.blurShader = Shader.Find("Hidden/FastBlur");
        if (blur != null)
            blur.enabled = true;
    }

}
