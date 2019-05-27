using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomizationSwitch : MonoBehaviour
{
    public void Open()
    {
        Camera.main.transform.position += Vector3.right * 5;
    }

    public void Close()
    {
        Camera.main.transform.position -= Vector3.right * 5;
    }
}
