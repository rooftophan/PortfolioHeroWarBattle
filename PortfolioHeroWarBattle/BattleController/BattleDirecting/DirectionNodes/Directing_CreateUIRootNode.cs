using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Directing_CreateUIRootNode : BaseDirecting
{
    public Directing_CreateUIRootNode(Action complete = null) : base(complete)
    {
    }

    public override void Execute()
    {
        UIRootPreset.RootType Root = UIRootPreset.RootType.FX;
        string Layer = "FX";

        var name = UIRootPreset.GetName(Root);

        var rootUGUI = GameObject.Find(name);

        if (rootUGUI == null)
        {
            rootUGUI = new GameObject(name);
            var canvas = rootUGUI.AddMissingComponent<Canvas>();
            var canvasScaler = rootUGUI.AddMissingComponent<CanvasScaler>();
            var canvasCamObj = new GameObject("Camera");
            var canvasCam = canvasCamObj.AddMissingComponent<Camera>();

            rootUGUI.AddMissingComponent<GraphicRaycaster>();
            canvasCamObj.transform.SetParent(rootUGUI.transform);

            canvasCam.clearFlags = CameraClearFlags.Depth;
            canvasCam.orthographic = true;
            canvasCam.nearClipPlane = -1f;
            canvasCam.farClipPlane = 1f;
            canvasCam.orthographicSize = 1f;

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.planeDistance = 100f;
            canvas.worldCamera = canvasCam;

            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1280f, 720f);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

            LayerUtil.ChangeRecusiveLayer(rootUGUI, LayerMask.NameToLayer(Layer));
        }

        Complete();
    }
}
