using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponCustomizer : MonoBehaviour
{
    public WeaponCustomizationLayout layout;
    public Transform showLocation;
    public WeaponClassData weapon1;
    public WeaponClassData weapon2;

    public Transform customizationLayoutBarrel;
    public Transform customizationLayourMagazine;
    public GameObject cusotmizationPanel;

    public Text weapon1Name;
    public Image weapon1Image;
    public Text weapon2Name;
    public Image weapon2Image;

    [HideInInspector]public bool displayWeapon;
    [HideInInspector]public int currentlyEditing;
    GameObject currentWeapon;
    //WeaponSelect
    public void StartWeaponSelect(int weaponSlot)
    {
        currentlyEditing = weaponSlot;
    }

    public void SimpleHoverWeaponDisplay(int weaponIndex)
    {
        if (currentWeapon)
            Destroy(currentWeapon);
        if (displayWeapon)
            currentWeapon = Instantiate(layout.weapons[weaponIndex].baseWeapon, showLocation.position, showLocation.rotation);
    }

    public void DisplayWeaponVoid(bool b)
    {
        displayWeapon = b;
        DisplayCurrentThings();
    }

    public void ChangeWeapon(int index)
    {
        if(currentlyEditing == 1)
        {
            weapon1.currentWeapon = index;
            weapon1.currentBarrel = 0;
            weapon1.currentMagazine = 0;
        }
        else
        {
            weapon2.currentWeapon = index;
            weapon2.currentBarrel = 0;
            weapon2.currentMagazine = 0;
        }
        displayWeapon = false;
        DisplayCurrentThings();
    }
    //CustomizationStart
    public void StartCustomization(int index)
    {
        currentlyEditing = index;
        DisplayCurrentThings();
        displayWeapon = true;
    }

    public void DisplayCustomizations()
    {
        foreach (Transform child in customizationLayoutBarrel)
            Destroy(child);
        foreach (Transform child in customizationLayourMagazine)
            Destroy(child);

        for (int i = 0; i < layout.weapons[(currentlyEditing == 1)? weapon1.currentWeapon : weapon2.currentWeapon].barrels.Length; i++)
        {
            GameObject g = Instantiate(cusotmizationPanel, customizationLayoutBarrel);
        }

        for (int i = 0; i < layout.weapons[(currentlyEditing == 1) ? weapon1.currentWeapon : weapon2.currentWeapon].magazines.Length; i++)
        {

        }

    }

    public void ChangeBarrel(int index)
    {
        if (currentlyEditing == 1)
            weapon1.currentBarrel = index;
        else
            weapon2.currentBarrel = index;
    }
    public void ChangeMagazine(int index)
    {
        if (currentlyEditing == 1)
            weapon1.currentBarrel = index;
        else
            weapon2.currentBarrel = index;
    }

    //Start
    public void Start()
    {
        DisplayCurrentThings();
    }

    //Display
    public void DisplayCurrentThings()
    {
        weapon1Name.text = layout.weapons[weapon1.currentWeapon].weaponName;
        weapon2Name.text = layout.weapons[weapon2.currentWeapon].weaponName;
        weapon1Image.sprite = layout.weapons[weapon1.currentWeapon].weaponSprite;
        weapon2Image.sprite = layout.weapons[weapon2.currentWeapon].weaponSprite;
        if (currentWeapon)
            Destroy(currentWeapon);
        if (displayWeapon)
        {
            WeaponClassData displayData = (currentlyEditing == 1) ? weapon1 : weapon2;
            currentWeapon = Instantiate(layout.weapons[displayData.currentWeapon].baseWeapon,showLocation.position,showLocation.rotation);
            WeaponCustomizations customization = currentWeapon.GetComponent<WeaponCustomizations>();
            for (int i = 0; i < customization.magazines.Length; i++)
                if (i == displayData.currentMagazine)
                    customization.magazines[i].SetActive(true);
                else
                    customization.magazines[i].SetActive(false);
            for (int i = 0; i < customization.barrels.Length; i++)
                if (i == displayData.currentBarrel)
                    customization.barrels[i].SetActive(true);
                else
                    customization.barrels[i].SetActive(false);
        }
    }

    [System.Serializable]
    public class WeaponClassData
    {
        public int currentWeapon;
        public int  currentMagazine;
        public int currentBarrel;
    }
}
