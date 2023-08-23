using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 * Code by Ed F
 * www.github.com/edf1101
 */

// Preset for the daylight cycle
[CreateAssetMenu(fileName = "Daylight Cycle Preset", menuName = "Scriptables/Daylight Cycle Preset", order = 2)]
public class dayLightCycle : ScriptableObject
{
    [Header("Colour Settings")]

    public Gradient fogColour;
    public Gradient sunColour;
    public Gradient ambientColour;

}
