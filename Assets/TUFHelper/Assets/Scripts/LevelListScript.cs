using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TUFHelper;
using UnityEngine;
using UnityEngine.Networking;
using Together.Utils;
using TUFHelper.ModScripts.Json;
using UnityEngine.UI;
using TUFHelper.ModScripts.Web;
using TMPro;
using System.Linq;
using UnityEngine.EventSystems;

public class LevelListScript : MonoBehaviour
{
    public const int REQUEST_LIMIT = 60;
    public static LevelListScript instance;

    public static int availablelevelPrefabScripts = 0;
    public static List<LevelListInfoElementJson> levelPrefabScripts = new List<LevelListInfoElementJson>();

    public GameObject levelPrefab, levelListParent, verticalScroll;

    public ScrollRect VerticalScrollComponent { get; private set; } 
    public bool HasMore { get; private set; }
    private bool isLoading = false;

    public static readonly TUFAPIRequest_Levels DefaultRequest = new(REQUEST_LIMIT);

    public void Awake()
    {
        instance = this;

        VerticalScrollComponent = verticalScroll.GetComponent<ScrollRect>();
    }
    public void Start()
    {
        UpdateLevelList();       
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
        {
            var selected = EventSystem.current.currentSelectedGameObject;
            bool isTyping = selected != null && selected.GetComponent<TMP_InputField>() != null;

            if (!isTyping)
            {
                if (GetIndexSelected() != -1)
                {
                    GetLevelPrefabScripts()[GetIndexSelected()].PlayButtonClick();
                    return;
                }
            }
        }
        var levelPrefabScripts = GetLevelPrefabScripts();

        // Handle Up Arrow Key
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (GetIndexSelected() == -1 && levelPrefabScripts.Length > 0)
            {
                levelPrefabScripts[0].IsSelected = true;
                return;
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
                return;
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
        if (!isLoading && VerticalScrollComponent.verticalNormalizedPosition <= 0.01f && HasMore)
        {
            
            isLoading = true;
            DefaultRequest.Offset = levelPrefabScripts.Length;
            UpdateLevelList();
        }
    }


    public void UpdateLevelList()
    {
        StartCoroutine(RequestNewLevels());
    }

    public void ClearLevels()
    {
        levelPrefabScripts.Clear();
        DefaultRequest.Offset = 0;
        LoadLevelList();
    }

    public IEnumerator RequestNewLevels()
    {
        if (DefaultRequest.Query.StartsWith("#"))
        {
            string newName = LevelListScript.DefaultRequest.Query.Substring(1);
            UnityWebRequest webRequest = UnityWebRequest.Get($"https://api.tuforums.com/v2/database/levels/byId/{newName}");
            webRequest.certificateHandler = new CertificateWhore();
            webRequest.disposeCertificateHandlerOnDispose = true;

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Main.Logger.Error("Request Error: " + webRequest.error);
            }
            else
            {
                levelPrefabScripts.Clear();

                LevelListInfoElementJson element = JsonConvert.DeserializeObject<LevelListInfoElementJson>(webRequest.downloadHandler.text);
                levelPrefabScripts.Add(element);

                HasMore = false;
            }
        }
        else
        {
            yield return DefaultRequest.GetAnswerCo();

            if (!string.IsNullOrEmpty(DefaultRequest.Answer))
            {
                LevelListInfoJson json = JsonConvert.DeserializeObject<LevelListInfoJson>(DefaultRequest.Answer);
                levelPrefabScripts.AddRange(json.Results);

                HasMore = json.HasMore;
            }
            else
            {
                Main.Logger.Error("Empty answer");
            }
        }

        isLoading = false;

        LoadLevelList();
    }


    public void LoadLevelList()
    {
        for (int i = 0; i < levelListParent.transform.childCount; i++)
        {
            Destroy(levelListParent.transform.GetChild(i).gameObject);
        }


        int cnt = 0;
        foreach (var level in levelPrefabScripts)
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
