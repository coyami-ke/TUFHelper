using NVorbis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TUFHelper;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

public class CustomMusicPlayer : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds0_15 = new WaitForSeconds(0.15f);
    public static CustomMusicPlayer instance;
    public AudioSource audioSource;

    private readonly Dictionary<string, AudioClip> _previewCache = new(StringComparer.OrdinalIgnoreCase);
    private CancellationTokenSource _cts;

    private const int PREVIEW_BYTE_RANGE = 524_288;
    private bool isPlayingBackground = true;

    public void Awake()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = 0.3f * Persistence.globalVolume * Persistence.musicVolume;
    }

    public void PlayLevelPreview(string songUrl)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        if (_previewCache.TryGetValue(songUrl, out AudioClip cachedClip))
        {
            PlayClipWithAutoStop(cachedClip);
            return;
        }

        StartCoroutine(StreamPreviewNative(songUrl, _cts.Token));
    }

    public IEnumerator StreamPreviewNative(string songUrl, CancellationToken ct)
    {
        yield return _waitForSeconds0_15;
        if (ct.IsCancellationRequested) yield break;

        using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(songUrl, AudioType.OGGVORBIS);
        ((DownloadHandlerAudioClip)www.downloadHandler).streamAudio = true;

        www.SendWebRequest();

        while (!www.isDone && www.downloadedBytes < PREVIEW_BYTE_RANGE)
        {
            if (ct.IsCancellationRequested)
            {
                www.Abort();
                yield break;
            }
            yield return null;
        }

        AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.time = 15f;
            audioSource.Play();
        }
    }

    public IEnumerator LoadAndPlayAudio(string path, float startTimeSeconds = -1)
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource not assigned.");
            yield break;
        }

        AudioClip clip = null;

        if (path.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
        {
            Task<(float[] samples, int channels, int sampleRate)> decodeTask = Task.Run(() =>
            {
                using var reader = new VorbisReader(path);
                return DecodeVorbisToSamples(reader, startTimeSeconds, maxDurationSeconds: 30f);
            });

            while (!decodeTask.IsCompleted)
                yield return null;

            if (!decodeTask.IsFaulted)
            {
                var (samples, channels, sampleRate) = decodeTask.Result;
                if (samples != null && samples.Length > 0)
                {
                    int totalSamples = samples.Length / channels;
                    clip = AudioClip.Create(Path.GetFileNameWithoutExtension(path), totalSamples, channels, sampleRate, false);
                    clip.SetData(samples, 0);
                }
            }
        }
        else if (path.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
        {
            string url = "file://" + path;
            using UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);

            ((DownloadHandlerAudioClip)uwr.downloadHandler).streamAudio = false;

            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error loading MP3: " + uwr.error);
                yield break;
            }

            clip = DownloadHandlerAudioClip.GetContent(uwr);
            if (clip != null)
            {
                float targetTime = (startTimeSeconds < 0) ? clip.length / 2f : startTimeSeconds;
                audioSource.clip = clip;
                audioSource.time = Mathf.Clamp(targetTime, 0f, clip.length);
            }
        }

        if (clip != null)
        {
            MusicControlScript.instance.audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
            isPlayingBackground = false;
        }
    }

    public IEnumerator PlayAudioStream(string songUrl, AudioType type = AudioType.OGGVORBIS, float startTimeSeconds = 15f)
    {
        if (type == AudioType.OGGVORBIS)
        {
            Stopwatch sw = Stopwatch.StartNew();

            using UnityWebRequest www = UnityWebRequest.Get(songUrl);
            www.SetRequestHeader("Range", $"bytes=0-{PREVIEW_BYTE_RANGE - 1}");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success && www.responseCode != 206)
            {
                Debug.LogError($"Stream error: {www.error}");
                yield break;
            }

            byte[] rawBytes = www.downloadHandler.data;
            if (rawBytes == null || rawBytes.Length == 0) yield break;

            Task<(float[] samples, int channels, int sampleRate)> decodeTask = Task.Run(() =>
            {
                byte[] repairedOgg = ChopOggPrefixToEos(rawBytes);
                using var ms = new MemoryStream(repairedOgg);
                using var reader = new VorbisReader(ms, true);
                return DecodeVorbisToSamples(reader, startTimeSeconds, maxDurationSeconds: 15f);
            });

            while (!decodeTask.IsCompleted)
                yield return null;

            if (decodeTask.IsFaulted)
            {
                Debug.LogError($"NVorbis Stream Decode Error: {decodeTask.Exception?.InnerException?.Message}");
                yield break;
            }

            var (samples, channels, sampleRate) = decodeTask.Result;

            if (samples != null && samples.Length > 0)
            {
                int totalSamples = samples.Length / channels;
                AudioClip clip = AudioClip.Create("StreamPreview", totalSamples, channels, sampleRate, false);
                clip.SetData(samples, 0);

                PlayClipWithAutoStop(clip);
            }

            sw.Stop();
            Main.Logger.Log($"Started playing stream preview in: {sw.Elapsed.TotalSeconds:F2}s");
        }
        else
        {
            using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(songUrl, type);
            ((DownloadHandlerAudioClip)www.downloadHandler).streamAudio = false;

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Stream error: {www.error}");
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            if (clip != null)
            {
                MusicControlScript.instance.audioSource.Stop();
                audioSource.clip = clip;
                if (startTimeSeconds > 0 && startTimeSeconds < clip.length)
                    audioSource.time = startTimeSeconds;
                audioSource.Play();
                isPlayingBackground = false;
            }
        }
    }

    public void StopPlay()
    {
        audioSource.Stop();
        if (!isPlayingBackground)
        {
            MusicControlScript.instance.audioSource.Play();
            isPlayingBackground = true;
        }
    }

    private void PlayClipWithAutoStop(AudioClip clip)
    {
        MusicControlScript.instance.audioSource.Stop();
        audioSource.Stop();

        audioSource.loop = true;

        audioSource.clip = clip;
        audioSource.time = 0f;
        audioSource.Play();

        isPlayingBackground = false;
    }

    private (float[] samples, int channels, int sampleRate) DecodeVorbisToSamples(VorbisReader reader, float startTimeSeconds, float maxDurationSeconds)
    {
        int channels = reader.Channels;
        int sampleRate = reader.SampleRate;
        long totalSamples = reader.TotalSamples;
        float totalDuration = (float)totalSamples / sampleRate;

        float targetTime = (startTimeSeconds < 0) ? totalDuration / 2f : startTimeSeconds;
        targetTime = Mathf.Clamp(targetTime, 0f, totalDuration);

        long startSample = (long)(targetTime * sampleRate);
        if (startSample >= totalSamples) startSample = 0;

        reader.SamplePosition = startSample;

        long maxSamplesToRead = (long)(maxDurationSeconds * sampleRate);
        long remaining = totalSamples - startSample;
        long samplesToRead = Math.Min(maxSamplesToRead, remaining);

        int floatCount = (int)(samplesToRead * channels);
        float[] samples = new float[floatCount];

        int readFloats = reader.ReadSamples(samples, 0, floatCount);
        if (readFloats < floatCount)
            Array.Resize(ref samples, readFloats);

        return (samples, channels, sampleRate);
    }


    private byte[] ChopOggPrefixToEos(byte[] raw)
    {
        if (raw.Length < 4 || Encoding.ASCII.GetString(raw, 0, 4) != "OggS")
            throw new Exception("Data is not a valid Ogg bitstream.");

        List<byte[]> pages = ExtractCompletePages(raw);
        if (pages.Count == 0)
            throw new Exception("No complete Ogg pages found in byte range.");

        using MemoryStream ms = new MemoryStream();
        for (int i = 0; i < pages.Count; i++)
        {
            byte[] page = pages[i];
            if (i == pages.Count - 1)
            {
                page[5] |= 0x04;
                Array.Clear(page, 22, 4); 

                uint crc = CalculateOggCrc(page);
                byte[] crcBytes = BitConverter.GetBytes(crc);
                Array.Copy(crcBytes, 0, page, 22, 4);
            }
            ms.Write(page, 0, page.Length);
        }
        return ms.ToArray();
    }

    private List<byte[]> ExtractCompletePages(byte[] buf)
    {
        List<byte[]> pages = new List<byte[]>();
        int off = 0;
        int n = buf.Length;

        while (off + 27 <= n)
        {
            if (buf[off] != 'O' || buf[off + 1] != 'g' || buf[off + 2] != 'g' || buf[off + 3] != 'S')
            {
                int next = Array.IndexOf(buf, (byte)'O', off + 1);
                if (next < 0) break;
                off = next;
                continue;
            }

            int nsegs = buf[off + 26];
            int hdrEnd = off + 27 + nsegs;
            if (hdrEnd > n) break;

            int bodyLen = 0;
            for (int i = 0; i < nsegs; i++)
                bodyLen += buf[off + 27 + i];

            int pageEnd = hdrEnd + bodyLen;
            if (pageEnd > n) break;

            byte[] page = new byte[pageEnd - off];
            Buffer.BlockCopy(buf, off, page, 0, page.Length);
            pages.Add(page);

            off = pageEnd;
        }

        return pages;
    }

    private uint CalculateOggCrc(byte[] data)
    {
        uint crc = 0;
        foreach (byte b in data)
        {
            crc ^= (uint)b << 24;
            for (int i = 0; i < 8; i++)
            {
                if ((crc & 0x80000000) != 0)
                    crc = (crc << 1) ^ 0x04C11DB7;
                else
                    crc <<= 1;
            }
        }
        return crc;
    }
}