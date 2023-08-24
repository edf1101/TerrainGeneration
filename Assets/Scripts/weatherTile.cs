using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/* 
 * Code by Ed F
 * www.github.com/edf1101
 */

public class weatherTile : MonoBehaviour
{

    

    private ParticleSystem ps; // reference to my particle system

    // average data from the last check 
    private float avgHum; // humidity ie is it raining
    private Color avgCol;// color for if its snow or rain

    private Vector2 tile; // what tile were in
    private Vector2 offset; // and where in that tile

    // last color and state so we know if weve gone to a new one
    private Color lastCol = Color.black; 
    private bool lastState;

    private tileManager TM; // reference to the tileManager for all these weatherTiles

    private bool doneOnce; // gets set to true after calibrated once

    void Start() // called before first frame
    {
        ps = GetComponentInChildren<ParticleSystem>();  // set the particle system
    }

    private float lastCheck; // used to check something every n secs
    void Update() // called each frame
    {
        if (Time.time - lastCheck > 0.5f) // every 0.5 secs check this
        {
            lastCheck = Time.time;
            calibrate();
        }
    }

   

    public bool getState() // public getter for private variable lastState
    {
        return lastState;
    }

    // This updates the ParticleSystem so it shows up correctly
    private void calibrate()
    {
        // get additional moving noise so it varies
        float noise = (terrainNoise.GetNoise(new Vector3(tile.x*100+offset.x,0,tile.y*100+offset.y) + new Vector3(1, 0, 1) * Time.time /2f, "humMap") * 0.6f) - 0.3f;

        // get what humidity and color it should be according to biome 
        avgHum = TM.getBiomeAt(offset).biomeHumidity + noise;
        avgCol = TM.getBiomeAt(offset).particleCol;

        // make it dark if nighttime so doesnt show up poorly
        if (weatherManager.getTime() > 16 || weatherManager.getTime() < 6.5f)
            avgCol = new Color(0.1f, 0.1f, 0.15f, 0.5f);

        // get state and last state so we know if it changes
        lastState = ps.isPlaying;
        bool state = (avgHum > 0.55f);

        // if it changed then update the PS
        if (state != lastState || lastCol != avgCol || !doneOnce)
        {
            doneOnce = true;

            // deprecated I know but its much easier
            ps.startColor = avgCol;

            // update last colour and state
            lastCol = avgCol;
            lastState = state;

            // if state changed then set it here
            if (state)
            {
                ps.Stop();
                ps.Clear();
                ps.Play();
            }
            else
            {
                ps.Stop();
                ps.Clear();
               // Debug.Log("stop");
            }

        }

    }

    private bool hasWater; // wheteher water in small tile
    private static AudioClip waterSound;// sound of water running

    public static void setWaterSound(AudioClip _sound)
    {
        waterSound = _sound;
    }

    // this is the setup procedure when it gets instantiates
    public void newLocation(Vector3 pos)
    {
        // if PS not set yet assign it here
        if(ps==null)
            ps = GetComponentInChildren<ParticleSystem>();

        // stop Particle system and clear it
        ps.Stop();
        ps.Clear();

        // calculate the tile and where in the tile
        tile = new Vector2(Mathf.Floor(pos.x / 100f), Mathf.Floor(pos.z / 100f));
        offset = new Vector2(pos.x,pos.z) - 100 * tile;

        
        TM = TerrainManager.getTileObject(tile).GetComponent<tileManager>();

        
        lastState = !lastState;

        // calibrate for first time
        calibrate();

        // do a check for water below
        bool breaking=false;
        Vector3 waterLocation=Vector3.zero  ;
        for(int dx = 3; dx < 8; dx += 2)
        {

            for (int dy = 3; dy < 8; dy += 2)
            {
                Vector3 rayPosition = new Vector3(tile.x * 100 + offset.x + dx, 70, tile.y * 100 + offset.y + dy);
             
                RaycastHit hit;

                if (Physics.Raycast(rayPosition,Vector3.down, out hit, 100))
                {
                    if(hit.collider.gameObject.layer==4) // 4 is water layer
                    {
                        breaking = true;
                        hasWater = true;
                        waterLocation = hit.point;
                        break;
                    }
                }
            }
            if (breaking) // this means we can break both loops
                break;

        }

        if (hasWater)
        {

            // were not using prefabs so set up the audio source from scratch
            GameObject waterSource = new GameObject();
            waterSource.name = "Water source";
            waterSource.transform.parent = transform;
            waterSource.transform.position = waterLocation;
            waterSource.AddComponent<AudioSource>();
            waterSource.GetComponent<AudioSource>().loop = true;
            waterSource.GetComponent<AudioSource>().clip = waterSound;
            waterSource.GetComponent<AudioSource>().Play();
            waterSource.GetComponent<AudioSource>().volume=0.3f;
            waterSource.GetComponent<AudioSource>().maxDistance = 12;
            waterSource.GetComponent<AudioSource>().spatialBlend = 1;
            waterSource.GetComponent<AudioSource>().spread = 0.8f;

        }
        
    }
    
}
