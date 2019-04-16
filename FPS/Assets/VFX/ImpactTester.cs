using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactTester : MonoBehaviour
{
    public GameObject prefab;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Fire();
        }    
    }

    public void Fire()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f))
        {
            GameObject decal = Instantiate(prefab, hit.point, Quaternion.FromToRotation(Vector3.up,hit.normal));
            Destroy(decal, 10f);
        }
    }
}
