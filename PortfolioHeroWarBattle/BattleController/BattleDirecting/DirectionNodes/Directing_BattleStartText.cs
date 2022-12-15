using System;
using System.Collections;
using NewBattleCore;
using UnityEngine.UI;
using Controller;

public class Directing_BattleStartText : BaseDirecting
{
    private string _text;
    public Directing_BattleStartText(Action complete = null) : base(complete)
    {
    }

    public override void Execute()
    {
        UIRootPreset.RootType Root = UIRootPreset.RootType.FX;

        var textModel = BattleManager.Instance.scene.System.Data.Text;

        var name = UIRootPreset.GetName(Root);

        string Path = "UIBattleStartUGUI/Text";
        string Key = "";
        string curText = textModel.GetText(TextKey.MI_UI_BattleStart);

        new GetCanvas((x) =>
        {
            var index = Path.IndexOf('/');

            string type = Path;
            string path = "";

            if (index > -1)
            {
                type = Path.Substring(0, index);
                index += 1;
                path = Path.Substring(index, Path.Length - index);
            }

            var view = x.transform.Find(type);
            if (view == null)
                view = x.transform.Find(type + "(Clone)");

            if (!string.IsNullOrEmpty(path))
            {
                var child = view.Find(path);
                if (child == null)
                    throw new System.Exception(string.Format("SetBattleStartText 의 {0} 가 이상합니다.", Path));
                var label = child.GetComponent<Text>();

                if (string.IsNullOrEmpty(Key))
                    label.text = curText;
                else
                    label.text = textModel.GetText(Key);
            }
            else
            {
                var label = view.GetComponent<Text>();

                if (string.IsNullOrEmpty(Key))
                    label.text = curText;
                else
                    label.text = textModel.GetText(Key);
            }

        }, name).Execute();

        Complete();

    }

}
