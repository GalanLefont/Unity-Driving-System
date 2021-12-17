using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;

public class Settings : MonoBehaviour
{
    public AudioMixer audioMixer;

    static float volume;

    public bool muteEffects = false;

    static bool isFullscreen;

    public TMP_Dropdown resolutionDropdown;
    Resolution[] resolutions;
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;

        isFullscreen = Screen.fullScreen;

        setVolume(volume);

        if (resolutionDropdown != null)
            setResolutionDropdown();
    }

    public void setResolutionDropdown()
    {
        resolutionDropdown.ClearOptions();

        resolutions = Screen.resolutions;

        List<string> options = new List<string>();
        int currentResolution = resolutions.Length - 1;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = string.Format("{0} x {1} {2}Hz", resolutions[i].width, resolutions[i].height, resolutions[i].refreshRate);
            options.Add(option);

            if (
                resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height &&
                resolutions[i].refreshRate == Screen.currentResolution.refreshRate)
            {
                currentResolution = i;
            }
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolution;
        resolutionDropdown.RefreshShownValue();
    }

    public void setVolume(float volume)
    {
        
    }

    public void setResolution(int index)
    {
        Screen.SetResolution(resolutions[index].width, resolutions[index].height, isFullscreen, resolutions[index].refreshRate);
    }

    public void setQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
    }

    public void setFullScreen(bool setFullscreenValue)
    {
        isFullscreen = setFullscreenValue;
        Screen.fullScreen = setFullscreenValue;
    }
}
