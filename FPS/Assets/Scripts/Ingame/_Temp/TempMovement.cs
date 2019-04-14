using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempMovement : Photon.MonoBehaviour
{
    public GameObject cam;
    public float speed;
    Vector3 position;
    Quaternion rotation;
    public bool isMine;
    public float health = 100;
    public List<DamageInfo> damageInfo = new List<DamageInfo>();
    public string gameManagerTag;

    [PunRPC]
    public void Damage(float damage, string damager)
    {
        if (photonView.isMine)
        {
            health -= damage;
            bool doesntContain = true;
            foreach (DamageInfo damageInf in damageInfo)
                if (damageInf.damagerName == damager)
                {
                    doesntContain = false;
                    damageInf.damage += damage;
                    break;
                }
            if (doesntContain)
                damageInfo.Add(new DamageInfo(damager, damage));

            if (health <= 0)
            {
                List<float> damages = new List<float>();
                List<string> damagers = new List<string>();
                foreach (DamageInfo info in damageInfo)
                {
                    damages.Add(info.damage);
                    damagers.Add(info.damagerName);
                }
                GameObject.FindWithTag(gameManagerTag).GetComponent<PhotonView>().RPC("PlayerKilled", PhotonTargets.MasterClient, PhotonNetwork.playerName, damager, damages.ToArray(), damagers.ToArray());
                GameInfoManager manager = GameObject.FindWithTag(gameManagerTag).GetComponent<GameInfoManager>();
                if (manager.allowRespawn)
                    manager.Respawn();
                else
                    PhotonNetwork.Destroy(gameObject);
            }
        }
    }

    public void Start()
    {
        if (photonView.isMine)
        {
            cam.SetActive(true);
            isMine = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
            StartCoroutine(LerpPosition());
    }

    public IEnumerator LerpPosition()
    {
        while (true)
        {
            transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * 9);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * 9);
            yield return null;
        }
    }

    public void FixedUpdate()
    {
        if (isMine)
        {
            transform.Translate(new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * speed * Time.deltaTime);
            transform.Rotate(Vector3.up * Input.GetAxis("Mouse X"));
            cam.transform.Rotate(-Vector3.right * Input.GetAxis("Mouse Y"));
        }
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            position = (Vector3)stream.ReceiveNext();
            rotation = (Quaternion)stream.ReceiveNext();
        }
    }

    [System.Serializable]
    public class DamageInfo
    {
        public string damagerName;
        public float damage;

        public DamageInfo(string _damager, float _damage)
        {
            damagerName = _damager;
            damage = _damage;
        }
    }
}
