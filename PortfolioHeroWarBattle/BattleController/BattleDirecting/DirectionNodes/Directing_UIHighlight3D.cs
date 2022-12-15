using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;


public class Directing_UIHighlight3D : BaseDirecting
{

    private UICommonHighlighter3D _highlighter;
    private Transform _target;
    private RectTransform[] _targets;
    
    private List<Action> _reserveActions;

    private bool _isReady = false;
    private bool _autoClose;

    private static Directing_UIHighlight3D _curHighlighter;

    public static void ForcedDispose()
    {
        if (_curHighlighter != null)
        {
            _curHighlighter.Close();
            _curHighlighter = null;
        }
    }
    
    public Directing_UIHighlight3D(Transform target, bool autoClose = true, Action complete = null) : base(complete)
    {
        _target = target;
        _reserveActions = new List<Action>();
        _isReady = false;
        _autoClose = autoClose;
        _curHighlighter = this;
    }
    
    public Directing_UIHighlight3D(RectTransform[] targets, bool autoClose = true, Action complete = null) : base(complete)
    {
        _targets        = targets;
        _reserveActions = new List<Action>();
        _isReady        = false;
        _autoClose      = autoClose;
        _curHighlighter = this;
    }

    public static void CopyButtonProcess(Directing_UIHighlight3D director, Transform origin, Transform copy)
    {
        var originBtn = origin.GetComponent<Button>();
        if (originBtn == null)
            originBtn = origin.GetComponentInChildren<Button>();

        var copyBtn = copy.GetComponent<Button>();
        if (copyBtn == null)
            copyBtn = copy.GetComponentInChildren<Button>();

        copyBtn.onClick = originBtn.onClick;
        copyBtn.onClick.AddListener(() =>
        {
            director.Close();
        });
    }

    public static void CopyButtonFuctionProcess(Directing_UIHighlight3D director, Transform origin, Transform copy)
    {
        var originBtn = origin.GetComponent<Button>();
        if (originBtn == null)
            originBtn = origin.GetComponentInChildren<Button>();

        bool isImageExist = copy.gameObject.GetComponent<Image>() != null;

        var copyBtn = copy.gameObject.AddMissingComponent<Button>();
        var image = copy.gameObject.AddMissingComponent<Image>();

        if(!isImageExist)
            image.color = Color.clear;

        copyBtn.onClick = originBtn.onClick;
        copyBtn.onClick.AddListener(() =>
        {
            director.Close();
        });
    }

    public static Directing_UIHighlight3D ProcessFocus(Component component, bool isHard = true, string desc = "", BoxEdge textDir = BoxEdge.Bottom, float textWidth = 300.0f, float textHieght = 100.0f, bool useArrow = true, BoxEdge arrowDir = BoxEdge.Right, Action complete = null)
    {
        var rcTrans = component.GetComponent<RectTransform>();
        var highlighter = new Directing_UIHighlight3D(rcTrans, !isHard, complete);
        
        if (useArrow)
            highlighter.AddHighlightArrow(rcTrans, arrowDir);
        highlighter.AddText(rcTrans, new Vector2(textWidth, textHieght), textDir, desc);

        if (isHard)
            highlighter.AddCopyOnlyFunction(rcTrans, Directing_UIHighlight3D.CopyButtonFuctionProcess);
        highlighter.Execute();

        return highlighter;
    }

    public static Directing_UIHighlight3D ProcessFocusWithText(Component component, bool isHard, string desc, BoxEdge textDir, float textWidth, float textHieght, Action complete)
    {
        return ProcessFocus(component, isHard, desc, textDir, textWidth, textHieght, false, BoxEdge.Bottom, complete);
    }
    public static Directing_UIHighlight3D ProcessFocusWithArrow(Component component, bool isHard, BoxEdge arrowDir, Action complete)
    {
        return ProcessFocus(component, isHard, "", BoxEdge.Bottom, 300, 100, true, arrowDir, complete);
    }

    public static Directing_UIHighlight3D ProcessFocus(Component component, bool isHard, Action complete)
    {
        return ProcessFocus(component, isHard, "", BoxEdge.Bottom, 300, 100, false, BoxEdge.Bottom, complete);
    }
    
    public static Directing_UIHighlight3D ProcessMultiFocus(bool isHard, Action complete, params Component[] components)
    {
        RectTransform[] rcTransArray = new RectTransform[components.Length];

        for (int i = 0; i < components.Length; i++)
        {
            rcTransArray[i] = components[i].GetComponent<RectTransform>();
        }

        var highlighter = new Directing_UIHighlight3D(rcTransArray, !isHard, complete);
        highlighter.Execute();

            
        return highlighter;
    }
    

    public override void Execute()
    {
        Execute(true);
    }

