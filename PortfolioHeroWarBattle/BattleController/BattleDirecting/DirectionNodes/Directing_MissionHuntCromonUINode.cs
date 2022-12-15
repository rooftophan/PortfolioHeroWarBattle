using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Directing_MissionHuntCromonUINode : BaseDirecting
{

    private string _path;
    private string _notice;
    private Vector3 _position;

    public Directing_MissionHuntCromonUINode(Action complete, string path, Vector3 position, string notice = null) : base(complete)
    {
        _path = path;
        _notice = notice;
        _position = position;
    }

    public override void Execute()
    {
        UIRootPreset.RootType Root = UIRootPreset.RootType.FX;
        string Layer = "FX";
        
        var name = UIRootPreset.GetName(Root);
        Debug.Log("Directing_MissionHuntCromonUINode prefab name : " + _path);
        var instance = ResourceLoader.Instantiate( _path);
        Debug.Log("LoadPrefab result : " + ((instance == null ) ? "null" : instance.name));
        if( instance == null )
        {
            Complete();
            return;
        }

        if(!string.IsNullOrEmpty(_notice)) {
            Transform textTrans = instance.transform.Find("Text");
            if(textTrans != null) {
                textTrans.GetComponent<Text>().text = _notice;
            }
        }

        new Controller.GetCanvas((x) =>
        {
            if (!string.IsNullOrEmpty(Layer))
            {
                Debug.Log("LayerUtil.ChangeRecusiveLayer : " +  ((instance == null ) ? "null" : instance.name));
                Debug.Log("LayerUtil.ChangeRecusiveLayer : " +  ((x == null ) ? "null" : x.gameObject.name));

                LayerUtil.ChangeRecusiveLayer(instance, Layer);
                LayerUtil.ChangeRecusiveLayer(x.gameObject, Layer);
            }

            instance.transform.SetParent(x.transform);
            instance.GetComponent<RectTransform>().anchoredPosition = _position;
            instance.transform.localScale = Vector3.one;
        }, name).Execute();

        Complete();
    }
}
