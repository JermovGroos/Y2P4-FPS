using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactManager : Photon.MonoBehaviour
{
    public SoundImpact[] impactTypes;
    public int baseParticle;
    public float particleLifetime;

    public void SendImpactInfo(PhysicMaterial type, RaycastHit hit)
    {
        bool found = false;
        for (int i = 0; i < impactTypes.Length; i++)
            if (impactTypes[i].materialType.name + " (Instance)" == type.name)
            {
                found = true;
                photonView.RPC("SpawnParticle", PhotonTargets.All, i, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
                break;
            }
        if (!found)
            photonView.RPC("SpawnParticle", PhotonTargets.All, baseParticle, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
    }

    [PunRPC]
    public void SpawnParticle(int index, Vector3 pos, Quaternion rot)
    {
        Destroy(Instantiate(impactTypes[index].particle, pos, rot),particleLifetime);
    }

    [System.Serializable]
    public class SoundImpact
    {
        public PhysicMaterial materialType;
        public GameObject particle;
    }
}
