using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Directing_UIHighlight : BaseDirecting
{
    private UICommonHighlighter _highlighter;
    private RectTransform _target;
    private RectTransform[] _targets;

    private List<Action> _reserveActions;

    private bool _isReady = false;
    private bool _autoClose;

    public UICommonHighlighter UI { get { return _highlighter; } }

    public RectTransform Target => _target;

    private static Directing_UIHighlight _curHighlighter;

    public static void ForcedDispose()
    {
        if (_curHighlighter != null)
        {
            _curHighlighter.Close(true);
            _curHighlighter = null;
        }
    }
    
    
    public Directing_UIHighlight(RectTransform target, bool autoClose = true, Action complete = null) : base(complete)
    {
        _target = target;
        _reserveActions = new List<Action>();
        _isReady = false;
        _autoClose = autoClose;
        _curHighlighter = this;
    }
    
    public Directing_UIHighlight(RectTransform[] targets, bool autoClose = true, Action complete = null) : base(complete)
    {
        _targets = targets;
        _reserveActions = new List<Action>();
        _isReady        = false;
        _autoClose      = autoClose;
        _curHighlighter = this;
    }

    public static void CopyButtonProcess(Directing_UIHighlight director, RectTransform origin, RectTransform copy)
    {
        var originBtn = origin.GetComponent<Button>();
        if (originBtn == null)
            originBtn = origin.GetComponentInChildren<Button>();

        var copyBtn = copy.GetComponent<Button>();
        if(copyBtn == null)
            copyBtn = copy.GetComponentInChildren<Button>();

        if (copyBtn != null)
        {
            copyBtn.onClick = originBtn.onClick;
            copyBtn.onClick.AddListener(() =>
            {
                director.Close();
            });
        }
    }

    public static void CopyButtonFuctionProcess(Directing_UIHighlight director, RectTransform origin, RectTransform copy)
    {
        var originBtn = origin.GetComponent<Button>();
        if (originBtn == null)
            originBtn = origin.GetComponentInChildren<Button>();

        var copyBtn = copy.gameObject.AddMissingComponent<Button>();
        var image = copy.gameObject.AddMissingComponent<Image>();
        image.color = Color.clear;

        if (copyBtn != null && originBtn != null)
        {
            copyBtn.onClick = originBtn.onClick;
            copyBtn.onClick.AddListener(() =>
            {
                director.Close();
            });
        }
    }
    
    public static Directing_UIHighlight ProcessFocus(Component component, bool isHard = true, string desc = "", BoxEdge textDir = BoxEdge.Bottom, float textWidth = 300.0f, float textHieght = 100.0f, bool useArrow = true, BoxEdge arrowDir = BoxEdge.Right, Action complete = null, float scale = 1f)
    {
        var rcTrans = component.GetComponent<RectTransform>();
        var highlighter = new Directing_UIHighlight(rcTrans, !isHard, complete);

        highlighter.SetScale(scale);

        if (useArrow)
            highlighter.AddHighlightArrow(rcTrans, arrowDir);
        highlighter.AddText(rcTrans, new Vector2(textWidth, textHieght), textDir, desc);

        if (isHard)
            highlighter.AddCopyOnlyFunction(rcTrans, Directing_UIHighlight.CopyButtonFuctionProcess);
        highlighter.Execute();

        return highlighter;
    }
    
    public static Directing_UIHighlight ProcessFocusWithText(Component component, bool isHard, string desc, BoxEdge textDir, float textWidth, float textHieght, Action complete, float scale = 1f)
    {
       return ProcessFocus(component, isHard, desc, textDir, textWidth, textHieght, false, BoxEdge.Bottom, complete, scale);
    }
    public static Directing_UIHighlight ProcessFocusWithArrow(Component component, bool isHard, BoxEdge arrowDir, Action complete)
    {
       return ProcessFocus(component, isHard, "", BoxEdge.Bottom, 300, 100, true, arrowDir, complete);
    }

    public static Directing_UIHighlight ProcessFocus(Component component, bool isHard, Action complete)
    {
       return ProcessFocus(component, isHard, "", BoxEdge.Bottom, 300, 100, false, BoxEdge.Bottom, complete);
    }
    
    public static Directing_UIHighlight ProcessMultiFocus(bool isHard, Action complete, params Component[] components)
    {
        RectTransform[] rcTransArray = new RectTransform[components.Length];

        for (int i = 0; i < components.Length; i++)
        {
            rcTransArray[i] = components[i].GetComponent<RectTransform>();
        }

        var highlighter = new Directing_UIHighlight(rcTransArray, !isHard, complete);
        highlighter.SetScale(1.0f);
            
        highlighter.Execute();

            
        return highlighter;
    }


    public override void Execute()
    {
        if (_target != null)
        {
            _highlighter = ResourceLoader.Instantiate<UICommonHighlighter>(Res.PREFAB.UICommonHighlighter);

            var canvasScaler = _target.GetComponentInParent<CanvasScaler>();
            if (canvasScaler != null)
            {
                _highlighter.GetComponent<CanvasScaler>().screenMatchMode    = canvasScaler.screenMatchMode;
                _highlighter.GetComponent<CanvasScaler>().matchWidthOrHeight = canvasScaler.matchWidthOrHeight;
            }

            _highlighter.GetComponent<CanvasScaler>().SendMessage("Update");
            _highlighter.CloseByTouch = _autoClose;

            var canvas       = _highlighter.GetComponentInParent<Canvas>();
            var targetCanvas = _target.GetComponentInParent<Canvas>();

            canvas.transform.position = targetCanvas.transform.position;


            var cam       = _highlighter.GetComponentInParent<Canvas>().worldCamera;
            var targetCam = _target.GetComponentInParent<Canvas>().worldCamera;

            // cam.cullingMask = targetCam.cullingMask;
            //cam.nearClipPlane = targetCam.nearClipPlane;
            //cam.farClipPlane = targetCam.farClipPlane;
            cam.orthographicSize = targetCam.orthographicSize;

            _isReady             = false;
            _highlighter.onClose = Complete;

            AddMaskingRect(_target);
            AddHighlightBox(_target);

            new Directing_WaitForFrame(Process, 1).Execute();
        }
        else if (_targets != null)
        {
            _highlighter = ResourceLoader.Instantiate<UICommonHighlighter>(Res.PREFAB.UICommonHighlighter);
            
            for (int i = 0; i < _targets.Length; i++)
            {
                var canvasScaler = _targets[i].GetComponentInParent<CanvasScaler>();
                if (canvasScaler != null)
                {
                    _highlighter.GetComponent<CanvasScaler>().screenMatchMode    = canvasScaler.screenMatchMode;
                    _highlighter.GetComponent<CanvasScaler>().matchWidthOrHeight = canvasScaler.matchWidthOrHeight;
                }

                _highlighter.GetComponent<CanvasScaler>().SendMessage("Update");
                _highlighter.CloseByTouch = _autoClose;

                var canvas       = _highlighter.GetComponentInParent<Canvas>();
                var targetCanvas = _targets[i].GetComponentInParent<Canvas>();

                canvas.transform.position = targetCanvas.transform.position;
                
                var cam       = _highlighter.GetComponentInParent<Canvas>().worldCamera;
                var targetCam = _targets[i].GetComponentInParent<Canvas>().worldCamera;

                cam.orthographicSize = targetCam.orthographicSize;

                _isReady             = false;
                _highlighter.onClose = Complete;

                AddMaskingRect(_targets[i]);
                AddHighlightBox(_targets[i]);

            }
            
            new Directing_WaitForFrame(Process, 1).Execute();
        }
      
    }

    private void Process()
    {
        _isReady = true;

        foreach(var each in _reserveActions)
            each();

        _reserveActions.Clear();
    }

    public void SetScale(float scale)
    {
        if (!_isReady)
        {
            _reserveActions.Add(() => SetScale(scale));
            return;
        }


        _highlighter.SetScale(scale);
    }

    public void ActiveBackColor(bool isActive)
    {
        _highlighter.ActiveBackColor(isActive);
    }

    public void AddMaskingRect(RectTransform targetRcTrans)
    {
        if(!_isReady)
        {
            _reserveActions.Add(() => AddMaskingRect(targetRcTrans));
            return;
        }

        _highlighter.AddMaskingRect(targetRcTrans);
    }

    public void AddCopyObject(RectTransform target, Action<Directing_UIHighlight, RectTransform, RectTransform> onComplete)
    {
        if (!_isReady)
        {
            _reserveActions.Add(() => AddCopyObject(target, onComplete));
            return;
        }

        var copyObj = GameObject.Instantiate(target.gameObject).GetComponent<RectTransform>();
        _highlighter.AddObject(target, copyObj);

        if(onComplete != null)
            onComplete(this, target, copyObj);
    }

    public void AddCopyOnlyFunction(RectTransform target, Action<Directing_UIHighlight, RectTransform, RectTransform> onComplete)
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


    public void AddHighlightBox(RectTransform targetRcTrans)
    {
        if (!_isReady)
        {
            _reserveActions.Add(() => AddHighlightBox(targetRcTrans));
            return;
        }

        _highlighter.AddHighlightBox(targetRcTrans);
    }

    public void AddHighlightArrow(RectTransform targetRcTrans, BoxEdge edge)
    {
        if (!_isReady)
        {
            _reserveActions.Add(() => AddHighlightArrow(targetRcTrans, edge));
            return;
        }

        _highlighter.AddHighlightArrow(targetRcTrans, edge);
    }

    public void AddText(RectTransform targetRcTrans, Vector2 size, BoxEdge edge, string text, 
                        Action<RectTransform> onComplete = null, TextAlign align = TextAlign.MiddleCenter)
    {
        if(string.IsNullOrEmpty(text))
            return;

        if (!_isReady)
        {
            _reserveActions.Add(() => AddText(targetRcTrans, size, edge, text, onComplete, align));
            return;
        }

        var uiText = _highlighter.AddText(targetRcTrans, edge, text, align);

        if (onComplete != null)
            onComplete(uiText.transform.parent.rectTransform());

    }

    public void AddTextControlSize(RectTransform targetRcTrans, Vector2 size, BoxEdge edge, string text,
                    Action<RectTransform> onComplete = null, TextAlign align = TextAlign.MiddleCenter)
    {
        if (string.IsNullOrEmpty(text))
            return;

        if (!_isReady)
        {
            _reserveActions.Add(() => AddTextControlSize(targetRcTrans, size, edge, text, onComplete, align));
            return;
        }

        var uiText = _highlighter.AddText(targetRcTrans, size, edge, text, align);

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
