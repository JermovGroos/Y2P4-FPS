using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponCustomization",menuName = "WeaponCustomization")]
public class WeaponCustomizationLayout : ScriptableObject
{
    public Weapon[] weapons;

    [System.Serializable]
    public class Weapon
    {
        public string weaponName;
        public Sprite weaponSprite;
        public GameObject baseWeapon;
        public WeaponScriptableSlot[] barrels, magazines;
        public WeaponStats stats;
    }

    [System.Serializable]
    public class WeaponScriptableSlot
    {
        public string objName;
        public GameObject obj;
    }
}

[System.Serializable]
public class WeaponStats
{
    [Header("BaseStats")]
    public float damage;
    public float spread;
    public float fireSpeed;
    public float weight;
    public float recoilAmount;
    public FireType firetype;
    public enum FireType { SingleFire,Burst,Auto}
    [Header("Reload")]
    public bool singleReload;
    public float maxAmmo;
    public float reloadSpeed;
    [Header("MultipleShotStats")]
    public bool multipleBullets;
    public int bulletAmount;
}
