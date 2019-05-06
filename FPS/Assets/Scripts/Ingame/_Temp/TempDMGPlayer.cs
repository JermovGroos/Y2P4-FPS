using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempDMGPlayer : MonoBehaviour
{
    public GameObject playerToDamage;
    public string myName;
    public bool damagePlayer;
    public float damage;

    // Update is called once per frame
    void Update()
    {
        if(damagePlayer) {
            playerToDamage.GetComponent<Player>().DamagePlayer(myName, damage);
            damagePlayer = false;
        }
    }
}
