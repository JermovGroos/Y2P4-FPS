using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Photon.MonoBehaviour
{
    public WeaponCustomizationLayout layout;
    public WeaponStats stats;

    public string playerTag;
    public string managerTag;

    private int currentAmmo;

    public float recoilSpeed;
    public float recoilReturnSpeed;

    public WeaponDisplay weaponDisplay;

    bool allowFire = true;

    public Saving saveData;

    public bool zoomed;
    public float standardFov;
    public float zoomedFov;

    public void Start()
    {
        saveData = GameObject.FindWithTag("Saving").GetComponent<Saving>();
        CalculateStats(0);
    }

    public void CalculateStats(int index)
    {
        WeaponCustomizer.WeaponClassData data = (index == 0) ? saveData.data.lastLoadout.weapon1 : saveData.data.lastLoadout.weapon2;
        stats = layout.weapons[data.currentWeapon].stats;
        AttachmentStatsChange(layout.weapons[data.currentWeapon].barrels[data.currentBarrel].addStats);
        AttachmentStatsChange(layout.weapons[data.currentWeapon].magazines[data.currentMagazine].addStats);
    }

    public void FixedUpdate()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, transform.parent.rotation, recoilReturnSpeed * Time.deltaTime);
        if (Input.GetButtonDown("Fire1") && stats.fireType == WeaponStats.FireTypes.SingleFire && allowFire || Input.GetButtonDown("Fire1") && stats.fireType == WeaponStats.FireTypes.Burst && allowFire)
            StartCoroutine(Fire(stats.fireRate));
        else if (Input.GetButton("Fire1") && stats.fireType == WeaponStats.FireTypes.Auto && allowFire)
            StartCoroutine(Fire(stats.fireRate));

        if (Input.GetButtonDown("Fire2"))
        {
            zoomed = !zoomed;
            GetComponent<Camera>().fieldOfView = zoomed? zoomedFov : standardFov;
        }
    }

    public IEnumerator Recoil()
    {
        Vector3 tempRecoil = new Vector3();
        tempRecoil += Vector3.right * -stats.recoil;
        tempRecoil += Vector3.up * Random.Range(-stats.recoil, stats.recoil);

        while(Vector3.Distance(tempRecoil,new Vector3()) > 0.1f)
        {
            transform.Rotate(tempRecoil * recoilSpeed);
            tempRecoil -= (tempRecoil * recoilSpeed);
            yield return null;
        }
    }

    public IEnumerator Fire(float time)
    {
        allowFire = false;
        weaponDisplay.photonView.RPC("ShowMuzzleFlash", PhotonTargets.All);
        for (int i = 0; i < stats.bulletAmount; i++)
        {
            Vector3 addDirection = new Vector3(Random.Range(-stats.spread, stats.spread), Random.Range(-stats.spread, stats.spread), Random.Range(-stats.spread, stats.spread));
            if (zoomed)
                addDirection /= 2;
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(transform.position, transform.forward + addDirection ,out hit, Mathf.Infinity))
            {
                if (hit.transform.tag == playerTag)
                    hit.transform.GetComponent<PhotonView>().RPC("DamagePlayer", PhotonTargets.All, PhotonNetwork.playerName, stats.damage);
                GameObject.FindWithTag(managerTag).GetComponent<ImpactManager>().SendImpactInfo(hit.collider.material, hit);
            }

            if (stats.fireType != WeaponStats.FireTypes.Burst && i == 0)
                StartCoroutine(Recoil());
            else if (stats.fireType != WeaponStats.FireTypes.Burst)
                StartCoroutine(Recoil());

            if (stats.fireType == WeaponStats.FireTypes.Burst)
                yield return new WaitForSeconds(stats.burstDelay);
        }
        yield return new WaitForSeconds(time);
        allowFire = true;
    }

    public void AttachmentStatsChange(WeaponStats _stats)
    {
        WeaponStats tempStats = new WeaponStats(stats);
        tempStats.damage += _stats.damage;
        tempStats.fireRate += _stats.fireRate;
        tempStats.spread += _stats.spread;
        tempStats.clipSize += _stats.clipSize;
        tempStats.burstDelay += _stats.bulletAmount;
        tempStats.recoil += _stats.recoil;
        stats = tempStats;
    }
}
