using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public void UpdateRanks()
    {
        ranks = ranks.OrderByDescending(e => e.PassInfo.ScoreV2).ToList();

        for (int i = 0; i < ranks.Count; i++)
            ranks[i].Rank = i + 1;

        int youIndex = ranks.FindIndex(e => e.PassInfo.Player.Name == "YOU");
        if (youIndex == -1) return;

        int startIndex = Mathf.Max(0, youIndex - 4);
        int endIndex = Mathf.Min(ranks.Count, startIndex + 5);

        for (int i = 0; i < ranks.Count; i++)
        {
            bool isVisible = i >= startIndex && i < endIndex;
            ranks[i].gameObject.SetActive(isVisible);

            if (isVisible)
            {
                ranks[i].Rank = i + 1; // correct leaderboard rank
                ranks[i].SetPosition(i - startIndex); // visual index from 0 to 4
            }
        }
    }


    public IEnumerator LoadLeaderboardAsync(PassesListInfoElementJson[] passes)
    {
        ranks.Clear();

        foreach (Transform child in parentList.transform)
            Destroy(child.gameObject);

        List<PassesListInfoElementJson> passList = passes.ToList();
        bool hasYou = passList.Any(p => p.Player.Name == "YOU");

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

        int processed = 0;
        foreach (var pass in passList)
        {
            GameObject obj = Instantiate(prefab, parentList.transform);
            var script = obj.GetComponent<IngamerankPrefabScript>();
            script.LoadPass(pass, 0);
            script.gameObject.SetActive(false); // Prevent flicker

            if (pass.Player.Name == "YOU")
                PlayerRankPrefab = script;

            ranks.Add(script);

            processed++;

            // Yield every 20 to avoid freezing the frame
            if (processed % 20 == 0)
                yield return null;
        }

        UpdateRanks();
    }
}
