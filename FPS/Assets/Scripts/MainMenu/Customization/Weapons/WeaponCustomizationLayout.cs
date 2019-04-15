using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponCustomization",menuName = "WeaponCustomization")]
public class WeaponCustomizationLayout : ScriptableObject
{
    public WeaponScriptableSlot[] barrels, magazines;

    [System.Serializable]
    public class WeaponScriptableSlot
    {
        public string objName;
        public GameObject obj;
    }
}
