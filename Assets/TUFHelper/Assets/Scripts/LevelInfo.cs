using System;
using System.Collections;
using TMPro;
using TUFHelper.ModScripts.Json;
using UnityEngine;

public class LevelInfo : MonoBehaviour
{
    public static LevelInfo instance;

    public TextMeshProUGUI bpm, lenght, tiles;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.4f;

    private Coroutine bpmCoroutine;
    private Coroutine tilesCoroutine;
    private Coroutine lengthCoroutine;

    private float currentBpm = 0f;
    private float currentTiles = 0f;
    private float currentLengthMs = 0f;

    public void Awake()
    {
        if (instance == null) instance = this;
    }

    private bool isShow = true;
    public bool IsShow
    {
        get => isShow;
        set
        {
            isShow = value;
            UITransition.SetVisible(gameObject, value);
        }
    }

    public void LoadLevelInfo(LevelListInfoElementJson info)
    {
        if (info == null)
        {
            StopAllAnimationCoroutines();
            bpm.text = "unknown";
            tiles.text = "unknown";
            lenght.text = "unknown";
            currentBpm = 0f;
            currentTiles = 0f;
            currentLengthMs = 0f;
            return;
        }
        if (info.BPM.HasValue)
        {
            if (bpmCoroutine != null) StopCoroutine(bpmCoroutine);
            bpmCoroutine = StartCoroutine(AnimateNumberRoutine(bpm, currentBpm, info.BPM.Value, val => currentBpm = val));
        }
        else
        {
            if (bpmCoroutine != null) StopCoroutine(bpmCoroutine);
            bpm.text = "unknown";
            currentBpm = 0f;
        }

        if (info.TileCount.HasValue)
        {
            if (tilesCoroutine != null) StopCoroutine(tilesCoroutine);
            tilesCoroutine = StartCoroutine(AnimateNumberRoutine(tiles, currentTiles, info.TileCount.Value, val => currentTiles = val));
        }
        else
        {
            if (tilesCoroutine != null) StopCoroutine(tilesCoroutine);
            tiles.text = "unknown";
            currentTiles = 0f;
        }

        if (info.LevelLengthInMs.HasValue)
        {
            if (lengthCoroutine != null) StopCoroutine(lengthCoroutine);
            lengthCoroutine = StartCoroutine(AnimateTimeRoutine(lenght, currentLengthMs, info.LevelLengthInMs.Value, val => currentLengthMs = val));
        }
        else
        {
            if (lengthCoroutine != null) StopCoroutine(lengthCoroutine);
            lenght.text = "unknown";
            currentLengthMs = 0f;
        }
    }

    private IEnumerator AnimateNumberRoutine(TextMeshProUGUI label, float startVal, float targetVal, Action<float> onUpdateCurrent)
    {
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / animationDuration);
            float value = Mathf.Lerp(startVal, targetVal, t);

            label.text = Mathf.RoundToInt(value).ToString();
            onUpdateCurrent?.Invoke(value);

            yield return null;
        }

        label.text = Mathf.RoundToInt(targetVal).ToString();
        onUpdateCurrent?.Invoke(targetVal);
    }

    private IEnumerator AnimateTimeRoutine(TextMeshProUGUI label, float startMs, float targetMs, Action<float> onUpdateCurrent)
    {
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / animationDuration);
            float currentMs = Mathf.Lerp(startMs, targetMs, t);

            TimeSpan time = TimeSpan.FromMilliseconds(currentMs);
            label.text = string.Format("{0}:{1:D2}", (int)time.TotalMinutes, time.Seconds);
            onUpdateCurrent?.Invoke(currentMs);

            yield return null;
        }

        TimeSpan finalTime = TimeSpan.FromMilliseconds(targetMs);
        label.text = string.Format("{0}:{1:D2}", (int)finalTime.TotalMinutes, finalTime.Seconds);
        onUpdateCurrent?.Invoke(targetMs);
    }

    private void StopAllAnimationCoroutines()
    {
        if (bpmCoroutine != null) StopCoroutine(bpmCoroutine);
        if (tilesCoroutine != null) StopCoroutine(tilesCoroutine);
        if (lengthCoroutine != null) StopCoroutine(lengthCoroutine);
    }
}