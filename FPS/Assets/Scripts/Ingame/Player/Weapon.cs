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

    public string playerTag;
    public string managerTag;

    public void Update()
    {
        if (Input.GetButtonDown("Fire1"))
            Fire();
    }

    public void Fire()
    {
        for (int i = 0; i < bulletAmount; i++)
        {
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(transform.position, transform.forward ,out hit, Mathf.Infinity))
                GameObject.FindWithTag(managerTag).GetComponent<ImpactManager>().SendImpactInfo(hit.collider.material, hit);
        }
    }
}
