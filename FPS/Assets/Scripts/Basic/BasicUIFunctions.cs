using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicUIFunctions : MonoBehaviour
{
    [Header("FromToButton")]
    public GameObject[] from;
    public GameObject[] to;

    public void FromToButton()
    {
        foreach (GameObject obj in from)
            obj.SetActive(false);
        foreach (GameObject obj in to)
            obj.SetActive(true);
    }
}
