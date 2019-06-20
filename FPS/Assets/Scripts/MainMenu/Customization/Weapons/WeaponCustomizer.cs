using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponCustomizer : MonoBehaviour
{
    public WeaponCustomizationLayout layout;
    public Transform showLocation;
    public WeaponLoadoutSlot weapons;

    public Transform customizationLayoutBarrel;
    public Transform customizationLayourMagazine;
    public GameObject cusotmizationPanel;
    [HideInInspector]public int currentlyEditing;
    GameObject currentWeapon;
    public string savingTag;
    public Saving saving;

    public void Awake()
    {
        LoadData();
    }

    public void OverWriteWeaponData()
    {
        weapons.weapon1.currentWeapon = 0;
        weapons.weapon2.currentWeapon = 1;
        weapons.weapon1.currentBarrel = saving.data.lastLoadout.weapon1.currentBarrel;
        weapons.weapon1.currentMagazine = saving.data.lastLoadout.weapon1.currentMagazine;
        weapons.weapon2.currentBarrel = saving.data.lastLoadout.weapon2.currentBarrel;
        weapons.weapon2.currentMagazine = saving.data.lastLoadout.weapon2.currentMagazine;
    }

    public void LoadData()
    {
        OverWriteWeaponData();
        StartCustomization(1);
    }

    public void SaveData()
    {
        saving.data.lastLoadout = weapons;
    }

    //WeaponSelect
    public void StartWeaponSelect(int weaponSlot)
    {
        currentlyEditing = weaponSlot;
    }

    public void SimpleHoverWeaponDisplay(int weaponIndex)
    {
        if (currentWeapon)
            Destroy(currentWeapon);
    }

    public void ChangeWeapon(int index)
    {
        if(currentlyEditing == 1)
        {
            weapons.weapon1.currentWeapon = index;
            weapons.weapon1.currentBarrel = 0;
            weapons.weapon1.currentMagazine = 0;
        }
        else
        {
            weapons.weapon2.currentWeapon = index;
            weapons.weapon2.currentBarrel = 0;
            weapons.weapon2.currentMagazine = 0;
        }
        DisplayCurrentThings();

        SaveData();
    }
    //CustomizationStart
    public void StartCustomization(int index)
    {
        currentlyEditing = index;
        DisplayCurrentThings();
        DisplayCustomizations();
    }

    public void DisplayCustomizations()
    {
        OverWriteWeaponData();
        foreach (Transform child in customizationLayoutBarrel)
            Destroy(child.gameObject);
        foreach (Transform child in customizationLayourMagazine)
            Destroy(child.gameObject);

        for (int i = 0; i < layout.weapons[(currentlyEditing == 1)? weapons.weapon1.currentWeapon : weapons.weapon2.currentWeapon].barrels.Length; i++)
        {
            GameObject g = Instantiate(cusotmizationPanel, customizationLayoutBarrel);
            g.GetComponent<CustomizationPanel>().SetInformation(i, false, this);
        }

        for (int i = 0; i < layout.weapons[(currentlyEditing == 1) ? weapons.weapon1.currentWeapon : weapons.weapon2.currentWeapon].magazines.Length; i++)
        {
            GameObject g = Instantiate(cusotmizationPanel, customizationLayourMagazine);
            g.GetComponent<CustomizationPanel>().SetInformation(i, true, this);
        }
    }

    public void ChangeBarrel(int index)
    {
        if (currentlyEditing == 1)
            weapons.weapon1.currentBarrel = index;
        else
            weapons.weapon2.currentBarrel = index;
        DisplayCurrentThings();

        SaveData();
    }
    public void ChangeMagazine(int index)
    {
        if (currentlyEditing == 1)
            weapons.weapon1.currentMagazine = index;
        else
            weapons.weapon2.currentMagazine = index;
        DisplayCurrentThings();

        SaveData();
    }

    //Display
    public void DisplayCurrentThings()
    {
        if (currentWeapon)
            Destroy(currentWeapon);
        WeaponClassData displayData = (currentlyEditing == 1) ? weapons.weapon1 : weapons.weapon2;
        currentWeapon = Instantiate(layout.weapons[displayData.currentWeapon].baseWeapon, showLocation.position, showLocation.rotation, showLocation);
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

    [System.Serializable]
    public class WeaponLoadoutSlot
    {
        public WeaponClassData weapon1;
        public WeaponClassData weapon2;
    }

    [System.Serializable]
    public class WeaponClassData
    {
        public int currentWeapon;
        public int  currentMagazine;
        public int currentBarrel;
    }
}
