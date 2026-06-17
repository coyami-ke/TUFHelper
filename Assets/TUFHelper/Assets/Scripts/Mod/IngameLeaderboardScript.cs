using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using UnityEngine;

public class IngameLeaderboardScript : MonoBehaviour
{
    public GameObject parentList, prefab;

    public static IngameLeaderboardScript instance { get; private set; }
    public static IngamerankPrefabScript PlayerRankPrefab { get; private set; }

    private List<IngamerankPrefabScript> ranks = new();

    public void Awake()
    {
        instance = this;
    }

    private float lastUpdateTime = 0f;
    private const float UpdateInterval = 0.05f;

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && scnEditor.instance.playMode)
        {
            gameObject.SetActive(false);
            return;
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

        bool hasYou = false;
        foreach (var p in passList)
        {
            if (p?.Player != null && p.Player.Name == "YOU")
            {
                hasYou = true;
                break;
            }
        }

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
}
