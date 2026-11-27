using DG.Tweening;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using UnityEngine;

public class LeaderboardScript : MonoBehaviour
{
    public GameObject scrollableParent, prefab, passListParent, notFoundClearsText, loadingIcon;
    public List<PassesListInfoElementJson> LastLoadedPasses { get; private set; }

    public RankPrefabScript YourScore;
    public RectTransform rectTransform;

    public static LeaderboardScript instance;

    public float heightWithYourScore, heightWithoutYourScore;
    public float posYWithYourScore, posYWithoutYourScore;

    [Header("Batch Spawning Settings")]
    [SerializeField] private int itemsPerFrame = 1;
    [SerializeField] private float itemSpacing = 65f;
    [SerializeField] private float topPadding = 30f;

    private CancellationTokenSource currentRequestToken;
    private Coroutine spawnCoroutine;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        notFoundClearsText.SetActive(false);
    }

    public static string GetDefaultUrl(int levelID) => $"https://api.tuforums.com/v2/database/passes/level/{levelID}";

    public async void LoadPasses(LevelListInfoElementJson level)
    {
        //notFoundClearsText.SetActive(false);
        UITransition.Hide(notFoundClearsText);
        UITransition.Show(loadingIcon);

        ClearPassList();
        currentRequestToken?.Cancel();
        currentRequestToken = new CancellationTokenSource();
        CancellationToken token = currentRequestToken.Token;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        string url = GetDefaultUrl(level.ID);
        Stream stream;

        try
        {
            HttpResponseMessage response = await Main.Client.GetAsync(url, token);
            response.EnsureSuccessStatusCode();

            token.ThrowIfCancellationRequested();
            stream = await response.Content.ReadAsStreamAsync(); //answer = await response.Content.ReadAsStringAsync();
        }
        catch (OperationCanceledException)
        {
            UITransition.Hide(loadingIcon);
            return;
        }
        catch (Exception ex)
        {
            UITransition.Hide(loadingIcon);
            Main.Logger.Error($"[TUFAPIRequest] Network or HTTP failure at {url}: {ex.Message}");
            return;
        }

        token.ThrowIfCancellationRequested();

        Stopwatch stopwatch1 = Stopwatch.StartNew();
        List<PassesListInfoElementJson> passes = new();
        try
        {
            passes = (await Utf8Json.JsonSerializer.DeserializeAsync<List<PassesListInfoElementJson>>(stream)).OrderByDescending(p => p.scoreV2).ToList();
        }
        catch (Exception ex)
        {
            Main.Logger.LogException(ex);
        }
        stopwatch1.Stop();

        Main.Logger.Log(stopwatch1.ElapsedMilliseconds.ToString());

        passes = passes.GetRange(0, Math.Min(passes.Count, 100));

        if (token.IsCancellationRequested) return;

        LastLoadedPasses = passes;

        if (passes.Count == 0) UITransition.Show(notFoundClearsText);
        else UITransition.Hide(notFoundClearsText);

        if (passes.Count == 0)
        {
            UITransition.Hide(loadingIcon);
            return;
        }

        RectTransform contentRect = passListParent.GetComponent<RectTransform>();
        if (contentRect != null)
        {
            float totalHeight = (passes.Count * itemSpacing) + topPadding;
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);
        }

        UITransition.Hide(loadingIcon);

        spawnCoroutine = StartCoroutine(BatchSpawnPassesRoutine(passes, level, token));
    }

    private System.Collections.IEnumerator BatchSpawnPassesRoutine(List<PassesListInfoElementJson> passes, LevelListInfoElementJson level, CancellationToken token)
    {
        int total = passes.Count;
        int spawned = 0;

        while (spawned < total)
        {
            if (token.IsCancellationRequested) yield break;

            int batchEnd = Mathf.Min(spawned + itemsPerFrame, total);

            for (int i = spawned; i < batchEnd; i++)
            {
                if (passListParent == null) yield break;

                GameObject obj = Instantiate(prefab, passListParent.transform);
                RectTransform rect = obj.GetComponent<RectTransform>();

                int rank = i + 1;
                var rps = obj.GetComponent<RankPrefabScript>();
                rps.Init(scrollableParent, rectTransform);
                rps.SetPassInfo(passes[i], level, rank);

                rect.anchoredPosition = new Vector2(0, (i * -itemSpacing) - topPadding);
            }

            spawned = batchEnd;

            yield return null;
        }

        spawnCoroutine = null;
    }

    private void ClearPassList()
    {
        if (passListParent == null) return;

        Transform[] children = new Transform[passListParent.transform.childCount];
        for (int i = 0; i < passListParent.transform.childCount; i++)
        {
            children[i] = passListParent.transform.GetChild(i);
        }

        foreach (Transform child in children)
        {
            if (child == null) continue;

            //var canvasGroup = child.GetComponent<CanvasGroup>();
            //if (canvasGroup == null) canvasGroup = child.gameObject.AddComponent<CanvasGroup>();
            //canvasGroup.blocksRaycasts = false;

            UITransition.AnimateLeaderboardItemOut(child, () =>
            {
                if (child != null && child.gameObject != null)
                {
                    child.DOKill(true);
                    Destroy(child.gameObject);
                }
            });
        }
    }

    private void OnDisable()
    {
        currentRequestToken?.Cancel();
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
    }

    private void OnDestroy()
    {
        currentRequestToken?.Cancel();
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
    }
}