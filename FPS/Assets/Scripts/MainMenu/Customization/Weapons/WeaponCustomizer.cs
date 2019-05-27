using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCustomizer : MonoBehaviour
{
    public WeaponCustomizationLayout layout;
    public Transform showLocation;
    public int currentWeapon = 0;
    public int currentBarrel = 0;
    public int currentMagazine = 0;
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

        WeaponCustomizations customization = currentItem.GetComponent<WeaponCustomizations>();

        for (int i = 0; i < customization.barrels.Length; i++)
            if (i == currentBarrel)
                customization.barrels[i].SetActive(true);
            else
                customization.barrels[i].SetActive(false);

        for (int i = 0; i < customization.magazines.Length; i++)
            if (i == currentMagazine)
                customization.magazines[i].SetActive(true);
            else
                customization.magazines[i].SetActive(false);
    }

    public void ChangeWeapon(int i)
    {

    }

    public void ChangeBarrel(int i)
    {

    }

    public void ChangeMagazine(int i)
    {

    }
}
