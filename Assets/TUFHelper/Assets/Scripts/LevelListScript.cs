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
using System.Threading;
using System.Threading.Tasks;
using TUFHelper.Utils;
using System.IO;
using System.Runtime.CompilerServices;
using DG.Tweening;

public class LevelListViewModel
{
    public List<LevelListInfoElementJson> LevelPrefabScripts { get; } = new();
    public delegate void LevelListAddedRange(LevelListInfoElementJson[] scripts);
    public event Action Cleared;
    public event LevelListAddedRange AddedRange;

    public void AddRange(LevelListInfoElementJson[] elements)
    {
        LevelPrefabScripts.AddRange(elements);
        AddedRange?.Invoke(elements);
    }

    public void Clear()
    {
        LevelPrefabScripts.Clear();
        Cleared?.Invoke();
    }
    public void Add(LevelListInfoElementJson element)
    {
        LevelPrefabScripts.Add(element);
        AddedRange?.Invoke(new[] { element });
    }
}

public class LevelListScript : MonoBehaviour
{
    public const int REQUEST_LIMIT = 30;
    public static LevelListScript instance;

    public LevelListViewModel ViewModel { get; private set; } = new();

    public GameObject levelPrefab, levelListParent, verticalScroll;

    public ScrollRect VerticalScrollComponent { get; private set; } 
    public bool HasMore { get; private set; }
    private bool isLoading = false;
    private CancellationTokenSource requestCancelToken;

    public static readonly TUFAPIRequest_Levels DefaultRequest = new(REQUEST_LIMIT);

    private bool showOnlyDownloaded = false;
    public bool ShowOnlyDownloaded
    {
        get => showOnlyDownloaded;
        set
        {
            LevelInfo.instance.IsShow = value;
            if (showOnlyDownloaded == value)
                return;

            showOnlyDownloaded = value;

            requestCancelToken?.Cancel();

            isLoading = false;
            HasMore = !value;

            ViewModel.Clear();
            DefaultRequest.Offset = 0;

        }
    }
    private bool showOnlyFavorites;
    public bool ShowOnlyFavorites
    {
        get => showOnlyFavorites;
        set
        {
            // if (showOnlyFavorites == value)
            //     return;
            showOnlyFavorites = value;

            requestCancelToken?.Cancel();

            //isLoading = false;
            //HasMore = !value;

            ViewModel.Clear();
            //DefaultRequest.Offset = 0;
        }
    }

    private bool groupByFolder;
    public bool GroupByFolder
    {
        get => groupByFolder;
        set
        {
            groupByFolder = value;

            ViewModel.Clear();
        }
    }

    public LevelFolder LevelFolder { get; set; }

    private int randomIDLevel = -1;


    public void Awake()
    {
        instance = this;

        VerticalScrollComponent = verticalScroll.GetComponent<ScrollRect>();

        ViewModel.Cleared += OnLevelsCleared;
        ViewModel.AddedRange += OnLevelsAdded;
    }
    public void OnLevelsCleared()
    {
        for (int i = 0; i < levelListParent.transform.childCount; i++)
        {
            Destroy(levelListParent.transform.GetChild(i).gameObject);
        }

        verticalScroll.GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
    }

    public void OnLevelsAdded(LevelListInfoElementJson[] levels)
    {
        int startingIndex = levelListParent.transform.childCount;

        foreach (var level in levels)
        {
            GameObject gameObject = Instantiate(levelPrefab);
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(levelListParent.transform, false);
            rect.localScale = Vector3.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.sizeDelta = new Vector2(0, 120);

            rect.anchoredPosition = new Vector3(0, (startingIndex * -125) - 90);

            LevelPrefabScript lps = gameObject.GetComponent<LevelPrefabScript>();
            lps.Init(verticalScroll);
            lps.SetLevelInfo(level, level.Clears);

            startingIndex++;
        }

        RectTransform contentRect = levelListParent.GetComponent<RectTransform>();
        float totalHeight = ViewModel.LevelPrefabScripts.Count * 125 + 90;
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);

