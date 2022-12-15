using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Directing_RemoveUIRootNode : BaseDirecting
{
    public Directing_RemoveUIRootNode(Action complete = null) : base(complete)
    {
    }

    public override void Execute()
    {
        UIRootPreset.RootType Root = UIRootPreset.RootType.FX;

        var name = UIRootPreset.GetName(Root);
        var root = GameObject.Find(name);
        GameObject.Destroy(root);

        Complete();
    }
}
