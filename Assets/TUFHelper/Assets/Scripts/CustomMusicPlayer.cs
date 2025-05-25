using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TUFHelper;

public class CustomMusicPlayer : MonoBehaviour
{
    public static CustomMusicPlayer instance;
    public AudioSource audioSource;

    public void Awake()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    public IEnumerator LoadAndPlayAudio(string path, float startTimeSeconds = -1)
    {
        if (!Main.Setting.PlayBackgroundMusic) yield return null;

        string url = "file:///" + path.Replace("\\", "/");

        using UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.OGGVORBIS);
        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error loading audio: " + uwr.error);
        }
        else
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(uwr);
            audioSource.clip = clip;
            if (startTimeSeconds == -1) audioSource.time = clip.length / 2;
            else if (startTimeSeconds > 0 && startTimeSeconds < clip.length)
            {
                audioSource.time = startTimeSeconds;
            }

            audioSource.Play();
            MusicControlScript.instance.audioSource.Stop();
            isPlayingBackground = false;
        }
    }
    private bool isPlayingBackground = true;
    public void StopPlay()
    {
        this.audioSource.Stop();
        if (Main.Setting.PlayBackgroundMusic && !isPlayingBackground)
        {
            MusicControlScript.instance.audioSource.Play();
            isPlayingBackground = true;
        }
    }
}
