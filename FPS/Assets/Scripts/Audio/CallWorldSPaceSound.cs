using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallWorldSPaceSound : MonoBehaviour
{
    public AudioClip[] audioClips;
    public GameObject audioSourceObject;

    [PunRPC,HideInInspector]
    public void WorldSpaceAudioClip(int index, Vector3 pos)
    {
        GameObject g = Instantiate(audioSourceObject, pos, Quaternion.identity);
        g.GetComponent<AudioSource>().PlayOneShot(audioClips[index]);
        Destroy(g, audioClips[index].length);
    }
}
