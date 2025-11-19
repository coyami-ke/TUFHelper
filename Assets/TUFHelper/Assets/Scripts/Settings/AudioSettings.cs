using System.Collections;
using System.Collections.Generic;
using TMPro;
using TUFHelper;
using UnityEngine;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    public Toggle playTUFHelperMusic;

    public void Start()
    {
        playTUFHelperMusic.isOn = Main.Setting.PlayBackgroundMusic;
    }

    public void OnPlayTUFHelperMusicChanged(bool value)
    {
        Main.Setting.PlayBackgroundMusic = value;
        Main.Setting.Save(Main.ModEntry);

        if (value) MusicControlScript.instance.audioSource.Play();
        else MusicControlScript.instance.audioSource.Stop();
    }
}
