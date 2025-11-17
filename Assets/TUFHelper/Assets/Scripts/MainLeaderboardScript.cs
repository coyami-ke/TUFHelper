using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using TUFHelper.ModScripts.Web;
using UnityEngine;

public class MainLeaderboardScript : MonoBehaviour
{
    public GameObject scrollableParent, prefab, playerListParent;
    public static MainLeaderboardScript instance { get; private set; }

    public void Awake()
    {
        instance = this;
    }

    public async void OnEnable()
    {
        await UpdateList();
    }

    CancellationToken token = new();
    public async Task UpdateList()
    {
        token = new();
        TUFLeaderboardRequest request = new TUFLeaderboardRequest();
        await request.GetAnswerAsync(token);

        MainLeaderboardJson json = JsonConvert.DeserializeObject<MainLeaderboardJson>(request.Answer);

        foreach (Transform child in playerListParent.transform)
            Destroy(child.gameObject);

        int rank = 1;
        foreach (var playerJson in json.Results)
        {
            GameObject obj = Instantiate(prefab, playerListParent.transform);
            RectTransform rect = obj.GetComponent<RectTransform>();

            var script = obj.GetComponent<MainLeaderboardPlayerPrefabScript>();
            script.SetPlayerInfo(playerJson);

            rect.localScale = Vector3.one;
            rect.anchoredPosition = new Vector2(0, (rank - 1) * -105 - 30);
            rank++;
        }

        RectTransform contentRect = playerListParent.GetComponent<RectTransform>();
        float totalHeight = (rank - 1) * 105 + 30;
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);

        Main.Logger.Log(json.Results.Count.ToString());
    }

    public class MainLeaderboardJson
    {
        [JsonProperty("count")]
        public int Count { get; set; }
        [JsonProperty("results")]
        public List<MainLeaderboardPlayerJson> Results { get; set; }
    }
    public class MainLeaderboardPlayerJson
    {
        [JsonProperty("id")]
        public int ID { get; set; }
        [JsonProperty("rankedScore")]
        public float RankedScore { get; set; }
        [JsonProperty("generalScore")]
        public float GeneralScore { get; set; }
        [JsonProperty("averageXacc")]
        public float AverageXAccuracy { get; set; }
        [JsonProperty("player")]
        public PassesListInfoElementPlayerJson Player { get; set; }
        [JsonProperty("generalScoreRank")]
        public int GeneralScoreRank { get; set; }  
    }
}
