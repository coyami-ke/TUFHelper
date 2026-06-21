using System.Collections;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;

public class CustomMusicPlayer : MonoBehaviour
{
    public static CustomMusicPlayer instance;
    public AudioSource audioSource;
    private bool isPlayingBackground = true;
    private UnityWebRequest activeWebRequest; // Track the active request to abort it safely

    public void Awake()
    {
        instance = this;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        audioSource.volume = 0.3f;
        // Ensure Persistence checks don't crash if accessed too early
        try
        {
            audioSource.volume *= Persistence.globalVolume * Persistence.musicVolume;
        }
        catch { }
    }

    public IEnumerator LoadAndPlayAudio(string path, float startTimeSeconds = -1)
    {
        if (audioSource == null) yield break;

        // Clean up any existing stream before starting a new one
        AbortActiveStream();

        string url = "file://" + path;
        AudioType audioType = AudioType.UNKNOWN;

        if (path.EndsWith(".ogg")) audioType = AudioType.OGGVORBIS;
        else if (path.EndsWith(".mp3")) audioType = AudioType.MPEG;
        else yield break;

        using (activeWebRequest = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
        {
            ((DownloadHandlerAudioClip)activeWebRequest.downloadHandler).streamAudio = true;
            UnityWebRequestAsyncOperation operation = activeWebRequest.SendWebRequest();

            while (!operation.isDone && activeWebRequest.downloadedBytes < 1024)
            {
                yield return null;
            }

            // If the request was aborted via menu quit, exit gracefully
            if (activeWebRequest == null || activeWebRequest.result == UnityWebRequest.Result.ConnectionError || activeWebRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                yield break;
            }

            if (MusicControlScript.instance != null && MusicControlScript.instance.audioSource != null)
            {
                MusicControlScript.instance.audioSource.Stop();
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(activeWebRequest);
            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.time = (startTimeSeconds < 0)
                    ? clip.length / 2
                    : Mathf.Clamp(startTimeSeconds, 0f, clip.length);

                audioSource.Play();
                isPlayingBackground = false;
            }

            while (!operation.isDone)
            {
                yield return null;
            }
        }
        activeWebRequest = null;
    }

    public void AbortActiveStream()
    {
        StopAllCoroutines();

        if (activeWebRequest != null)
        {
            activeWebRequest.Abort(); // Force-kill libcurl connection immediately
            activeWebRequest.Dispose();
            activeWebRequest = null;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }
    }

    private void OnDestroy()
    {
        AbortActiveStream();
        if (instance == this) instance = null;
    }
}

//[HarmonyPatch] i dont fucking know how to fix this :sob:
//public static class CustomMusicPlayerScenePatches
//{
//    [HarmonyPatch(typeof(scrController), "QuitToMainMenu")]
//    [HarmonyPatch(typeof(scrController), "QuitToMenu")]
//    [HarmonyPrefix]
//    public static void PrefixQuit()
//    {
//        if (CustomMusicPlayer.instance != null)
//        {
//            CustomMusicPlayer.instance.AbortActiveStream();
//        }
//    }
//}