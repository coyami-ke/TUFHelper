using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TUFHelper;
using UnityEngine;
using System.Linq;
using TUFHelper.Utils;
using UnityEngine.Networking;
using Together.Utils;
using TUFHelper.ModScripts.Json;
using UnityEditor.PackageManager.Requests;
using System.Net;

public class LevelListScript : MonoBehaviour
{
    public const string TUF_DATABASE_URL = "https://api.tuforums.com/v2/database";

    public static LevelListScript instance;

    public static int availableLevels = 0;
    public static List<LevelListInfoElementJson> Levels = new List<LevelListInfoElementJson>();

    public GameObject levelPrefab, levelListParent, loadingText;

    public void Awake()
    {
        Main.Logger.Log("LevelListScript: Awake");

        instance = this;


        StartCoroutine(RequestAllPlayers());

        UpdateLevelList("");
    }

    public void UpdateLevelList(string name)
    {
        StartCoroutine(RequestAllLevels(name));
        LoadLevelListCo();
        // if (Levels.Count == 0)
        // {
        //     StartCoroutine(RequestAllLevels(""));
        // }
        // else
        // {
        //     SortLevelList();
        //     StartCoroutine(LoadLevelListCo());
        // }
    }

    public string GetDefaultRequestString(string name)
    {
        return $"https://api.tuforums.com/v2/database/levels/?limit=30&offset=0&query={name}&sort=RECENT_DESC&deletedFilter=hide&clearedFilter=show";
    }
    public IEnumerator RequestAllLevels(string name)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(GetDefaultRequestString(name)); //UnityWebRequest.Get("https://be.tuforums.com/levels");
        webRequest.certificateHandler = new CertificateWhore();

        yield return webRequest.SendWebRequest();

