using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class terrainNoise : MonoBehaviour
{

    // this holds the each noisePreset which we can find by inputting its name
    private static Dictionary<string, terrainNoisePreset> noisePresets = null;

    private static FastNoiseLite noiseGenerator = new FastNoiseLite();

    // this function will return a height value for a given type of noise
    public static float GetNoise(Vector3 position,string noiseType)
    {
        float noiseValue = 0; // initialise height value

        // this is the preset we will use
        terrainNoisePreset noisePreset = noisePresets[noiseType];

        //basics
        noiseGenerator.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noiseGenerator.SetFrequency(noisePreset.frequency);

        //set noise generator to use fractal features
        noiseGenerator.SetFractalGain(noisePreset.gain);
        noiseGenerator.SetFractalLacunarity(noisePreset.lacunarity);
        noiseGenerator.SetFractalOctaves(noisePreset.octaves);

        // calculate noise at that position
        float rawNoise = noiseGenerator.GetNoise(position.x, position.z);
        float fixedNoise = (rawNoise + 1f) / 2f;
       // Debug.Log(rawNoise);
        // and add it to the height value
        noiseValue += fixedNoise * noisePreset.heightMult;

        return noiseValue;
    }

    // use this to set the seed
    public static void setSeed(int _seed)
    {
        noiseGenerator.SetSeed(_seed);
    }

    // this function will set up the different types of noise in the generator
    public static void setNoisePresets(List<terrainNoisePreset> _presets)
    {
        //check we havent already set up and data we're given is valid
        if(noisePresets==null && _presets.Count > 0)
        {
            //initialise as dictionary
            noisePresets = new Dictionary<string, terrainNoisePreset>();

            for(int i = 0; i < _presets.Count; i++)
            {
                // add in each type to the dictionary
                noisePresets.Add(_presets[i].terrainName, _presets[i]);
            }
        }
    }

}
