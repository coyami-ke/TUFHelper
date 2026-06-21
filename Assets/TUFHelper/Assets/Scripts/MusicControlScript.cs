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
        // Enforce Singleton: If an instance already exists, destroy this duplicate
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void Start()
    {
        // Apply volumes safely. 
        audioSource.volume = Main.Setting.TUFHelperMusicVolume;

        if (CustomMusicPlayer.instance != null && CustomMusicPlayer.instance.audioSource != null)
        {
            CustomMusicPlayer.instance.audioSource.volume = Main.Setting.TUFHelperMusicVolume;
        }
    }
}