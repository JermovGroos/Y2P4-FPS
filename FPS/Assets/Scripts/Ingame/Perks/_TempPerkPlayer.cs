using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _TempPerkPlayer : MonoBehaviour
{
    public bool apply;
    public Perk toApply;
    public PerkManager perkManager;
    public GameObject player;

    // Update is called once per frame
    void Update()
    {
        if(apply)
        {
            apply = false;
            perkManager.ApplyPerk(player, toApply);
        }
    }
}
