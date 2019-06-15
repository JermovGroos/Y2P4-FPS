using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponDisplay : Photon.MonoBehaviour
{
    public WeaponCustomizationLayout layout;
    public WeaponCustomizations weapon;
    public Transform weaponLocation;
    public Weapon weapons;

    public void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        if (photonView.isMine)
            Display(weapons.currentlySelected - 1);
    }

    public void Start()
    {
        if(photonView.isMine)
            Display(0);
    }

    public void Display(int weapon)
    {
        Saving save = GameObject.FindWithTag("Saving").GetComponent<Saving>();
        if(weapon == 0)
            photonView.RPC("DisplayWeapon", PhotonTargets.All, save.data.lastLoadout.weapon1.currentWeapon, save.data.lastLoadout.weapon1.currentBarrel, save.data.lastLoadout.weapon1.currentMagazine);
        else
            photonView.RPC("DisplayWeapon", PhotonTargets.All, save.data.lastLoadout.weapon2.currentWeapon, save.data.lastLoadout.weapon2.currentBarrel, save.data.lastLoadout.weapon2.currentMagazine);
    }

    [PunRPC]
    public void ShowMuzzleFlash()
    {
        if(weapon)
            weapon.muzzleFlash.GetComponent<ParticleSystem>().Play();
    }

    [PunRPC]
    public void DisplayWeapon(int _weapon, int _barrel, int _magazine)
    {
        foreach (Transform chid in weaponLocation)
            Destroy(chid.gameObject);
        weapon = Instantiate(layout.weapons[_weapon].baseWeapon, weaponLocation).GetComponent<WeaponCustomizations>();
        for (int i = 0; i < weapon.barrels.Length; i++)
            if (i == _barrel)
                weapon.barrels[i].SetActive(true);
            else
                weapon.barrels[i].SetActive(false);

        for (int i = 0; i < weapon.magazines.Length; i++)
            if (i == _magazine)
                weapon.magazines[i].SetActive(true);
            else
                weapon.magazines[i].SetActive(false);
    }
}
