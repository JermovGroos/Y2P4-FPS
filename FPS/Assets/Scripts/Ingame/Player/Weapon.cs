using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum FireTypes { SingleFire,Burst,Auto}
    public FireTypes fireType;
    public float damage;
    public float fireRate;
    public float spread;
    public int clipSize;
    [Range(1,20)]
    public int bulletAmount;
    public float burstDelay;
    public float recoil;
    public float recoilSpeed;
    public float recoilReturnSpeed;

    public string playerTag;
    public string managerTag;

    private int currentAmmo;

    public ParticleSystem p;

    bool allowFire = true;

    public void FixedUpdate()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, transform.parent.rotation, recoilReturnSpeed * Time.deltaTime);
        if (Input.GetButtonDown("Fire1") && fireType == FireTypes.SingleFire && allowFire || Input.GetButtonDown("Fire1") && fireType == FireTypes.Burst && allowFire)
            StartCoroutine(Fire(fireRate));
        else if (Input.GetButton("Fire1") && fireType == FireTypes.Auto && allowFire)
            StartCoroutine(Fire(fireRate));
    }

    public IEnumerator Recoil()
    {
        Vector3 tempRecoil = new Vector3();
        tempRecoil += Vector3.right * -recoil;
        tempRecoil += Vector3.up * Random.Range(-recoil, recoil);

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
        p.Play();
        for (int i = 0; i < bulletAmount; i++)
        {
            Vector3 addDirection = new Vector3(Random.Range(-spread, spread), Random.Range(-spread, spread), Random.Range(-spread, spread));
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(transform.position, transform.forward + addDirection ,out hit, Mathf.Infinity))
            {
                if (hit.transform.tag == playerTag)
                    hit.transform.GetComponent<PhotonView>().RPC("DamagePlayer", PhotonTargets.All, PhotonNetwork.playerName, damage);
                GameObject.FindWithTag(managerTag).GetComponent<ImpactManager>().SendImpactInfo(hit.collider.material, hit);
            }

            if (fireType != FireTypes.Burst && i == 0)
                StartCoroutine(Recoil());
            else if (fireType != FireTypes.Burst)
                StartCoroutine(Recoil());

            if (fireType == FireTypes.Burst)
                yield return new WaitForSeconds(burstDelay);
        }
        yield return new WaitForSeconds(time);
        allowFire = true;
    }
}
