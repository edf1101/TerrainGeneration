using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// struct for biome description we use this is the c# version to convert between c# and hlsl
[System.Serializable]

[CreateAssetMenu(fileName = "TerrainNoise Preset", menuName = "Scriptables/TerrainNoise Preset", order = 2)]
public class terrainNoisePreset : ScriptableObject
{

    //Basics
    [Header("Basic Settings")]
    public string terrainName; // name of terrain type
    public float heightMult; // Basic Height multiplier
    public float frequency; // basic frequency
    public int seedOffset; // how much we offset the standard seed for each terrain type


    // Fractal noise stuff
    [Header("Fractal Noise Settings")]
    public int octaves; // octaves of fractal noise
    public float gain; // how the intensity of each octave varies
    public float lacunarity; // how frequency of each octave varies


    // settings for plateaus (when main noise is rounded to nearest 10 or sumn
    // and minor noise added on top
    [Header("Plateau Settings")]
    public bool doingPlateau = false; // bool to explain if we're doing it or not
    public float plateauGap=20; // the interval for plateaus


    //Holds Dune Settings
    [Header("Dune Settings")]
    public bool isDune = false;


    // Holds advanced settings
    [Header("Advanced Settings")]
    // This curve maps how input values go to output values
    public AnimationCurve behaviourCurve = null; 
    public blendOptions blendOption = new blendOptions(); // blend with add or mult


    public enum blendOptions
    {
        Addtion,
        Multiplication
    };
}

