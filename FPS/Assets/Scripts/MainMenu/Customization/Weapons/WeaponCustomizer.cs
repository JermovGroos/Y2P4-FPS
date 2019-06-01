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

    public Text weapon1Name;
    public Image weapon1Image;
    public Text weapon2Name;
    public Image weapon2Image;

    [HideInInspector]public bool displayWeapon;
    [HideInInspector]public int currentlyEditing;
    GameObject currentWeapon;

    public void Start()
    {
        DisplayCurrentThings();
    }

    public void DisplayCurrentThings()
    {
        weapon1Name.text = layout.weapons[weapon1.currentWeapon].weaponName;
        weapon2Name.text = layout.weapons[weapon2.currentWeapon].weaponName;
        weapon1Image.sprite = layout.weapons[weapon1.currentWeapon].weaponSprite;
        weapon2Image.sprite = layout.weapons[weapon2.currentWeapon].weaponSprite;

        if (displayWeapon)
        {
            if (currentWeapon)
                Destroy(currentWeapon);
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
