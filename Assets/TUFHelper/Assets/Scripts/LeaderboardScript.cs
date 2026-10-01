using DG.Tweening;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using UnityEngine;

public class LeaderboardScript : MonoBehaviour
{
    public GameObject scrollableParent, prefab, passListParent;
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

    public static string GetDefaultUrl(int levelID) => $"https://api.tuforums.com/v2/database/passes/level/{levelID}";

    public async void LoadPasses(LevelListInfoElementJson level)
    {
        currentRequestToken?.Cancel();
        currentRequestToken = new CancellationTokenSource();
        CancellationToken token = currentRequestToken.Token;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        string url = GetDefaultUrl(level.ID);
        string answer = "";

        try
        {
            HttpResponseMessage response = await Main.Client.GetAsync(url, token);
            response.EnsureSuccessStatusCode();

            token.ThrowIfCancellationRequested();
            answer = await response.Content.ReadAsStringAsync();
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[TUFAPIRequest] Network or HTTP failure at {url}: {ex.Message}");
            return;
        }

        token.ThrowIfCancellationRequested();

        List<PassesListInfoElementJson> passes = await Task.Run(() =>
        {
            var levelDes = JsonConvert.DeserializeObject<PassesListInfoElementJson[]>(answer);
            if (levelDes == null) return new List<PassesListInfoElementJson>();

            return levelDes.OrderByDescending(p => p.ScoreV2).ToList();
        }, token);

        if (token.IsCancellationRequested) return;

        LastLoadedPasses = passes;

        ClearPassList();

        if (passes.Count == 0) return;

        if (passListParent != null)
        {
            RectTransform contentRect = passListParent.GetComponent<RectTransform>();
            if (contentRect != null)
            {
                float totalHeight = (passes.Count * itemSpacing) + topPadding;
                contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);
            }
        }

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

        foreach (Transform child in passListParent.transform)
        {
            child.DOKill(true);
            Destroy(child.gameObject);
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