        if (randomIDLevel == -177013 || !ShowOnlyDownloaded) return;

        if (randomIDLevel == -1)
        {
            randomIDLevel = UnityEngine.Random.Range(0, GetLevelPrefabScripts().Length - 1);
        }

        if (randomIDLevel != -1)
        {
            SelectIndex(GetLevelPrefabScripts(), randomIDLevel);
            randomIDLevel = -177013;
        }
    }

    public void Update()
    {
        var levelPrefabs = GetLevelPrefabScripts();
        int selectedIndex = GetIndexSelected();

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (selectedIndex == -1 && levelPrefabs.Length > 0)
            {
                SelectIndex(levelPrefabs, 0);
                return;
            }

            if (selectedIndex > 0)
            {
                SelectIndex(levelPrefabs, selectedIndex - 1);
            }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (selectedIndex == -1 && levelPrefabs.Length > 0)
            {
                SelectIndex(levelPrefabs, 0);
                return;
            }

            if (selectedIndex < levelPrefabs.Length - 1)
            {
                SelectIndex(levelPrefabs, selectedIndex + 1);
            }
        }


        // Handle Scroll-Based Pagination
        if (!isLoading && !ShowOnlyDownloaded && VerticalScrollComponent.verticalNormalizedPosition <= 0.01f && HasMore)
        {
            isLoading = true;
            DefaultRequest.Offset = GetLevelPrefabScripts().Length;
            UpdateLevelList();
        }
    }

    private void SelectIndex(LevelPrefabScript[] levelPrefabs, int index)
    {
        for (int i = 0; i < levelPrefabs.Length; i++)
        {
            levelPrefabs[i].IsSelected = i == index;
        }

        var selected = levelPrefabs[index];
        //WindowsManager.instance.ShowPassList();
    }

    public IOrderedEnumerable<LevelListInfoElementJson> SortLevels(IEnumerable<LevelListInfoElementJson> levels)
    {
        switch (DefaultRequest.SortBy)
        {
            case "RECENT":
                if (DefaultRequest.SortAsc == AscendingOrDescending.Ascending) return levels.OrderBy(e => e.ID);
                else return levels.OrderByDescending(e => e.ID);
            case "DIFF":
                if (DefaultRequest.SortAsc == AscendingOrDescending.Ascending) return levels.OrderBy(e => e.DiffId);
                else return levels.OrderByDescending(e => e.DiffId);
            case "CLEARS":
                if (DefaultRequest.SortAsc == AscendingOrDescending.Ascending) return levels.OrderBy(e => e.Clears);
                else return levels.OrderByDescending(e => e.Clears);
            case "LIKES":
                if (DefaultRequest.SortAsc == AscendingOrDescending.Ascending) return levels.OrderBy(e => e.Likes);
                else return levels.OrderByDescending(e => e.Likes);
            default:
                return levels.OrderBy(e => e.ID);
        }
    }

    public async void UpdateLevelList()
    {
        await UpdateLevelListAsync();
    }
    public async Task UpdateLevelListAsync()
    {
        DeselectAll();
        requestCancelToken?.Cancel();
        requestCancelToken = new CancellationTokenSource();
        CancellationToken token = requestCancelToken.Token;

        if (ShowOnlyDownloaded)
        {
            isLoading = false;
            DefaultRequest.Offset = 0;
            HasMore = false;

            var levels = Main.Setting.DownloadedLevels
                .Where(dl => dl.LevelInfo != null)
                .Select(dl => dl.LevelInfo);

            var sortedLevels = SortLevels(levels);

            string query = DefaultRequest.Query.ToLower();
            bool searchId = query.StartsWith("#");
            int queryId = 0;
            if (searchId && int.TryParse(query.Substring(1), out int id))
                queryId = id;

            var filteredLevels = sortedLevels.Where(level =>
            {
                // Filter by #ID
                if (searchId && level.ID != queryId)
                    return false;

                // Filter by difficulty
                if (DiffSpriteHelper.IsSpecialDiff(level.DiffId))
                {
                    if (!DefaultRequest.SpecialDifficulties.Contains(DiffSpriteHelper.DiffIDRegister[level.DiffId]))
                        return false;
                }
                else
                {
                    if (level.DiffId < DefaultRequest.MinDiffPGU || level.DiffId > DefaultRequest.MaxDiffPGU)
                        return false;
                }

                // Filter by text
                if (!searchId && !string.IsNullOrEmpty(query))
                {
                    string creator = level.Creator.ToLower();
                    string artist = level.Artist.ToLower();
                    string song = level.Song.ToLower();

                    if (!(creator.Contains(query) || artist.Contains(query) || song.Contains(query)))
                        return false;
                }
                // Filter by group
                if (GroupByFolder)
                {
                    if (LevelFolder != null && !LevelFolder.Levels.Contains(level.ID))
                        return false;
                }

                return true;
            }).ToList();

            if (ShowOnlyDownloaded && ShowOnlyFavorites)
            {
                foreach (var level in filteredLevels.ToArray())
                {
                    if (!Main.Setting.FavoriteLevels.Contains(level.ID)) filteredLevels.Remove(level);
                }
            }

            int i = 0;
            foreach (var level in filteredLevels)
            {
                GameObject gameObject = Instantiate(levelPrefab);
                RectTransform rect = gameObject.GetComponent<RectTransform>();
                rect.SetParent(levelListParent.transform, false);
                rect.localScale = Vector3.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.sizeDelta = new Vector2(0, 120);

                rect.anchoredPosition = new Vector3(0, (i * -125) - 90);

                LevelPrefabScript lps = gameObject.GetComponent<LevelPrefabScript>();
                lps.Init(verticalScroll);
                lps.SetLevelInfo(level, level.Clears);

                i++;
            }

            RectTransform contentRect = levelListParent.GetComponent<RectTransform>();
            float totalHeight = filteredLevels.Count * 125 + 90;
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);

            ViewModel.LevelPrefabScripts.AddRange(levels.ToArray());

            

            return;
        }


        try
        {
            isLoading = true;

            if (DefaultRequest.Query.StartsWith("#"))
            {
                string id = DefaultRequest.Query.Substring(1);
                string url = $"https://api.tuforums.com/v2/database/levels/byId/{id}";

                using var request = UnityWebRequest.Get(url);
                request.certificateHandler = new CertificateWhore();
                request.disposeCertificateHandlerOnDispose = true;

                var op = request.SendWebRequest();
                while (!op.isDone)
                {
                    await Task.Yield();
                    token.ThrowIfCancellationRequested();
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    return;
                }

                var json = request.downloadHandler.text;
                var level = JsonConvert.DeserializeObject<LevelListInfoElementJson>(json);

                ViewModel.Clear();
                ViewModel.Add(level);
                HasMore = false;
            }
            else
            {
                await DefaultRequest.GetAnswerAsync(token);

                if (!string.IsNullOrEmpty(DefaultRequest.Answer))
                {
                    var json = JsonConvert.DeserializeObject<LevelListInfoJson>(DefaultRequest.Answer);
                    ViewModel.AddRange(json.Results.ToArray());
                    HasMore = json.HasMore;
                }
                else
                {
                    Main.Logger.Error("Empty answer");
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Main.Logger.Error("Update failed: " + ex.Message);
            Main.Logger.Error(ex.Source);
        }
        finally
        {
            isLoading = false;
        }
    }

    
    public void ClearLevels()
    {
        ViewModel.Clear();
        DefaultRequest.Offset = 0;
    }

    public void DeselectAll()
    {
        foreach (var level in GetLevelPrefabScripts()) 
        {
            level.IsSelected = false;
        }
        //WindowsManager.instance.ShowPassList();
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
    
    public LevelPrefabScript[] GetLevelPrefabScripts() =>
    instance.levelListParent
        .GetComponentsInChildren<LevelPrefabScript>(includeInactive: false);
}
