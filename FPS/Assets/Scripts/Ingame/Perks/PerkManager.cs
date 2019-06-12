using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class PerkManager : MonoBehaviour
{
    public List<Perk> perks = new List<Perk>();

    public void ApplyPerk(GameObject holder, Perk perk)
    {
        //Get value of requested variable
        Component component = holder.GetComponent(perk.className); //Get component
        FieldInfo field = component.GetType().GetField(perk.variableName); //Get field
        float value = (float)field.GetValue(component); //Get value

        //Add value to said variable
        value *= perk.toMultiply; //Multiply current value with perk multiplier
        value += perk.toAdd; //Add perk value to current value

        //Set new value
        field.SetValue(component, value); //Set variable
    }
}
