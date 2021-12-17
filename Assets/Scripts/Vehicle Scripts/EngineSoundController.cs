using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineSoundController : MonoBehaviour
{
    public float smoothingFactor = 0.1f;

    public float RPM;
    public float baseRPM;
    public float maxRPM;
    public float minRPM;

    public float throttle;

    float soundRPM;

    public float filterValue;
    public float lowMinimumValue;
    public float throttleSmothingFactor;

    public AudioSource highAudioSource;
    public AudioHighPassFilter highAudioFilter;

    public AudioSource lowAudioSource;
    public AudioLowPassFilter lowAudioFilter;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        soundRPM += (Mathf.Clamp(RPM, minRPM, maxRPM)- soundRPM) * smoothingFactor;

        
        if(float.IsNaN(soundRPM) || float.IsInfinity(soundRPM))
        {
            soundRPM = RPM;
        }

        float pitch = soundRPM / baseRPM;
        
        if (highAudioSource.time != lowAudioSource.time)
            lowAudioSource.time = highAudioSource.time;

        if (float.IsNaN(pitch) || float.IsInfinity(pitch))
        {
            Debug.LogWarning(string.Format("Pitch: {0} SoundRPM: {1} BaseRPM: {2}", pitch, soundRPM, baseRPM));
            pitch = 1;
        }
        highAudioSource.pitch = pitch;
        lowAudioSource.pitch = pitch;

        highAudioFilter.cutoffFrequency = filterValue * pitch;
        lowAudioFilter.cutoffFrequency = filterValue * pitch;

        lowAudioSource.volume = lowMinimumValue + (1 - lowMinimumValue) * throttle;
    }

    public void setSound(float nRPM, float nThottle)
    {
        RPM = nRPM;
        throttle += (nThottle - throttle) * throttleSmothingFactor;
    }

    public void initializeSound(AudioClip sound, float nBaseRPM, float nMaxRPM, float nSmoothingFactor)
    {
        highAudioSource.clip = sound;
        lowAudioSource.clip = sound;

        if (nBaseRPM == 0)
            nBaseRPM = 2000;
        baseRPM = nBaseRPM;
        maxRPM = nMaxRPM;
        smoothingFactor = nSmoothingFactor;

        highAudioSource.Play();
        lowAudioSource.Play();
    }
}
