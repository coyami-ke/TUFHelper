using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TUFHelper;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public class LevelListScript : MonoBehaviour
{

    public static LevelListScript instance;

    public static int availableLevels = 0;
    public static List<LevelInfo> Levels = new List<LevelInfo>();

    public GameObject levelPrefab, levelListParent, loadingText;

    public void Awake()
    {
        instance = this;

        if (Levels.Count == 0)
        {
            StartCoroutine(RequestAllLevels());
        }
        else
        {
            SortLevelList();
            StartCoroutine(LoadLevelListCo());
        }
    }

    public void Update()
    {

    }

    public IEnumerator RequestAllLevels()
    {
        UnityWebRequest www = UnityWebRequest.Get("https://be.t21c.kro.kr/levels");

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Levels Request Error: " + www.error);
        }
        else
        {
            JObject jo = JsonConvert.DeserializeObject<JObject>(www.downloadHandler.text);
            JArray ja = jo.Value<JArray>("results");

            Levels.Clear();
            foreach (LevelInfo li in JsonConvert.DeserializeObject<List<LevelInfo>>(ja.ToString()))
            {
                Levels.Add(li);
            }

            SortLevelList();
            StartCoroutine(LoadLevelListCo());
        }
    }

    public IEnumerator LoadLevelListCo()
    {
        yield return new WaitForEndOfFrame();

        loadingText.SetActive(false);

        for (int i = 0; i < levelListParent.transform.childCount; i++)
        {
            Destroy(levelListParent.transform.GetChild(i).gameObject);
        }

        int cnt = 0;
        int page = PageSwitcherScript.currentPage;

        if (page == 0)
        {
            yield break;
        }

        string search = SearchScript.searchText;
        string minPguDiff = SettingsScript.instance.minDiffDropdown.options[SettingsScript.instance.minDiffDropdown.value].text;
        string maxPguDiff = SettingsScript.instance.maxDiffDropdown.options[SettingsScript.instance.maxDiffDropdown.value].text;

        List<LevelInfo> list = new List<LevelInfo>();
        foreach (LevelInfo li in Levels)
        {
            bool isIdSearch = search.StartsWith("#");
            if (isIdSearch)
            {
                if (("#" + li.id).Equals(search))
                {
                    list.Add(li);
                }
            }
            else
            {
                if (li.artist.ToLower().Contains(search.ToLower()) ||
                    li.creator.ToLower().Contains(search.ToLower()) ||
                    li.song.ToLower().Contains(search.ToLower()))
                {
                    if (Helper.pguDiffToSortNumber(li.pgu_diff) >= Helper.pguDiffToSortNumber(minPguDiff) &&
                        Helper.pguDiffToSortNumber(li.pgu_diff) <= Helper.pguDiffToSortNumber(maxPguDiff))
                    {
                        if (Main.Setting.showUnratedLevels || (!li.pgu_diff.Equals("0") && !li.pgu_diff.Equals("-2")))
                        {
                            list.Add(li);
                        }
                    }
                }
            }
        }

        availableLevels = list.Count;

        int startIndex = (page - 1) * 6;
        int endIndex = Math.Min(page * 6, list.Count);

        for (int i = startIndex; i < endIndex; i++)
        {
            GameObject level = Instantiate(levelPrefab);

            RectTransform rect = level.GetComponent<RectTransform>();
            rect.SetParent(levelListParent.transform);
            rect.localScale = Vector3.one;
            rect.offsetMin = new Vector2(0, 0);
            rect.offsetMax = new Vector2(0, 0);
            rect.sizeDelta = new Vector2(0, 120);
            rect.anchoredPosition = new Vector3(0, (cnt * -140) - 90);

            LevelPrefabScript lps = level.GetComponent<LevelPrefabScript>();
            lps.SetLevelInfo(list[i]);

            cnt++;
        }
    }

    public void SortLevelList()
    {
        if (Main.Setting.orderMode == 0)
        {
            if (Main.Setting.orderByIDMode == 1)
            {
                Levels = Levels.OrderByDescending(level => level.id).ToList();
            }
            else
            {
                Levels = Levels.OrderBy(level => level.id).ToList();
            }
        }
        else if (Main.Setting.orderMode == 1)
        {
            if (Main.Setting.orderByDifficultyMode == 1)
            {
                Levels = Levels.OrderByDescending(level => Helper.pguDiffToSortNumber(level.pgu_diff)).ToList();
            }
            else
            {
                Levels = Levels.OrderBy(level => Helper.pguDiffToSortNumber(level.pgu_diff)).ToList();
            }
        }
    }

}
