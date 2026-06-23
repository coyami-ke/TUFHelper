using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Together.Utils;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.Networking;

[RegisterIngameElement("IngameLeaderboard", "assets/tufhelper/assets/prefabs/ingameleaderboardprefab.prefab")]
public class IngameLeaderboardScript : BasicIngameElement
{
    public GameObject parentList, prefab;
    public static IngamerankPrefabScript PlayerRankPrefab { get; private set; }
    private List<IngamerankPrefabScript> ranks = new();

    public override string ID => "IngameLeaderboard";

    // Scoring & calculation state cache variables
    private readonly PPDisplayerScript.PassData _cachedPassData = new();
    private PPDisplayerScript.LevelData _cachedLevelData;
    private const float LeaderboardUpdateInterval = 0.12f;
    private float lastLeaderboardUpdateTime = -999f;
    private CancellationTokenSource currentRequestToken;

    protected override bool ShouldElementBeVisible()
    {
        return Main.Setting.ShowIngameLeaderboard;
    }

    #region Self-Contained Gameplay Event Hooks

    protected override async void OnPlay(PlayButtonEventArgs e)
    {
        if (!ADOFAIGameplayHandler.IsFromTUFHelper) return;
        lastLeaderboardUpdateTime = -999f;

        // Fetch web data directly from here
        var passes = await GetPassesAsync(e.CurrentLevelInfo.ID);
        StartCoroutine(LoadLeaderboardAsync(passes));

        if (PlayerRankPrefab?.PassInfo?.Judgements != null)
        {
            PlayerRankPrefab.PassInfo.Judgements = new();
        }

        _cachedLevelData = new PPDisplayerScript.LevelData(e.CurrentLevelInfo);
    }

    protected override void OnHit(HitMargin hit)
    {
        if (PlayerRankPrefab == null) return;

        var player = PlayerRankPrefab.PassInfo;
        if (player.Judgements.Deaths > 0) return;

        UpdateJudgements(player.Judgements, hit);
        var judg = player.Judgements;

        _cachedPassData.IsNoHoldTap = Persistence.holdBehavior == HoldBehavior.NoHoldNeeded;
        _cachedPassData.Speed = scnGame.instance.levelData.pitch / 100f * scnEditor.instance.playbackSpeed;

        var pJudg = _cachedPassData.Judgements;
        pJudg.Perfect = judg.Perfect;
        pJudg.LPerfect = judg.LPerfect;
        pJudg.EPerfect = judg.EPerfect;
        pJudg.EarlySingle = judg.EarlySingle;
        pJudg.LateSingle = judg.LateSingle;
        pJudg.EarlyDouble = judg.EarlyDouble;
        pJudg.LateDouble = judg.LateDouble;
        pJudg.Deaths = judg.Deaths;

        float now = Time.unscaledTime;
        if (now - lastLeaderboardUpdateTime < LeaderboardUpdateInterval) return;
        lastLeaderboardUpdateTime = now;

        player.ScoreV2 = (float)PPDisplayerScript.ScoreCalculator.GetScoreV2(_cachedPassData, _cachedLevelData);
        player.Accuracy = (float)PPDisplayerScript.ScoreCalculator.CalcAcc(judg);

        PlayerRankPrefab.UpdateVisual();
        UpdateRanks();
    }

    #endregion

    #region Leaderboard Internal Logic & Sorting

    public void UpdateRanks()
    {
        if (ranks.Count == 0 || PlayerRankPrefab == null) return;

        if (ranks.Count > 1)
        {
            int currentIndex = ranks.IndexOf(PlayerRankPrefab);
            if (currentIndex != -1)
            {
                int newIndex = currentIndex;
                float myScore = PlayerRankPrefab.PassInfo.ScoreV2;

                while (newIndex > 0 && ranks[newIndex - 1].PassInfo.ScoreV2 < myScore)
                    newIndex--;

                while (newIndex < ranks.Count - 1 && ranks[newIndex + 1].PassInfo.ScoreV2 > myScore)
                    newIndex++;

                if (newIndex != currentIndex)
                {
                    var item = ranks[currentIndex];
                    ranks.RemoveAt(currentIndex);
                    ranks.Insert(newIndex, item);
                }
            }
        }

        int myIdx = ranks.IndexOf(PlayerRankPrefab);
        int startIndex = Mathf.Max(0, myIdx - 2);
        int endIndex = Mathf.Min(ranks.Count, startIndex + 5);

        for (int i = 0; i < ranks.Count; i++)
        {
            var rankItem = ranks[i];
            bool shouldBeVisible = i >= startIndex && i < endIndex;

            rankItem.Rank = i + 1;

            if (rankItem.gameObject.activeSelf != shouldBeVisible)
                rankItem.gameObject.SetActive(shouldBeVisible);

            if (shouldBeVisible)
            {
                rankItem.SetPosition(i - startIndex);
                rankItem.UpdateVisual();
            }
        }
    }