    public void Execute(bool defaultSet)
    {
        if (_target != null)
        {
            _highlighter = ResourceLoader.Instantiate<UICommonHighlighter3D>(Res.PREFAB.UICommonHighlighter3D);

            var canvasScaler = _target.GetComponentInParent<CanvasScaler>();
            if (canvasScaler != null)
            {
                _highlighter.GetComponent<CanvasScaler>().screenMatchMode    = canvasScaler.screenMatchMode;
                _highlighter.GetComponent<CanvasScaler>().matchWidthOrHeight = canvasScaler.matchWidthOrHeight;
            }

            _highlighter.GetComponent<CanvasScaler>().SendMessage("Update");

            var canvas       = _highlighter.GetComponentInParent<Canvas>();
            var targetCanvas = _target.GetComponentInParent<Canvas>();

            canvas.transform.position = targetCanvas.transform.position;
            canvas.transform.rotation = targetCanvas.transform.rotation;
            var cam       = canvas.worldCamera;
            var targetCam = targetCanvas.worldCamera;



            cam.transform.Copy(targetCam.transform);
            cam.cullingMask      = targetCam.cullingMask;
            cam.orthographic     = targetCam.orthographic;
            cam.orthographicSize = targetCam.orthographicSize;
            cam.fieldOfView      = targetCam.fieldOfView;

            _highlighter.CloseByTouch = _autoClose;
            _isReady                  = false;
            _highlighter.onClose      = Complete;

            if (defaultSet)
            {
                AddMaskingRect(_target);
                AddHighlightBox(_target);
            }

            new Directing_WaitForFrame(Process, 1).Execute();
        }
        else if (_targets != null)
        {
            _highlighter = ResourceLoader.Instantiate<UICommonHighlighter3D>(Res.PREFAB.UICommonHighlighter3D);

            for (int i = 0; i < _targets.Length; i++)
            {
                var canvasScaler = _targets[i].GetComponentInParent<CanvasScaler>();
                if (canvasScaler != null)
                {
                    _highlighter.GetComponent<CanvasScaler>().screenMatchMode    = canvasScaler.screenMatchMode;
                    _highlighter.GetComponent<CanvasScaler>().matchWidthOrHeight = canvasScaler.matchWidthOrHeight;
                }

                _highlighter.GetComponent<CanvasScaler>().SendMessage("Update");

                var canvas       = _highlighter.GetComponentInParent<Canvas>();
                var targetCanvas = _targets[i].GetComponentInParent<Canvas>();

                canvas.transform.position = targetCanvas.transform.position;
                canvas.transform.rotation = targetCanvas.transform.rotation;
                var cam       = canvas.worldCamera;
                var targetCam = targetCanvas.worldCamera;



                cam.transform.Copy(targetCam.transform);
                cam.cullingMask      = targetCam.cullingMask;
                cam.orthographic     = targetCam.orthographic;
                cam.orthographicSize = targetCam.orthographicSize;
                cam.fieldOfView      = targetCam.fieldOfView;

                _highlighter.CloseByTouch = _autoClose;
                _isReady                  = false;
                _highlighter.onClose      = Complete;

                if (defaultSet)
                {
                    AddMaskingRect(_targets[i]);
                    AddHighlightBox(_targets[i]);
                }
            }
            
            new Directing_WaitForFrame(Process, 1).Execute();
        }
      
    }

    private void Process()
    {
        _isReady = true;

        foreach (var each in _reserveActions)
            each();

        _reserveActions.Clear();
    }

    public void AddMaskingRect(Transform targetRcTrans)
    {
        if (!_isReady)
        {
            _reserveActions.Add(() => AddMaskingRect(targetRcTrans));
            return;
        }

        _highlighter.AddMaskingRect(targetRcTrans);
    }

    public void AddCopyObject(Transform target, Action<Directing_UIHighlight3D, Transform, Transform> onComplete)
    {
        if (!_isReady)
        {
            _reserveActions.Add(() => AddCopyObject(target, onComplete));
            return;
        }

        var copyObj = GameObject.Instantiate(target.gameObject).GetComponent<RectTransform>();
        _highlighter.AddObject(target, copyObj);

        if (onComplete != null)
            onComplete(this, target, copyObj);
    }

    public void AddCopyOnlyFunction(Transform target, Action<Directing_UIHighlight3D, Transform, Transform> onComplete)
    {
        if (!_isReady)
        {
            _reserveActions.Add(() => AddCopyOnlyFunction(target, onComplete));
            return;
        }
        
        var copyObj = new GameObject().AddMissingComponent<RectTransform>();
        _highlighter.AddObject(target, copyObj);

        if (onComplete != null)
            onComplete(this, target, copyObj);
    }

    public void AddHighlightBox(Transform targetRcTrans)
    {
        if (!_isReady)
        {
            _reserveActions.Add(() => AddHighlightBox(targetRcTrans));
            return;
        }

        _highlighter.AddHighlightBox(targetRcTrans);
    }

    public void AddHighlightArrow(Transform targetRcTrans, BoxEdge edge)
    {
        if (!_isReady)
        {
            _reserveActions.Add(() => AddHighlightArrow(targetRcTrans, edge));
            return;
        }

        _highlighter.AddHighlightArrow(targetRcTrans, edge);
    }

    public void AddText(Transform targetRcTrans, Vector2 size, BoxEdge edge, string text, Action<RectTransform> onComplete = null, TextAlign align = TextAlign.MiddleCenter)
    {
        if (string.IsNullOrEmpty(text))
            return;

        if (!_isReady)
        {
            _reserveActions.Add(() => AddText(targetRcTrans, size, edge, text, onComplete, align));
            return;
        }

        var uiText = _highlighter.AddText(targetRcTrans, size, edge, text, align);

        if (onComplete != null)
            onComplete(uiText.transform.parent.rectTransform());

    }

    public void AddTextFlat(Transform targetRcTrans, Vector2 size, BoxEdge edge, string text, Action<RectTransform> onComplete = null, TextAlign align = TextAlign.MiddleCenter)
    {
        if (string.IsNullOrEmpty(text))
            return;

        if (!_isReady)
        {
            _reserveActions.Add(() => AddTextFlat(targetRcTrans, size, edge, text, onComplete, align));
            return;
        }

        var uiText = _highlighter.AddText(targetRcTrans, size, edge, text, align, true);

        if (onComplete != null)
            onComplete(uiText.transform.parent.rectTransform());

    }

    public void Close(bool isForced = false)
    {
        if (_highlighter != null)
        {
            if (isForced)
                _highlighter.onClose = null;
            
            _highlighter.Destroy();
            _highlighter = null;
        }
    }

    protected override void Complete()
    {
        _reserveActions = null;
        _target = null;
        _targets = null;

        base.Complete();
    }
}
