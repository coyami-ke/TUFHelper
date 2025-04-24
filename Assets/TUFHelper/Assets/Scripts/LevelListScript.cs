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
using System.Text;
using SA.GoogleDoc;
using System.Linq.Expressions;
using UnityEngine.UI;

public class LevelListScript : MonoBehaviour
{

    public const int REQUEST_LIMIT = 60;
    public static LevelListScript instance;

    public static int availablelevelPrefabScripts = 0;
    public static List<LevelListInfoElementJson> levelPrefabScripts = new List<LevelListInfoElementJson>();

    public GameObject levelPrefab, levelListParent, loadingText, verticalScroll;

    public ScrollRect VerticalScrollComponent { get; private set; } 

    public void Awake()
    {
        Main.Logger.Log("LevelListScript: Awake");

        instance = this;
        
        UpdateLevelList("");

        VerticalScrollComponent = verticalScroll.GetComponent<ScrollRect>();
    }
    private bool isLoading = false;
    public void Update()
    {
        var levelPrefabScripts = GetLevelPrefabScripts();

        // Handle Up Arrow Key
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (GetIndexSelected() == -1 && levelPrefabScripts.Length > 0)
            {
                levelPrefabScripts[0].IsSelected = true;
            }

            int index = GetIndexSelected();
            if (index > 0)
            {
                foreach (var level in levelPrefabScripts)
                {
                    level.IsSelected = false;
                }

                levelPrefabScripts[index - 1].IsSelected = true;
                LeaderboardScript.instance.LoadPasses(levelPrefabScripts[index - 1].levelInfo.ID);
            }
        }

        // Handle Down Arrow Key
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (GetIndexSelected() == -1 && levelPrefabScripts.Length > 0)
            {
                levelPrefabScripts[0].IsSelected = true;
            }

