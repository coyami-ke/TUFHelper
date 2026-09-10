using DG.Tweening;
using TUFHelper;
using TUFHelper.Utils;
using UnityEngine;

public class MusicControlScript : MonoBehaviour
{
    public static MusicControlScript instance;
    public AudioSource audioSource;
    private bool isPlaying = true;

    public void Awake()
    {
        instance = this;
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void Start()
    {
        if (CustomMusicPlayer.instance != null && CustomMusicPlayer.instance.audioSource != null)
        {
            CustomMusicPlayer.instance.audioSource.volume = Main.Setting.TUFHelperMusicVolume;
        }

        if (audioSource != null)
        {
            audioSource.volume = Main.Setting.TUFHelperMusicVolume;
        }
    }

    public void PauseBackgroundMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
            isPlaying = false;
        }
    }

    public void ResumeBackgroundMusic()
    {
        if (audioSource != null && !isPlaying)
        {
            audioSource.UnPause();
            isPlaying = true;
        }
    }
}