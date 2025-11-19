using DG.Tweening;
using TUFHelper;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MusicControlScript : MonoBehaviour
{
    public static MusicControlScript instance;
    public AudioSource audioSource;
    private bool isPlaying = true;
    public void Awake()
    {
        instance = this;        
    }
    public void Start()
    {
        if (!Main.Setting.PlayBackgroundMusic) 
        {
            audioSource.Pause();
            isPlaying = false;
        }
        else
        {
            audioSource.Play();
            isPlaying = true;
        }

        audioSource.volume *= Persistence.globalVolume * Persistence.musicVolume;
    }
}