            int index = GetIndexSelected();
            if (index < levelPrefabScripts.Length - 1)
            {
                foreach (var level in levelPrefabScripts)
                {
                    level.IsSelected = false;
                }

                levelPrefabScripts[index + 1].IsSelected = true;
                LeaderboardScript.instance.LoadPasses(levelPrefabScripts[index + 1].levelInfo.ID);
            }
        }

        // Handle Scroll-Based Pagination
        if (!isLoading && VerticalScrollComponent.verticalNormalizedPosition <= 0.01f)
        {
            Main.Logger.Log("Scrolled to bottom — loading more levels");
            isLoading = true;
            UpdateLevelList(SearchScript.instance.searchField.text, LevelListScript.levelPrefabScripts.Count);
        }
    }


    public void UpdateLevelList(string name, int offset = 0)
    {
        if (offset == 0) 
            StartCoroutine(RequestNewLevels(name, 0, true));
        else
            StartCoroutine(RequestNewLevels(name, offset, false));
    }

    public string GetDefaultRequestString(string name, int offset)
    {
        if (name.StartsWith('#')) 
        {
            string newName = name.Substring(1);
            return $"https://api.tuforums.com/v2/database/levels/byId/{newName}";
        }
        else 
        {
            return $"https://api.tuforums.com/v2/database/levels/filter?query={name}&limit={REQUEST_LIMIT}&offset={offset}";
        }
    }
    public IEnumerator RequestNewLevels(string name, int offset = 0, bool clearList = true)
    {
        UnityWebRequest webRequest;

        if (name.StartsWith("#"))
        {
            webRequest = UnityWebRequest.Get(GetDefaultRequestString(name, levelPrefabScripts.Count));
        }
        else
        {
            string url = GetDefaultRequestString(name, levelPrefabScripts.Count);

            //url = url + $"&clearedFilter={clearedFilter}";
            string minDiff = SettingsScript.instance.minDiffDropdown.options[SettingsScript.instance.minDiffDropdown.value].text;
            string maxDiff = SettingsScript.instance.minDiffDropdown.options[SettingsScript.instance.maxDiffDropdown.value].text;
            Dictionary<string, int> reversedDictionary = DiffSpriteHelper.GetReversedDiffIDRegister();
            int minDiffId = reversedDictionary[minDiff];
            int maxDiffId = reversedDictionary[maxDiff];

            List<string> specialDiffs = new();
            if (SettingsScript.instance.showUnratedLevelToggle.isOn) specialDiffs.Add("Unranked");
            if (SettingsScript.instance.showHiddenLevelToggle.isOn) specialDiffs.Add("-2", "-21");
            if (SettingsScript.instance.showExtraLevelToggle.isOn) specialDiffs.Add("Gimmick", "Marathon"); 
            TUFLevelsPostRequest jsonData = new() {
                PGURange = new()
                    { From = DiffSpriteHelper.DiffIDRegister[Math.Min(minDiffId, maxDiffId)], 
                      To = DiffSpriteHelper.DiffIDRegister[Math.Max(minDiffId, maxDiffId)] },
                      SpecialDifficulties = specialDiffs.ToArray() 
            };

            string json = JsonConvert.SerializeObject(jsonData);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

            webRequest = new UnityWebRequest(url, "POST");
            webRequest.uploadHandler = new UploadHandlerRaw(jsonBytes);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
        }

        webRequest.certificateHandler = new CertificateWhore();
        webRequest.disposeCertificateHandlerOnDispose = true;

        yield return webRequest.SendWebRequest();

        if (clearList) levelPrefabScripts.Clear();

        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Main.Logger.Error("levelPrefabScripts Request Error: " + webRequest.error);
        }
        else
        {
            if (clearList) levelPrefabScripts.Clear();

            if (name.StartsWith("#"))
            {
                LevelListInfoElementJson info = JsonConvert.DeserializeObject<LevelListInfoElementJson>(webRequest.downloadHandler.text);
                levelPrefabScripts.Add(info);
            }
            else
            {
                LevelListInfoJson info = LevelListInfoJson.Deserialize(webRequest.downloadHandler.text);
                levelPrefabScripts.AddRange(info.Results);
            }

            isLoading = false;

            LoadLevelList();
        }
    }


    public void LoadLevelList()
    {
        loadingText.SetActive(false);

        for (int i = 0; i < levelListParent.transform.childCount; i++)
        {
            Destroy(levelListParent.transform.GetChild(i).gameObject);
        }


        string search = SearchScript.searchText;
        string minPguDiff = SettingsScript.instance.minDiffDropdown.options[SettingsScript.instance.minDiffDropdown.value].text;
        string maxPguDiff = SettingsScript.instance.maxDiffDropdown.options[SettingsScript.instance.maxDiffDropdown.value].text;

        List<LevelListInfoElementJson> list = new();

        foreach (var level in levelPrefabScripts)
        {
            bool matchesSearch = false;
            
            if (!SearchScript.instance.showClearedToggle.isOn && level.Clears > 0) // if showing uncleared is disabled and the level has > 0 clears
            {
                continue;
            }
            if (!SearchScript.instance.showUnclearedToggle.isOn && level.Clears == 0) // if showing cleared is disabled and the level has 0 clears
            {
                continue;
            }

            if (search.StartsWith("#")) // if search starts with #, try find level by using ID
            {
                matchesSearch = ("#" + level.ID).Equals(search, StringComparison.OrdinalIgnoreCase);
            }
            else // else if search doesnt start with #, try find levelPrefabScripts by using info: artist, creator or song
            {
                string searchLower = search.ToLower();
                matchesSearch =
                level.Artist.ToLower().Contains(searchLower) ||
                level.Creator.ToLower().Contains(searchLower) ||
                level.Song.ToLower().Contains(searchLower);
            }

            if (matchesSearch) list.Add(level);
        }

        int cnt = 0;
        foreach (var level in list)
        {
            GameObject gameObject = Instantiate(levelPrefab);

            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(levelListParent.transform);
            rect.localScale = Vector3.one;
            rect.offsetMin = new Vector2(0, 0);
            rect.offsetMax = new Vector2(0, 0);
            rect.sizeDelta = new Vector2(0, 120);
            rect.anchoredPosition = new Vector3(0, (cnt * -125) - 90);

            

            LevelPrefabScript lps = gameObject.GetComponent<LevelPrefabScript>();
            lps.Init(verticalScroll);
            lps.SetLevelInfo(level, level.Clears);
            cnt++;
        }

        RectTransform contentRect = levelListParent.GetComponent<RectTransform>();
        float totalHeight = cnt * 125 + 90;
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);
        

        availablelevelPrefabScripts = list.Count;

        Main.Logger.Log("Updated Level List: " + LevelListScript.levelPrefabScripts.Count);
    }

    public void SortLevelList()
    {
        if (Main.Setting.orderMode == 0)
        {
            if (Main.Setting.orderByIDMode == 1)
            {
                levelPrefabScripts = levelPrefabScripts.OrderByDescending(level => level.ID).ToList();
            }
            else
            {
                levelPrefabScripts = levelPrefabScripts.OrderBy(level => level.ID).ToList();
            }
        }
        else if (Main.Setting.orderMode == 1)
        {
            if (Main.Setting.orderByDifficultyMode == 1)
            {
                levelPrefabScripts = levelPrefabScripts.OrderByDescending(level => Helper.newDiffToSortNumber(level.DiffId)).ToList();
            }
            else
            {
                levelPrefabScripts = levelPrefabScripts.OrderBy(level => Helper.newDiffToSortNumber(level.DiffId)).ToList();
            }
        }
        if (Main.Setting.orderMode == 2)
        {
            if (Main.Setting.orderByClearsMode == 1)
            {
                levelPrefabScripts = levelPrefabScripts.OrderByDescending(level => level.Clears).ToList();
            }
            else
            {
                levelPrefabScripts = levelPrefabScripts.OrderBy(level => level.Clears).ToList();
            }
        }
    }
    public void DeselectAll()
    {
        foreach (var level in GetLevelPrefabScripts()) 
        {
            level.IsSelected = false;
        }
    }
    public int GetIndexSelected()
    {
        int index = 0;
        foreach (var level in GetLevelPrefabScripts())
        {
            if (level.IsSelected)
            {
                return index;
            }
            index++;
        }
        return -1;
    }
    private LevelPrefabScript[] GetLevelPrefabScripts() => instance.levelListParent.GetComponentsInChildren<LevelPrefabScript>();
}
