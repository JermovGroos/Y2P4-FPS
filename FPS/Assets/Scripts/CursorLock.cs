using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorLock : MonoBehaviour
{

    bool locked = false;
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            if(locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                locked = !locked;
            } else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                locked = !locked;
            }
        }
    }
}
