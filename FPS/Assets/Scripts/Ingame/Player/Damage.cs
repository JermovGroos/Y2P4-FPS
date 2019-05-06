using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Damage
{
    public string damager;
    public float damageAmount;

    public Damage(string _damager, float _damageAmount) {
        damager = _damager;
        damageAmount = _damageAmount;
    }
}
