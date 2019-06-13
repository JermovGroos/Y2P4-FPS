using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class PerkManager : MonoBehaviour
{
    public List<Perk> activePerks = new List<Perk>(); //List of active perks

    public virtual void ApplyPerk(GameObject holder, Perk perk)
    {
        if (!activePerks.Contains(perk))
        {
            //Get value of requested variable
            Component component = holder.GetComponent(perk.className); //Get component
            FieldInfo field = component.GetType().GetField(perk.variableName); //Get field
            float oldValue = (float)field.GetValue(component); //Get value

            //Add perk value to said variable
            float newValue = oldValue; //Create new value variable
            newValue *= perk.toMultiply; //Multiply current value with perk multiplier
            newValue += perk.toAdd; //Add perk value to current value

            //Set new value
            field.SetValue(component, newValue); //Set variable

            //Add perk to active perks
            activePerks.Add(perk); //Add perk to active perks

            //Log to console
            print("Successfully changed " + component.name + "." + field.Name + " on " + holder.name + " from " + oldValue + " to " + newValue);
        }
    }

    public virtual void RemovePerk(GameObject holder, Perk perk)
    {
        if (activePerks.Contains(perk))
        {
            //Get value of requested variable
            Component component = holder.GetComponent(perk.className); //Get component
            FieldInfo field = component.GetType().GetField(perk.variableName); //Get field
            float oldValue = (float)field.GetValue(component); //Get value

            //Remove perk value from said variable
            float newValue = oldValue; //Create new value variable
            newValue /= perk.toMultiply; //Devide new value by perk multiplication
            newValue -= perk.toAdd; //Subtract new value by perk addition

            //Set new value
            field.SetValue(component, newValue); //Set variable

            //Remove perk from active perks
            activePerks.Remove(perk);

            //Log to console
            print("Successfully changed " + component.name + "." + field.Name + " on " + holder.name + " from " + oldValue + " to " + newValue);
        }
    }
}
