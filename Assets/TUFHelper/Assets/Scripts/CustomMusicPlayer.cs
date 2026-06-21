using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TUFHelper;
using System;

public class CustomMusicPlayer : MonoBehaviour
{
    public static CustomMusicPlayer instance;
    public AudioSource audioSource;

    public void Awake()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();

        audioSource.volume = 0.3f;

        audioSource.volume *= Persistence.globalVolume * Persistence.musicVolume;

    }

    public IEnumerator LoadAndPlayAudio(string path, float startTimeSeconds = -1)
    {
        //if (!Main.Setting.PlayBackgroundMusic)
        //    yield break;

        if (audioSource == null)
        {
            Debug.LogError("AudioSource not assigned.");
            yield break;
        }

        string url = "file://" + path;
        UnityWebRequest uwr; // = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.OGGVORBIS);
        if (path.EndsWith(".ogg")) uwr = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.OGGVORBIS);
        else if (path.EndsWith(".mp3")) uwr = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);
        else yield break;
        ((DownloadHandlerAudioClip)uwr.downloadHandler).streamAudio = true; // Enable streaming

        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error loading audio: " + uwr.error);
        }
        else
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(uwr);

            MusicControlScript.instance.audioSource.Stop();
            audioSource.clip = clip;

            // Important: Wait until it's ready to set time
            while (clip.loadState != AudioDataLoadState.Loaded)
                yield return null;

            audioSource.time = (startTimeSeconds < 0)
                ? clip.length / 2
                : Mathf.Clamp(startTimeSeconds, 0f, clip.length);

            audioSource.Play();
            isPlayingBackground = false;
        }
    }


    private bool isPlayingBackground = true;
    public void StopPlay()
    {
        this.audioSource.Stop();
        if (!isPlayingBackground)
        {
            MusicControlScript.instance.audioSource.Play();
            isPlayingBackground = true;
        }
    }
}