using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomizationPanel : MonoBehaviour
{
    public WeaponCustomizer customizer;
    public bool magazine;
    public int index;
    public Text nameInput;
    public Image sprite;

    public void SetInformation(int _index, bool _magazine, WeaponCustomizer _customizer)
    {
        customizer = _customizer;
        index = _index;
        magazine = _magazine;

        WeaponCustomizationLayout.Weapon weapon = customizer.layout.weapons[(customizer.currentlyEditing == 1) ? customizer.weapon1.currentWeapon : customizer.weapon2.currentWeapon];
        nameInput.text = magazine ? weapon.magazines[index].objName : weapon.barrels[index].objName;
    }

    public void OnButtonPressed()
    {
        if (magazine)
            customizer.ChangeMagazine(index);
        else
            customizer.ChangeBarrel(index);
        Debug.Log("Yes");
    }
}