        Levels.Clear();

        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Main.Logger.Error("Levels Request Error: " + webRequest.error);
        }
        else
        {
            LevelListInfoJson info = LevelListInfoJson.Deserialize(webRequest.downloadHandler.text);
            foreach (var element in info.Results)
            {
                Levels.Add(element);
            }
            StartCoroutine(LoadLevelListCo());
            Main.Logger.Log("Loaded levels: " + Levels.Count);
        }
    }

    public IEnumerator RequestAllPlayers()
    {
        UnityWebRequest www = UnityWebRequest.Get("https://be.tuforums.com/players");
        www.certificateHandler = new CertificateWhore();

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Players Request Error: " + www.error);
        }
        else
        {
            JObject jo = JsonConvert.DeserializeObject<JObject>(www.downloadHandler.text);
            JArray ja = jo.Value<JArray>("results");

            foreach (PlayerInfo pi in JsonConvert.DeserializeObject<List<PlayerInfo>>(ja.ToString()))
            {
                if (Main.playerData.ContainsKey(pi.name))
                {
                    continue;
                }
                Main.playerData.Add(pi.name, pi);
            }


            StartCoroutine(RequestAllPasses());
        }
    }


    public IEnumerator RequestAllPasses()
    {
        UnityWebRequest www = UnityWebRequest.Get("https://be.tuforums.com/passes");
        www.certificateHandler = new CertificateWhore();

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Passes Request Error: " + www.error);
        }
        else
        {
            JObject jo = JsonConvert.DeserializeObject<JObject>(www.downloadHandler.text);
            JArray ja = jo.Value<JArray>("results");

            Main.passesData.Clear();
            foreach (PassInfo pi in JsonConvert.DeserializeObject<List<PassInfo>>(ja.ToString()))
            {
                if (pi.levelId == null)
                {
                    continue;
                }
                if (!Main.passesData.ContainsKey(pi.GetLevelId()))
                {
                    Main.passesData.Add(pi.GetLevelId(), new List<PassInfo>());
                }

                if (!Main.playerData.ContainsKey(pi.player))
                {
                    continue;
                }

                PlayerInfo playerInfo = Main.playerData[pi.player];
                if (playerInfo.isBanned)
                {
                    continue;
                }

                List<PassInfo> list = Main.passesData[pi.GetLevelId()];
                list.Add(pi);
                list = list.OrderByDescending(x => x.getXAcc()).ToList();
                Main.passesData[pi.GetLevelId()] = list;
            }

            foreach (var li in Levels)
            {
                if (Main.passesData.ContainsKey(li.ID))
                {
                    li.Clears = Main.passesData[li.ID].Count;
                }
            }

            SortLevelList();
            StartCoroutine(LoadLevelListCo());
        }
    }

    public IEnumerator LoadLevelListCo()
    {
        if (Main.passesData.Count == 0 || Levels.Count == 0)
        {
            yield break;
        }
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

        List<LevelListInfoElementJson> list = new List<LevelListInfoElementJson>();
        foreach (var li in Levels)
        {
            if (SearchScript.isShowingFavourites && Main.Setting.favouritesID.Contains(li.ID)) list.Add(li);
            else
            {
                if (search.StartsWith("#"))
                {
                    if (("#" + li.ID).Equals(search))
                    {
                        list.Add(li);
                    }
                }
                else
                {
                    if (li.Artist.ToLower().Contains(search.ToLower()) ||
                        li.Creator.ToLower().Contains(search.ToLower()) ||
                        li.Song.ToLower().Contains(search.ToLower()))
                    {
                        if (Helper.newDiffToSortNumber(li.DiffId) >= Helper.newDiffToSortNumber(Helper.pguDiffToNewDiff(minPguDiff)) &&
                            Helper.newDiffToSortNumber(li.DiffId) <= Helper.newDiffToSortNumber(Helper.pguDiffToNewDiff(maxPguDiff)))
                        {
                            if (Main.Setting.showUnratedLevels || (li.DiffId != 0 && li.DiffId != -2))
                            {
                                if (Main.passesData.ContainsKey(li.DiffId))
                                {
                                    List<PassInfo> passes = Main.passesData[li.DiffId];
                                    int totalClears = passes.Count;

                                    if (totalClears > 0)
                                    {
                                        if (!SearchScript.isShowingCleared)
                                        {
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        if (!SearchScript.isShowingUncleared)
                                        {
                                            continue;
                                        }
                                    }
                                }
                                else
                                {
                                    if (!SearchScript.isShowingUncleared)
                                    {
                                        continue;
                                    }
                                }

                                list.Add(li);
                            }
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

            if (Main.passesData.ContainsKey(list[i].ID))
            {
                List<PassInfo> passes = Main.passesData[list[i].ID];
                double bestAcc = passes.Count == 0 ? 0 : passes[0].getXAcc() * 100;
                int totalClears = passes.Count;

                LevelPrefabScript lps = level.GetComponent<LevelPrefabScript>();
                lps.SetLevelInfo(list[i], bestAcc, totalClears);
            }
            else
            {

                LevelPrefabScript lps = level.GetComponent<LevelPrefabScript>();
                lps.SetLevelInfo(list[i], 0, 0);
            }

            cnt++;
        }
    }

    public void SortLevelList()
    {
        if (Main.Setting.orderMode == 0)
        {
            if (Main.Setting.orderByIDMode == 1)
            {
                Levels = Levels.OrderByDescending(level => level.ID).ToList();
            }
            else
            {
                Levels = Levels.OrderBy(level => level.ID).ToList();
            }
        }
        else if (Main.Setting.orderMode == 1)
        {
            if (Main.Setting.orderByDifficultyMode == 1)
            {
                Levels = Levels.OrderByDescending(level => Helper.newDiffToSortNumber(level.DiffId)).ToList();
            }
            else
            {
                Levels = Levels.OrderBy(level => Helper.newDiffToSortNumber(level.DiffId)).ToList();
            }
        }
        if (Main.Setting.orderMode == 2)
        {
            if (Main.Setting.orderByClearsMode == 1)
            {
                Levels = Levels.OrderByDescending(level => level.Clears).ToList();
            }
            else
            {
                Levels = Levels.OrderBy(level => level.Clears).ToList();
            }
        }
    }

}
