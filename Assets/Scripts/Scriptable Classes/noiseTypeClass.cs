using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// struct for biome description we use this is the c# version to convert between c# and hlsl
[System.Serializable]

[CreateAssetMenu(fileName = "TerrainNoise Preset", menuName = "Scriptables/TerrainNoise Preset", order = 2)]
public class terrainNoisePreset : ScriptableObject
{
    //Basics

    public string terrainName; // name of terrain type
    public float heightMult; // Basic Height multiplier
    public float frequency; // basic frequency


    // Fractal noise stuff

    public int octaves;
    public float gain;
    public float lacunarity;


    // Advanced
}