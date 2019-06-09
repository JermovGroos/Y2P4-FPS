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
        public WeaponStats addStats;
        public Sprite sprite;
    }
}

[System.Serializable]
public class WeaponStats
{
    public enum FireTypes { SingleFire, Burst, Auto }
    public FireTypes fireType;
    public float damage;
    public float fireRate;
    public float spread;
    public int clipSize;
    [Range(1, 20)]
    public int bulletAmount;
    public float burstDelay;
    public float recoil;

    public WeaponStats()
    {
        fireType = FireTypes.SingleFire;
        damage = 0;
        fireRate = 0;
        spread = 0;
        clipSize = 0;
        bulletAmount = 1;
        burstDelay = 0;
        recoil = 0;
    }

    public WeaponStats(WeaponStats copyStats)
    {
        fireType = copyStats.fireType;
        damage = copyStats.damage;
        fireRate = copyStats.fireRate;
        spread = copyStats.spread;
        clipSize = copyStats.clipSize;
        bulletAmount = copyStats.bulletAmount;
        burstDelay = copyStats.burstDelay;
        recoil = copyStats.recoil;
    }
}
