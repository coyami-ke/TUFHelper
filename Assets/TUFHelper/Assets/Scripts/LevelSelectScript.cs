using DG.Tweening;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TUFHelper;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectScript : MonoBehaviour
{

    public static LevelSelectScript instance;
    public static List<LevelInfo> Levels = new List<LevelInfo>();
    public static int availableLevels = 0;

    public GameObject levelPrefab, levelListParent, loadingText;
    public TextMeshProUGUI orderByIDButtonText, orderByDifficultyButtonText;
    public TMP_InputField searchField;
    public Image orderByIDIcon, orderByDifficultyIcon;

    private int orderMode = 0; // 0 -> id, 1 -> difficulty
    private int orderByIDMode = -1; // 1 -> down, -1 -> up
    private int orderByDifficultyMode = 1; // 1 -> down, -1 -> up

    public void Awake()
    {
        DOTween.KillAll();

        instance = this;

        StartCoroutine(RequestAllLevels());
    }

    public void Update()
    {
        orderByIDButtonText.color = orderMode == 0 ? Color.white : new Color(1, 1, 1, 128 / 256f);
        orderByDifficultyButtonText.color = orderMode == 1 ? Color.white : new Color(1, 1, 1, 128 / 256f);
        orderByIDIcon.transform.localScale = new Vector2(1, orderByIDMode);
        orderByDifficultyIcon.transform.localScale = new Vector2(1, orderByDifficultyMode);
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
                if (!li.pgu_diff.Equals("0") && !li.pgu_diff.Equals("-2"))
                {
                    Levels.Add(li);
                }
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
        int page = PageSwitcherScript.instance.currentPage;

        if (page == 0)
        {
            yield break;
        }

        string search = searchField.text;
        List<LevelInfo> list = new List<LevelInfo>();
        foreach (LevelInfo li in Levels)
        {
            if (li.artist.ToLower().Contains(search.ToLower()) ||
                li.creator.ToLower().Contains(search.ToLower()) ||
                li.song.ToLower().Contains(search.ToLower()) ||
                (li.id + "").Contains(search.ToLower()))
            {
                list.Add(li);
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

    public void OnSearchTextChange()
    {
        PageSwitcherScript.instance.currentPage = 1;
        StartCoroutine(LoadLevelListCo());
    }

    public void ExitButtonClick()
    {
        UIScript.SwipeToBlack(() =>
        {
            Main.isInTUFHelper = false;
            GCS.sceneToLoad = "";
            SceneManager.LoadScene("scnLevelSelect");
        });
    }

    public void DiscordButtonClick()
    {
        Application.OpenURL("https://discord.gg/8FBDmAPrKe");
    }

    public void OrderByIDButtonClick()
    {
        if (orderMode == 0)
        {
            orderByIDMode = -orderByIDMode;
        }
        orderMode = 0;

        SortLevelList();
        StartCoroutine(LoadLevelListCo());
    }

    public void OrderByDifficultyButtonClick()
    {
        if (orderMode == 1)
        {
            orderByDifficultyMode = -orderByDifficultyMode;
        }
        orderMode = 1;

        SortLevelList();
        StartCoroutine(LoadLevelListCo());
    }

    public void SortLevelList()
    {
        if (orderMode == 0)
        {
            if (orderByIDMode == 1)
            {
                Levels = Levels.OrderByDescending(level => level.id).ToList();
            }
            else
            {
                Levels = Levels.OrderBy(level => level.id).ToList();
            }
        }
        else if (orderMode == 1)
        {
            if (orderByDifficultyMode == 1)
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
