using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCustomizer : MonoBehaviour
{
    public WeaponCustomizationLayout layout;
    public Transform showLocation;
    public int currentWeapon = 0;
    public GameObject currentItem;

    public void Start()
    {
        DisplayCurrentThings();
    }

    public void DisplayCurrentThings()
    {
        if (currentItem)
            Destroy(currentItem);
        currentItem = Instantiate(layout.weapons[currentWeapon].baseWeapon, showLocation.position, showLocation.rotation);
    }
}
