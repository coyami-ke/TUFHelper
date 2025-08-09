using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using TMPro;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using TUFHelper.ModScripts.Web;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.UI;

public class RatingPanel : MonoBehaviour
{
    public GameObject scrollableParent, prefab, listParent;

    private bool isHideRatedValue;
    private Rating4Plus rating4PlusValue;


    public TMP_Dropdown rating4Plus;
    public Toggle hideRated;

    public RatingElementJson[] RatingElements { get; private set; }

    public static RatingPanel instance { get; private set; }

    public void Awake()
    {
        instance = this;

        hideRated.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>(OnHideRatedChanged));
        rating4Plus.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<int>(On4PlusRatedChanged));
    }

    private CancellationToken cancellationToken;
    public async void UpdateList()
    {
        if (cancellationToken == null) cancellationToken = new();

        var request = new TUFRatingRequest();
        await request.GetAnswerAsync(cancellationToken);

        List<RatingElementJson> elements = JsonConvert.DeserializeObject<List<RatingElementJson>>(request.Answer);

        List<RatingElementJson> filtered = new();

        foreach (var element in elements.ToArray())
        {
            bool passesRatingCountFilter = rating4PlusValue switch
            {
                Rating4Plus.Show => true,
                Rating4Plus.Hide => element.Details.Count < 4,
                Rating4Plus.Only => element.Details.Count >= 4,
                _ => false
            };

            var yourRating = element.Details.FirstOrDefault(e => e.User.Username == AccountScript.instance.AccountInfo.User.Username);
            
            bool passesRatedFilter = !isHideRatedValue || yourRating != null;

            if (passesRatingCountFilter && passesRatedFilter)
                filtered.Add(element);
        }


        foreach (Transform child in listParent.transform)
            Destroy(child.gameObject);

        int i = 0;
        float height = 0;
        foreach (var element in filtered)
        {
            GameObject obj = Instantiate(prefab, listParent.transform);
            RectTransform rect = obj.GetComponent<RectTransform>();

            rect.localScale = Vector3.one;

            var rps = obj.GetComponent<RatePrefabScript>();
            rps.SetRateInfo(element);

            rect.anchoredPosition = new Vector2(0, -height); //new Vector2(0, i * -(5 + rps.rectTransform.sizeDelta.y) - 30);
            height += rps.rectTransform.sizeDelta.y + 5;

            i++;
        }

        RectTransform contentRect = listParent.GetComponent<RectTransform>();
        float totalHeight = height;
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);

        RatingElements = elements.ToArray();

        Main.Logger.Log("Rating Elements: " + elements.Count);
    }

    public void On4PlusRatedChanged(int value)
    {
        switch (value)
        {
            case 0: rating4PlusValue = Rating4Plus.Hide; break;
            case 1: rating4PlusValue = Rating4Plus.Show; break;
            case 2: rating4PlusValue = Rating4Plus.Only; break;
        }
        Main.Logger.Log(value.ToString());
        UpdateList();
    }
    public void OnHideRatedChanged(bool value)
    {
        isHideRatedValue = value;
        Main.Logger.Log(value.ToString());
        UpdateList();
    }
}

namespace TUFHelper.Utils
{
    public enum Rating4Plus
    {
        Hide,
        Show,
        Only,
    }
    public class RatingElementJson
    {
        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("levelId")]
        public int LevelID { get; set; }

        [JsonProperty("currentDifficultyId")]
        public int? CurrentDifficultyID { get; set; }

        [JsonProperty("lowDiff")]
        public bool LowDiff { get; set; }

        [JsonProperty("requesterFR")]
        public string RequesterFR { get; set; }

        [JsonProperty("averageDifficultyId")]
        public int? AverageDifficultyID { get; set; }

        [JsonProperty("communityDifficultyId")]
        public int? CommunityDifficultyID { get; set; }

        [JsonProperty("level")]
        public LevelListInfoElementJson Level { get; set; }

        [JsonProperty("details")]
        public List<DifficultyDetail> Details { get; set; }

        [JsonProperty("currentDifficulty")]
        public DifficultyInfo CurrentDifficulty { get; set; }

        [JsonProperty("averageDifficulty")]
        public DifficultyInfo AverageDifficulty { get; set; }

        [JsonProperty("communityDifficulty")]
        public DifficultyInfo CommunityDifficulty { get; set; }

        [JsonProperty("requestedDiffId")]
        public int? RequestedDiffID { get; set; }
    }
    public class DifficultyDetail
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("ratingId")]
        public int RatingId { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("rating")]
        public string Rating { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("isCommunityRating")]
        public bool IsCommunityRating { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("user")]
        public UserInfo User { get; set; }
    }

    public class UserInfo
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }
        [JsonProperty("avatarUrl")]
        public string AvatarUrl { get; set; }
    }
    public class DifficultyInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("emoji")]
        public string Emoji { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("baseScore")]
        public float BaseScore { get; set; }

        [JsonProperty("sortOrder")]
        public int SortOrder { get; set; }

        [JsonProperty("legacy")]
        public string Legacy { get; set; }

        [JsonProperty("legacyIcon")]
        public string LegacyIcon { get; set; }

        [JsonProperty("legacyEmoji")]
        public string LegacyEmoji { get; set; }
    }
}