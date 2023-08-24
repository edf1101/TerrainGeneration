using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

/* 
 * Code by Ed F
 * www.github.com/edf1101
 */

public class introController : MonoBehaviour
{
    [SerializeField] private GameObject canvas; // will be set to active once vid intro done
    [SerializeField] private VideoPlayer myVideo;

    void Start() // called before first frame
    {
        
    }

    
    void Update() // called each frame
    {
        if(!myVideo.isPlaying && Time.time > 4)
        {
            canvas.SetActive(true);
        }
    }

    // these get called by buttons

    public void Exit()
    {
        Application.Quit();
    }
    public void LoadGame()
    {
        SceneManager.LoadScene(1); // game scene is index 1
    }
}
