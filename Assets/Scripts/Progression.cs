using UnityEngine;
using System.Collections.Generic;
public class Progression : MonoBehaviour
{
    [TextArea]
    [SerializeField] private string developerNote = "Leave notes in this space in editor";

    [Header("Sparx quantity (per level, last element will be max)")]
    [Tooltip("Quantities for Sparx.")]
    [SerializeField] public List<float> sparxQuantities;
    [Header("Sparx Speeds (per level, last element will be max)")]
    [Tooltip("Speeds for Sparx.")]
    [SerializeField] public List<float> sparxSpeeds;

    [Header("Qix Speeds (per level, last element will be max)")]
    [Tooltip("Speeds for Qix.")]
    [SerializeField] public List<float> qixSpeeds;

    [Header("Level to start at (starting at 0)")]
    [SerializeField] private int currentLevel = 0;


    public float getSparxSpeed() {
        // Get level's speed, or max if level count too high; has no cap on level count
        if (currentLevel < sparxSpeeds.Count) {
            return sparxSpeeds[currentLevel];
        }
        return sparxSpeeds[sparxSpeeds.Count - 1]; 
    }

    public float getQixSpeed() {
        // Get level's speed, or max if level count too high; has no cap on level count
        if (currentLevel < qixSpeeds.Count) {
            return qixSpeeds[currentLevel];
        }
        return qixSpeeds[qixSpeeds.Count - 1]; 
    }

    public float getSparxQuantity() {
        // Get level's quantity, or max if level count too high; has no cap on level count
        if (currentLevel < sparxQuantities.Count) {
            return sparxQuantities[currentLevel];
        }
        return sparxQuantities[sparxQuantities.Count - 1]; 
    }

    public int incrementLevel() {
        //Should be called by GameManager when a level is complete (before getting speeds and quantities)
        currentLevel += 1;
        return currentLevel;
    }

}
