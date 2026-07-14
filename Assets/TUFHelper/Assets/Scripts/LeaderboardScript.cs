using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Together.Utils;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using UnityEngine;
using UnityEngine.Networking;

public class LeaderboardScript : MonoBehaviour
{
    public GameObject scrollableParent, prefab, passListParent;
    public List<PassesListInfoElementJson> LastLoadedPasses { get; private set; }

    public RankPrefabScript YourScore;

    public RectTransform rectTransform;

    public static LeaderboardScript instance;

    public float heightWithYourScore, heightWithoutYourScore;
    public float posYWithYourScore, posYWithoutYourScore;

    private CancellationTokenSource currentRequestToken;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        YourScore.gameObject.SetActive(false);
    }

    public static string GetDefaultUrl(int levelID) => $"https://api.tuforums.com/v2/database/passes/level/{levelID}";

    public async void LoadPasses(LevelListInfoElementJson level)
    {
        // Cancel any ongoing request
        currentRequestToken?.Cancel();
        currentRequestToken = new CancellationTokenSource();
        CancellationToken token = currentRequestToken.Token;

        string url = GetDefaultUrl(level.ID);
        string answer = "";
        try
        {
            HttpResponseMessage response = await Main.Client.GetAsync(url, token);

            response.EnsureSuccessStatusCode();

            answer = await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex)
        {
            Debug.LogError($"[TUFAPIRequest] Network HTTP failure at {url}: {ex.Message}");
            throw;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Debug.LogError($"[TUFAPIRequest] Unexpected error: {ex.Message}");
            throw;
        }


        PassesListInfoElementJson[] levelDes = JsonConvert.DeserializeObject<PassesListInfoElementJson[]>(answer);
        if (levelDes == null) return;
        List<PassesListInfoElementJson> passes = levelDes.ToList();
        if (passes == null)
        {
            LastLoadedPasses = new List<PassesListInfoElementJson>();
            return;
        }
        passes = passes.OrderByDescending(p => p.ScoreV2).ToList();

        while (passListParent == null)
        {
            await Task.Yield();
        }


        foreach (Transform child in passListParent.transform)
            Destroy(child.gameObject);

        int rank = 1;
        foreach (var pass in passes)
        {
            GameObject obj = Instantiate(prefab, passListParent.transform);
            BundleFontFixer.FixFontsIn(obj);
            RectTransform rect = obj.GetComponent<RectTransform>();

            var rps = obj.GetComponent<RankPrefabScript>();
            rps.SetPassInfo(pass, level, rank);


            rect.localScale = Vector3.one;
            rect.sizeDelta = new Vector2(0, 60);
            rect.anchoredPosition = new Vector2(0, (rank - 1) * -75 - 30);
            rank++;
        }

        RectTransform contentRect = passListParent.GetComponent<RectTransform>();
        float totalHeight = (rank - 1) * 75 + 30;
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);

        LastLoadedPasses = passes;

        PassesListInfoElementJson yourBestScore = null;

        //if (AccountScript.instance.AccountInfo != null) yourBestScore = passes.FirstOrDefault(e => e.PlayerID == AccountScript.instance.AccountInfo.User.PlayerID);

        if (yourBestScore != null)
        {
            int yourRank = 0;
            for (int i = 0; i < passes.Count; i++)
            {
                if (passes[i].PlayerID == AccountScript.instance.AccountInfo.User.PlayerID)
                {
                    yourRank = i + 1;
                    break;
                }
            }

            YourScore.SetPassInfo(yourBestScore, level, yourRank);
            YourScore.gameObject.SetActive(true);
            rectTransform.sizeDelta = new(rectTransform.sizeDelta.x, heightWithYourScore);
            rectTransform.anchoredPosition = new(rectTransform.anchoredPosition.x, posYWithYourScore);
        }
        else
        {
            YourScore.gameObject.SetActive(false);
            rectTransform.sizeDelta = new(rectTransform.sizeDelta.x, heightWithoutYourScore);
            rectTransform.anchoredPosition = new(rectTransform.anchoredPosition.x, posYWithoutYourScore);
        }
    }
}
