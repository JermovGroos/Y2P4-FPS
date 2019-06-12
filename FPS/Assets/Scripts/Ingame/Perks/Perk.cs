using UnityEngine;

[CreateAssetMenu(fileName = "New Perk", menuName = "Perk")]
public class Perk : ScriptableObject
{
    [Header("Info")]
    [Tooltip("In-game name of the perk.")] public new string name; //In-game name of the perk
    [Tooltip("In-game description of the perk.")] public string description; //In-game description of the perk
    [Tooltip("In-game cost of the perk.")] public int cost; //In-game cost of the perk

    [Header("Reflection")]
    [Tooltip("Name of the class to change a float of")] public string className; //Name of the class
    [Tooltip("Name of the variable to change the value of")] public string variableName; //Name of the variable

    [Header("Changers")]
    [Tooltip("Value to add to the variable")] public float toAdd = 0; //Value to add to the variable
    [Tooltip("Value to multiply the variable with")] public float toMultiply = 1; //Value to multiply the variable with
}
