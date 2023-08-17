using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// struct for biome description we use this is the c# version to convert between c# and hlsl
[System.Serializable]

[CreateAssetMenu(fileName = "Biome Preset", menuName = "Scriptables/Biome Preset",order =1)]
public class biomeDescription : ScriptableObject
{
    public string biomeName;
    public int id;
    public Vector2 tempRange;
    public Vector2 humRange;
    public Color debugColour;
    public string[] terrainGenTypes;
}


// Need to pass biomeDescription to compute shaders at some point
// but classes arent blittable so we create this slightly lighter struct that
// holds the essential data that we need for that shader and pass it in instead
[System.Serializable]
public struct BiomeDescriptionBlittable
{
    public int id;
    public Vector2 tempRange;
    public Vector2 humRange;
    public Color debugColour;

    public BiomeDescriptionBlittable(int _id, Vector2 _tempRange, Vector2 _humRange,Color _debugCol)
    {
        id = _id;
        tempRange = _tempRange;
        humRange = _humRange;
        debugColour = _debugCol;
    }
}
