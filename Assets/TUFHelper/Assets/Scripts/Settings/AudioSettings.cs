using System.Collections;
using System.Collections.Generic;
using TMPro;
using TUFHelper;
using UnityEngine;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    public Slider volumeSlider;
    public void Start()
    {
        volumeSlider.value = Main.Setting.TUFHelperMusicVolume;
    }
    public void OnVolumeChanged(float value)
    {
        CustomMusicPlayer.instance.audioSource.volume = value;
        MusicControlScript.instance.audioSource.volume = value;
        Main.Setting.TUFHelperMusicVolume = value;

        Main.Setting.Save(Main.ModEntry);
    }
}
