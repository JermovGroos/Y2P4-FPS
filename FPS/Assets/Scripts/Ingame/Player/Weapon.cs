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

    public string playerTag;
    public string managerTag;

    public ParticleSystem p;

    bool allowFire = true;

    public void Update()
    {
        if (Input.GetButtonDown("Fire1") && fireType == FireTypes.SingleFire && allowFire || Input.GetButtonDown("Fire1") && fireType == FireTypes.Burst && allowFire)
            StartCoroutine(Fire(fireRate));
        else if (Input.GetButton("Fire1") && fireType == FireTypes.Auto && allowFire)
            StartCoroutine(Fire(fireRate));
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
                    hit.transform.GetComponent<Player>().DamagePlayer(PhotonNetwork.playerName, damage);
                GameObject.FindWithTag(managerTag).GetComponent<ImpactManager>().SendImpactInfo(hit.collider.material, hit);
            }
            if (fireType == FireTypes.Burst)
                yield return new WaitForSeconds(burstDelay);
        }
        yield return new WaitForSeconds(time);
        allowFire = true;
    }
}
