using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class PerkManager : MonoBehaviour
{
    public List<Perk> activePerks = new List<Perk>();

    public void ApplyPerk(GameObject holder, Perk perk)
    {
        //Get value of requested variable
        Component component = holder.GetComponent(perk.className); //Get component
        FieldInfo field = component.GetType().GetField(perk.variableName); //Get field
        float oldValue = (float)field.GetValue(component); //Get value

        //Add value to said variable
        float newValue = oldValue; //Create new value variable
        newValue *= perk.toMultiply; //Multiply current value with perk multiplier
        newValue += perk.toAdd; //Add perk value to current value

        //Set new value
        field.SetValue(component, oldValue); //Set variable

        //Add perk to active perks
        activePerks.Add(perk); //Add perk to active perks

        //Log to console
        print("Successfully changed " + component.name + "." + field.Name + " on " + holder.name + " from " + oldValue + " to " + newValue);
    }

    public void RemovePerk(GameObject holder, Perk perk)
    {

    }
}
