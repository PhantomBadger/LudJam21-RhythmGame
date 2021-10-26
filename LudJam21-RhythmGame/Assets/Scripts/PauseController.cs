using Assets.Scripts.Song;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseController : MonoBehaviour
{
    public KeyCode PauseKey;
    public SongPlayer SongPlayer;
    public GameObject PauseObject;
    public float PauseCooldownInSeconds = 0.5f;

    private bool isPaused;
    private bool didPauseSong;
    private bool canPause;
    private float prevPitch;
    private DateTime prevUnpause;

    // Start is called before the first frame update
    void Start()
    {
        canPause = true;
        isPaused = false;
        prevUnpause = DateTime.Now;
        PauseObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(PauseKey) && canPause)
        {
            if (isPaused)
            {
                //Unpause 
                isPaused = false;
                Time.timeScale = 1;
                SongPlayer.TargetAudioSource.pitch = prevPitch;
                prevUnpause = DateTime.Now;
                PauseObject.SetActive(false);
            }
            else
            {
                TimeSpan timeSinceLastPause = DateTime.Now - prevUnpause;
                if (timeSinceLastPause.TotalSeconds > PauseCooldownInSeconds)
                {
                    // Pause
                    isPaused = true;
                    Time.timeScale = 0;
                    prevPitch = SongPlayer.TargetAudioSource.pitch;
                    SongPlayer.TargetAudioSource.pitch = 0;
                    PauseObject.SetActive(true);
                }
            }
        }
    }

    public void SetCanPause(bool canPause)
    {
        this.canPause = canPause;
    }
}
