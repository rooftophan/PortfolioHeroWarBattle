using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFormationAddableIcon
{
    void OnAddFormationIcon(int posIndex, UIFormationIcon formationIcon, bool isAlly, bool isHide);
    void OnChangePosition(int posIndex, Vector3 pos, bool isAlly);
}
