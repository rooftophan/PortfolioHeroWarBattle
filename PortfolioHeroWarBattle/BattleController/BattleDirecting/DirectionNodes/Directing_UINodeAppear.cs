using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Directing_UINodeAppear : BaseDirecting
{
    private UIRootPreset.RootType _rootType;
    private string _path;
    private bool _isShow;
    public Directing_UINodeAppear(Action complete, UIRootPreset.RootType rootType, string path, bool isShow) : base(complete)
    {
        _rootType = rootType;
        _path = path;
        _isShow = isShow;

    }

    public override void Execute()
    {
        string reactorName = "UINodeAppear";
        
        var name = UIRootPreset.GetName(_rootType);

        new Controller.GetCanvas((x) =>
        {
            var index = _path.IndexOf('/');

            string type = _path;
            string path = "";

            if (index > -1)
            {
                type = _path.Substring(0, index);
                index += 1;
                path = _path.Substring(index, _path.Length - index);
            }

            var view = x.transform.Find(type);
            if (view == null)
                view = x.transform.Find(type + "(Clone)");
            if (view == null)
                view = x.transform;

            if (!string.IsNullOrEmpty(path))
            {
                var child = view.Find(path);
                if (child == null)
                {
                    Debug.Log(string.Format("!!!!! SetUINodeAppear  [UINode] {0} 의 {1} 가 이상합니다.", reactorName, _path));
                }
                else
                {
                    child.gameObject.SetActive(_isShow);
                }

            }

        }, name).Execute();

        Complete();
    }
}
