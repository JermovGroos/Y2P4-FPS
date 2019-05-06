using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Damage
{
    public string damager;
    public float amount;

    public Damage(string _damager, float _amount) {
        damager = _damager;
        amount = _amount;
    }
}