    public IEnumerator LoadLeaderboardAsync(PassesListInfoElementJson[] passes)
    {
        PassesListInfoElementJson[] safePasses = passes ?? Array.Empty<PassesListInfoElementJson>();
        ranks.Clear();
        PlayerRankPrefab = null;

        for (int i = parentList.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(parentList.transform.GetChild(i).gameObject);
        }

        yield return new WaitForEndOfFrame();

        List<PassesListInfoElementJson> passList = safePasses.ToList();
        bool hasYou = passList.Any(p => p?.Player != null && p.Player.Name == "YOU");

        if (!hasYou)
        {
            passList.Add(new PassesListInfoElementJson
            {
                Player = new() { Name = "YOU" },
                Accuracy = 0f,
                Judgements = new(),
                ScoreV2 = 0
            });
        }

        foreach (var pass in passList)
        {
            if (pass == null || pass.Player == null) continue;

            GameObject obj = Instantiate(prefab, parentList.transform);
            BundleFontFixer.FixFontsIn(obj);
            var script = obj.GetComponent<IngamerankPrefabScript>();

            obj.SetActive(false);
            script.LoadPass(pass, 0);

            if (pass.Player.Name == "YOU")
            {
                PlayerRankPrefab = script;
            }

            ranks.Add(script);
            if (ranks.Count % 50 == 0) yield return null;
        }

        UpdateRanks();
    }

    #endregion

    #region Calculations & Web Helpers

    private void UpdateJudgements(PassesListInfoElementJudgementsJson judgements, HitMargin hit)
    {
        switch (hit)
        {
            case HitMargin.TooEarly: judgements.EarlyDouble++; break;
            case HitMargin.VeryEarly: judgements.EarlySingle++; break;
            case HitMargin.EarlyPerfect: judgements.EPerfect++; break;
            case HitMargin.Perfect: judgements.Perfect++; break;
            case HitMargin.LatePerfect: judgements.LPerfect++; break;
            case HitMargin.VeryLate: judgements.LateSingle++; break;
            case HitMargin.TooLate: judgements.LateDouble++; break;
            case HitMargin.FailMiss:
            case HitMargin.FailOverload: judgements.Deaths++; break;
        }
    }

    private async Task<PassesListInfoElementJson[]> GetPassesAsync(int levelID)
    {
        currentRequestToken?.Cancel();
        currentRequestToken = new CancellationTokenSource();
        CancellationToken token = currentRequestToken.Token;

        string url = LeaderboardScript.GetDefaultUrl(levelID);
        using UnityWebRequest webRequest = UnityWebRequest.Get(url);
        webRequest.certificateHandler = new CertificateWhore();
        webRequest.timeout = 10;

        var operation = webRequest.SendWebRequest();
        while (!operation.isDone)
        {
            await Task.Yield();
            if (token.IsCancellationRequested)
            {
                webRequest.Abort();
                return null;
            }
        }

        if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
            return null;

        PassesListInfoElementJson[] levelDes = JsonConvert.DeserializeObject<PassesListInfoElementJson[]>(webRequest.downloadHandler.text);
        if (levelDes == null) return Array.Empty<PassesListInfoElementJson>();
        return levelDes.OrderByDescending(p => p.ScoreV2).ToArray();
    }

    #endregion

    protected override void OnDestroy()
    {
        currentRequestToken?.Cancel();
        base.OnDestroy();
    }
}