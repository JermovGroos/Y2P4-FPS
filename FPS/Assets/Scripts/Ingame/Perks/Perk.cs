using UnityEngine;

[CreateAssetMenu(fileName = "New Perk", menuName = "Perk")]
public class Perk : ScriptableObject
{
    public string className; //Name of the class
    public string variableName; //Name of the variable
    public float toAdd = 0; //Value to add to the variable
    public float toMultiply = 1; //Value to multiply the variable with
}
