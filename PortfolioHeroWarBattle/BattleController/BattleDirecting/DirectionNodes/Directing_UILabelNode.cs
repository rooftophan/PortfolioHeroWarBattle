using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using NewBattleCore;

public class Directing_UILabelNode : BaseDirecting
{
    private string _path;
    private string _text;
    private float _delay;
    public Directing_UILabelNode(Action complete, string path, string text, float delay = 1f) : base(complete)
    {
        _path = path;
        _text = text;
        _delay = delay;
    }

    public override void Execute()
    {
        UIRootPreset.RootType Root = UIRootPreset.RootType.FX;

        var textModel = BattleManager.Instance.scene.System.Data.Text;

        var name = UIRootPreset.GetName(Root);

        string Key = "";

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

            if (!string.IsNullOrEmpty(path))
            {
                var child = view.Find(path);
                if (child == null)
                    throw new System.Exception(string.Format("SetUILabelNode 의 {0} 가 이상합니다.", _path));
                var label = child.GetComponent<Text>();

                if (string.IsNullOrEmpty(Key))
                    label.text = _text;
                else
                    label.text = textModel.GetText(Key);
            }
            else
            {
                var label = view.GetComponent<Text>();

                if (string.IsNullOrEmpty(Key))
                    label.text = _text;
                else
                    label.text = textModel.GetText(Key);
            }

        }, name).Execute();

        new Directing_Wait(Complete, _delay).Execute();
    }
}
