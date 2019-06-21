using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToggleInGameOptions : MonoBehaviour
{
    public GameObject options;

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            options.SetActive(!options.activeInHierarchy);
            Cursor.visible = options.activeInHierarchy;
            Cursor.lockState = options.activeInHierarchy ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    public void Return()
    {
        options.SetActive(false);
    }
}
