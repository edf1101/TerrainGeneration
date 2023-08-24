using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 * Code by Ed F
 * www.github.com/edf1101
 */

public class weatherManager : MonoBehaviour
{
    // References to other transforms
    [Header("Transform References")]
    [SerializeField] private Transform playerReference;
    [SerializeField] private Transform sunReference;
    [SerializeField] private Transform soundTransform;
    private Camera myCam;


    // Settings for time passing
    [Header("Time Settings")]
    [SerializeField] private float timeSpeed = 0.05f;
    [SerializeField] private AnimationCurve dayNightCurve;
    private float rawTime = 12;
    private static float timeOfDay;


    // Settings for weather /lighting
    [Header("Weather / Lighting Settings")]
    [SerializeField] private Vector2 fogRange;
    [SerializeField] private dayLightCycle myCycle;
    private bool isPrecipitating;
    private float precipitationLerp;
    private float fogValue;


    [Header("Other Settings")]
    [SerializeField] private Vector2 tileSize;
    private Vector2 currentTile;
    private AudioSource weatherAudioSource;



    private void Start() // called before first frame
    {
        myCam = playerReference.GetComponentInChildren<Camera>(); // set camera reference
        weatherAudioSource = soundTransform.GetComponentInChildren<AudioSource>();
    }



    private void Update() // called each frame
    {

        updatePrecipitation();
        updateTime();
        updateColours();
        updateSounds();


    }

    private float lastUpdateSound; // used to update sound periodically

    private void updateSounds() // update weather sounds
    {
        if (!weatherAudioSource.isPlaying)
            weatherAudioSource.Play();
        soundTransform.position = playerReference.position + Vector3.up * 1;
        if (Time.time - lastUpdateSound > 2f)
        {
            lastUpdateSound = Time.time;
           

            currentTile = new Vector2(Mathf.FloorToInt(playerReference.position.x / 100), Mathf.FloorToInt(playerReference.position.z / 100));

            // calculate where I am within that tile
            Vector2 smallPart1 = new Vector2(playerReference.position.x, playerReference.position.z) - currentTile * 100;
            smallPart1 = new Vector2((int)smallPart1.x, (int)smallPart1.y);

            // fetch current biome from that data
            biomeDescription currentBiome = TerrainManager.getTileObject(currentTile).GetComponent<tileManager>().getBiomeAt(smallPart1);
           
            // if sounds changed update them
            if (weatherAudioSource.clip != currentBiome.weatherSound)
            {
                weatherAudioSource.clip = currentBiome.weatherSound;
                weatherAudioSource.Play();

            }
        }
    }

    // once time calculated update colours accordingly
    private void updateColours()
    {
        // fog must match colour with skybox color
        RenderSettings.fogColor = myCycle.fogColour.Evaluate(timeOfDay / 24f);
        myCam.backgroundColor = myCycle.fogColour.Evaluate(timeOfDay / 24f);


        RenderSettings.ambientLight = myCycle.ambientColour.Evaluate(timeOfDay / 24f);


        RenderSettings.sun.color = myCycle.sunColour.Evaluate(timeOfDay / 24f);

        // also update angle of the sun
        sunReference.localEulerAngles = new Vector3(((timeOfDay / 24f) * 360f +270f)%180, -30, 0);
    }


    private float lastUpdate; // used to check something every n secs

    private void updatePrecipitation()
    {
        if (Time.time - lastUpdate > 1.0f) // recheck every 1s
        {
            lastUpdate = Time.time;

            // calculate what tile im in and where i am in it
            currentTile = new Vector2(Mathf.FloorToInt(playerReference.position.x / tileSize.x), Mathf.FloorToInt(playerReference.position.z / tileSize.y));
            Vector2 temp= new Vector2(playerReference.position.x, playerReference.position.z) - currentTile * 100;
            Vector2 smallTile = new Vector2(Mathf.Floor(temp.x / 10f) * 10f, Mathf.Floor(temp.y / 10f) * 10f);
           
            // is it precipitating where I am? 
            isPrecipitating = TerrainManager.getTileObject(currentTile).GetComponent<tileManager>().getPrecipating(smallTile);

            // update whether weather sound is playing or not
           
        }

        // adjust fog density according to whether its precipitating or not
        if (isPrecipitating)
            precipitationLerp += 0.03f * Time.deltaTime;
        else
            precipitationLerp -= 0.03f * Time.deltaTime;

        precipitationLerp = Mathf.Clamp01(precipitationLerp);
        fogValue = Mathf.Lerp(fogRange.x, fogRange.y, precipitationLerp);
        RenderSettings.fogDensity = fogValue;

        weatherAudioSource.volume =Mathf.Clamp01(precipitationLerp*8); // this will help audio volume move smoothly
    }

    // update the time according to a curve
    private void updateTime()
    {
        rawTime += Time.deltaTime * timeSpeed;
        rawTime %= 24;
        timeOfDay = dayNightCurve.Evaluate(rawTime / 24f) * 24f;
    }

    // get the time for other scripts
    public static float getTime()
    {
        return timeOfDay;
    }

} 