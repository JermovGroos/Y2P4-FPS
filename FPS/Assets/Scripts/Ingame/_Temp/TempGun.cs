using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempGun : MonoBehaviour
{
    public float range;
    public float damage;
    public string playerTag;
    public TempMovement movement;

    public void Update()
    {
        if (movement.isMine)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                RaycastHit hit = new RaycastHit();
                if (Physics.Raycast(transform.position, transform.forward, out hit, range))
                    if (hit.transform.gameObject.tag == playerTag)
                        hit.transform.GetComponent<PhotonView>().RPC("Damage", PhotonTargets.All, damage, PhotonNetwork.playerName);
            }
        }
    }
}
