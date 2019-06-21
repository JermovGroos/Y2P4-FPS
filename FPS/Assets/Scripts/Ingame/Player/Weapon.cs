using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Photon.MonoBehaviour
{
    public WeaponCustomizationLayout layout;

    public WeaponIngameSlot weapon1, weapon2;
    public int currentlySelected;

    public string playerTag;
    public string managerTag;

    public float recoilSpeed;
    public float recoilReturnSpeed;

    public WeaponDisplay weaponDisplay;

    bool allowFire = true;

    public Saving saveData;

    public bool zoomed;
    public float standardFov;
    public float zoomedFov;
    public string reload;
    public string weaponSwitch;

    public void Start()
    {
        saveData = GameObject.FindWithTag("Saving").GetComponent<Saving>();
        CalculateStats(0);
        CalculateStats(1);
        weapon1.currentAmmo = weapon1.stats.clipSize;
        weapon2.currentAmmo = weapon2.stats.clipSize;
        currentlySelected = 1;
    }

    public void CalculateStats(int index)
    {
        WeaponCustomizer.WeaponClassData data = (index == 0) ? saveData.data.lastLoadout.weapon1 : saveData.data.lastLoadout.weapon2;
        WeaponStats stats = layout.weapons[data.currentWeapon].stats;
        AttachmentStatsChange(layout.weapons[data.currentWeapon].barrels[data.currentBarrel].addStats, index);
        AttachmentStatsChange(layout.weapons[data.currentWeapon].magazines[data.currentMagazine].addStats, index);
        if (index == 0)
            weapon1.soundIndex = layout.weapons[data.currentWeapon].barrels[data.currentBarrel].soundIndex;
        else
            weapon2.soundIndex = layout.weapons[data.currentWeapon].barrels[data.currentBarrel].soundIndex;

        if (index == 0)
            weapon1.stats = stats;
        else
            weapon2.stats = stats;
    }

    public void FixedUpdate()
    {
        WeaponStats stats = (currentlySelected == 1) ? weapon1.stats : weapon2.stats;
        WeaponIngameSlot slot = (currentlySelected == 1) ? weapon1 : weapon2;

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

        if (Input.GetButtonDown(reload) && allowFire && slot.currentAmmo != stats.clipSize)
            StartCoroutine(Reload());
        else if (Input.GetButtonDown(reload))
            Debug.Log(allowFire + "   /   " + slot.currentAmmo + "   " + stats.clipSize);
        if (Input.GetAxis(weaponSwitch) != 0 && allowFire)
        {
            if (currentlySelected == 1)
                currentlySelected = 2;
            else
                currentlySelected = 1;
            StartCoroutine(WeaponSwitch(currentlySelected));
        }
    }

    public IEnumerator WeaponSwitch(int switchTo)
    {
        allowFire = false;
        currentlySelected = switchTo;
        weaponDisplay.Display(switchTo - 1);
        float speed = (currentlySelected == 1) ? weapon1.stats.switchSpeed : weapon2.stats.switchSpeed;
        ManagerBasicStuff basic = GameObject.FindWithTag(managerTag).GetComponent<ManagerBasicStuff>();
        WeaponCustomizer.WeaponClassData data = (currentlySelected == 1) ? saveData.data.lastLoadout.weapon1 : saveData.data.lastLoadout.weapon2;
        basic.currentAmmo.text = (switchTo == 1) ? weapon1.currentAmmo.ToString() : weapon2.currentAmmo.ToString();
        basic.maxAmmo.text = (switchTo == 1) ? weapon1.stats.clipSize.ToString() : weapon2.stats.clipSize.ToString();
        basic.weaponSprite.sprite = layout.weapons[data.currentWeapon].weaponSprite;
        yield return new WaitForSeconds(speed);
        allowFire = true;
    }

    public IEnumerator Reload()
    {
        WeaponStats stats = (currentlySelected == 1) ? weapon1.stats : weapon2.stats;
        WeaponIngameSlot slot = (currentlySelected == 1) ? weapon1 : weapon2;
        allowFire = false;
        yield return new WaitForSeconds(stats.reloadTime);
        slot.currentAmmo = stats.clipSize;
        ManagerBasicStuff basic = GameObject.FindWithTag(managerTag).GetComponent<ManagerBasicStuff>();
        basic.currentAmmo.text = (currentlySelected == 1) ? weapon1.currentAmmo.ToString() : weapon2.currentAmmo.ToString();
        allowFire = true;
    }

    public IEnumerator Recoil()
    {
        WeaponStats stats = (currentlySelected == 1) ? weapon1.stats : weapon2.stats;
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
        WeaponStats stats = (currentlySelected == 1) ? weapon1.stats : weapon2.stats;
        WeaponIngameSlot slot = (currentlySelected == 1) ? weapon1 : weapon2;
        allowFire = false;
        for (int i = 0; i < stats.bulletAmount; i++)
        {
            if(slot.currentAmmo > 0)
            {
                weaponDisplay.photonView.RPC("ShowMuzzleFlash", PhotonTargets.All);
                weaponDisplay.CallSound((currentlySelected == 1) ? weapon1.soundIndex : weapon2.soundIndex);
                Vector3 addDirection = new Vector3(Random.Range(-stats.spread, stats.spread), Random.Range(-stats.spread, stats.spread), Random.Range(-stats.spread, stats.spread));
                if (zoomed)
                    addDirection /= 2;
                RaycastHit hit = new RaycastHit();
                if (Physics.Raycast(transform.position, transform.forward + addDirection, out hit, Mathf.Infinity))
                {
                    if (hit.transform.tag == playerTag)
                        hit.transform.GetComponent<PhotonView>().RPC("DamagePlayer", PhotonTargets.All, PhotonNetwork.playerName, stats.damage);
                    GameObject.FindWithTag(managerTag).GetComponent<ImpactManager>().SendImpactInfo(hit.collider.material, hit);
                }
                if (stats.fireType != WeaponStats.FireTypes.Burst && i == 0)
                    StartCoroutine(Recoil());
                else if (stats.fireType != WeaponStats.FireTypes.Burst)
                    StartCoroutine(Recoil());
                slot.currentAmmo--;

                if (stats.fireType == WeaponStats.FireTypes.Burst)
                    yield return new WaitForSeconds(stats.burstDelay);

                ManagerBasicStuff basic = GameObject.FindWithTag(managerTag).GetComponent<ManagerBasicStuff>();
                basic.currentAmmo.text = (currentlySelected == 1) ? weapon1.currentAmmo.ToString() : weapon2.currentAmmo.ToString();
            }
        }
        yield return new WaitForSeconds(time);
        allowFire = true;
    }

    public void AttachmentStatsChange(WeaponStats _stats, int index)
    {
        WeaponStats tempStats = new WeaponStats((index == 0)? weapon1.stats : weapon2.stats);
        tempStats.damage += _stats.damage;
        tempStats.fireRate += _stats.fireRate;
        tempStats.spread += _stats.spread;
        tempStats.clipSize += _stats.clipSize;
        tempStats.burstDelay += _stats.bulletAmount;
        tempStats.recoil += _stats.recoil;
        tempStats.reloadTime += _stats.reloadTime;
        tempStats.switchSpeed += _stats.switchSpeed;
        if (index == 0)
            weapon1.stats = tempStats;
        else
            weapon2.stats = tempStats;
    }

    [System.Serializable]
    public class WeaponIngameSlot
    {
        public WeaponStats stats;
        public int currentAmmo;
        public int soundIndex;
    }
}
