using DG.Tweening;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using Together.Utils;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using TUFHelper.ModScripts.Web;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

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
    public const int REQUEST_LIMIT = 50;
    //public static LevelListScript instance;

    public LevelListViewModel ViewModel { get; private set; } = new();

    public GameObject levelPrefab, levelListParent, verticalScroll, errorMessage, loadingIcon;
    public Toggle showOnlyDownloadedToggle, showOnlyFavoritesToggle, enableQRangeToggle;
    public DiffSliderPGU diffSliderPGU;
    public DiffSliderQuantum diffSliderQ;
    public CurationTypesScript curationTypes;
    public SpecialDifficultiesUI specialDifficulties;
    public TagsScript tags;
    public TMP_InputField searchField;
    public TMP_Dropdown sortByDropdown, sortOrderDropdown;

    public ScrollRect VerticalScrollComponent { get; private set; }
    public bool HasMore { get; private set; }
    private bool isLoading = false;
    private CancellationTokenSource requestCancelToken;

    public TUFAPIRequest_Levels DefaultRequest;

    public void Awake()
    {
        var settings = Main.Setting;
        string sortBy = settings.SortBy switch
        {
            0 => "RECENT",
            1 => "DIFF",
            2 => "CLEARS",
            3 => "LIKES",
            _ => "UNKNOWN",
        };
        DefaultRequest = new(REQUEST_LIMIT)
        {
            MinDiffPGU = settings.MinDiff,
            MaxDiffPGU = settings.MaxDiff,
            QDifficulties = settings.SelectedQDiifs,
            SortAsc = settings.SortOrder,
            SortBy = sortBy,
            CurationTypes = settings.SelectedCurationTypes,
            SpecialDifficulties = settings.SelectedSpecialDiffs,
            TagsFilter = settings.SelectedTags,
        };

        VerticalScrollComponent = verticalScroll.GetComponent<ScrollRect>();

        ViewModel.Cleared += OnLevelsCleared;
        ViewModel.AddedRange += OnLevelsAdded;
    }
    public async void Start()
    {
        sortByDropdown.value = Main.Setting.SortBy;
        int sortOrder;
        if (Main.Setting.SortOrder == AscendingOrDescending.Ascending)
        {
            sortOrder = 0;
        }
        else
        {
            sortOrder = 1;
        }
        sortOrderDropdown.value = sortOrder;
        enableQRangeToggle.isOn = Main.Setting.EnableQRange;
        showOnlyDownloadedToggle.isOn = Main.Setting.ShowOnlyDownloaded;
        showOnlyFavoritesToggle.isOn = Main.Setting.ShowOnlyFavorites;

        diffSliderPGU.DiffSliderPGUChanged += OnDiffSliderPGUChanged;
        diffSliderQ.DiffSliderQuantumChanged += DiffSliderQuantumChanged;
        searchField.onEndEdit.AddListener(OnSearchFieldChanged);
        sortByDropdown.onValueChanged.AddListener(OnSortByChanged);
        sortOrderDropdown.onValueChanged.AddListener(OnSortOrderChanged);
        enableQRangeToggle.onValueChanged.AddListener(OnEnableQRangeChanged);
        showOnlyDownloadedToggle.onValueChanged.AddListener(OnShowOnlyDownloadedChanged);
        showOnlyFavoritesToggle.onValueChanged.AddListener(OnShowOnlyFavoritesChanged);

        curationTypes.CurationTypesChanged += CurationTypesChanged;
        specialDifficulties.Changed += SpecialDifficultiesChanged;
        tags.Changed += TagsChanged;

        errorMessage.SetActive(false);

        await UpdateLevelListAsync();
    }

    private async void TagsChanged(object sender, TagsScript.TagsScriptArgs e)
    {
        DefaultRequest.TagsFilter = e.Tags.ToList();

        Main.Setting.SelectedTags = e.Tags.ToList();

        ClearLevels();
        await UpdateLevelListAsync();
    }

    private async void SpecialDifficultiesChanged(object sender, SpecialDifficultiesUI.SpecialDifficultiesArgs e)
    {
        DefaultRequest.SpecialDifficulties = e.Selected.ToList();

        Main.Setting.SelectedSpecialDiffs = e.Selected.ToList();
        Main.Setting.Save(Main.ModEntry);

        ClearLevels();
        await UpdateLevelListAsync();
    }

    private async void CurationTypesChanged(object sender, CurationTypesScript.CurationTypesArgs e)
    {
        DefaultRequest.CurationTypes = e.Selected.ToList();

        Main.Setting.SelectedCurationTypes = e.Selected.ToList();
        Main.Setting.Save(Main.ModEntry);

        ClearLevels();
        await UpdateLevelListAsync();
    }

    private async void DiffSliderQuantumChanged(object sender, DiffSliderQuantum.DiffSliderQuantumArgs e)
    {
        DefaultRequest.QDifficulties = DiffSliderQuantum.GetSelectedQDiffs(e.MinDiff, e.MaxDiff).ToList();

        Main.Setting.MinQDiff = e.MinDiff;
        Main.Setting.MaxQDiff = e.MaxDiff;
        Main.Setting.Save(Main.ModEntry);

        ClearLevels();

        Main.Logger.Log("changed");

        await UpdateLevelListAsync();
    }

    public async void OnDiffSliderPGUChanged(object sender, DiffSliderPGU.DiffSliderPGUArgs e)
    {
        DefaultRequest.MinDiffPGU = e.MinDiff;
        DefaultRequest.MaxDiffPGU = e.MaxDiff;

        Main.Setting.MinDiff = e.MinDiff;
        Main.Setting.MaxDiff = e.MaxDiff;
        Main.Setting.Save(Main.ModEntry);

        ClearLevels();
        await UpdateLevelListAsync();
    }

    public async void OnShowOnlyDownloadedChanged(bool value)
    {
        requestCancelToken?.Cancel();

        isLoading = false;
        HasMore = !value;

        try
        {
            ViewModel.Clear();
        }
        catch (Exception ex)
        {
            Main.Logger.LogException(ex);
        }
        DefaultRequest.Offset = 0;

        Main.Setting.ShowOnlyDownloaded = value;
        Main.Setting.Save(Main.ModEntry);

        ClearLevels();
        await UpdateLevelListAsync();
    }
    public async void OnShowOnlyFavoritesChanged(bool value)
    {
        Main.Setting.ShowOnlyFavorites = value;
        Main.Setting.Save(Main.ModEntry);

        ViewModel.Clear();
        await UpdateLevelListAsync();
    }

    public async void OnSearchFieldChanged(string value)
    {
        DefaultRequest.Query = value;
        DefaultRequest.Offset = 0;
        ClearLevels();
        await UpdateLevelListAsync();
    }
    public void OnLevelsCleared()
    {
        for (int i = levelListParent.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = levelListParent.transform.GetChild(i);

            child.SetParent(null);
            Destroy(child.gameObject);
        }

        verticalScroll.GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
    }
    public async void OnSortByChanged(int value)
    {
        switch (value)
        {
            case 0:
                DefaultRequest.SortBy = "RECENT";
                break;
            case 1:
                DefaultRequest.SortBy = "DIFF";
                break;
            case 2:
                DefaultRequest.SortBy = "CLEARS";
                break;
            case 3:
                DefaultRequest.SortBy = "LIKES";
                break;
        }

        ClearLevels();
        await UpdateLevelListAsync();
    }
    public async void OnSortOrderChanged(int value)
    {
        switch (value)
        {
            case 0:
                DefaultRequest.SortAsc = AscendingOrDescending.Ascending;
                break;
            case 1:
                DefaultRequest.SortAsc = AscendingOrDescending.Descending;
                break;
        }

        ClearLevels();
        await UpdateLevelListAsync();
    }
    public async void OnEnableQRangeChanged(bool value)
    {
        DefaultRequest.EnableQRange = value;
        Main.Setting.EnableQRange = value;
        Main.Setting.Save(Main.ModEntry);

        ClearLevels();
        await UpdateLevelListAsync();
    }

    public void OnLevelsAdded(LevelListInfoElementJson[] levels)
    {
        int startingIndex = levelListParent.transform.childCount;
        int animationIndex = 0;

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
            lps.Init(verticalScroll, levelListParent.transform);
            lps.SetLevelInfo(level, level.Clears);
            UITransition.AnimateLevelListItem(gameObject, animationIndex, 30f);

            startingIndex++;
            animationIndex++;
        }

        RectTransform contentRect = levelListParent.GetComponent<RectTransform>();
        float totalHeight = ViewModel.LevelPrefabScripts.Count * 125 + 90;
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);
    }

    public async void Update()
    {
        //var levelPrefabs = GetLevelPrefabScripts();
        //int selectedIndex = GetIndexSelected();

        //if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        //{
        //    var selected = EventSystem.current.currentSelectedGameObject;
        //    var inputField = selected != null ? selected.GetComponent<TMP_InputField>() : null;
        //    bool isTyping = inputField != null && inputField.isFocused;

        //    if (!isTyping && levelPrefabs.Length > 0)
        //    {
        //        if (selectedIndex == -1)
        //        {
        //            SelectIndex(levelPrefabs, 0);
        //            return;
        //        }

        //        // Wrap to 0 if at last
        //        int nextIndex = (selectedIndex + 1) % levelPrefabs.Length;
        //        SelectIndex(levelPrefabs, nextIndex);
        //    }
        //}
        //else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.UpArrow))
        //{
        //    var selected = EventSystem.current.currentSelectedGameObject;
        //    var inputField = selected != null ? selected.GetComponent<TMP_InputField>() : null;
        //    bool isTyping = inputField != null && inputField.isFocused;

        //    if (!isTyping && levelPrefabs.Length > 0)
        //    {
        //        if (selectedIndex == -1)
        //        {
        //            SelectIndex(levelPrefabs, 0);
        //            return;
        //        }

        //        // Wrap to last if at 0
        //        int prevIndex = (selectedIndex - 1 + levelPrefabs.Length) % levelPrefabs.Length;
        //        SelectIndex(levelPrefabs, prevIndex);
        //    }
        //}



        // Handle Scroll-Based Pagination
        if (!isLoading && !showOnlyDownloadedToggle.isOn && VerticalScrollComponent.verticalNormalizedPosition <= 0.01f && HasMore)
        {
            isLoading = true;
            DefaultRequest.Offset = GetLevelPrefabScripts().Length;
            await UpdateLevelListAsync();
        }
    }

    private void SelectIndex(LevelPrefabScript[] levelPrefabs, int index)
    {
        for (int i = 0; i < levelPrefabs.Length; i++)
        {
            levelPrefabs[i].IsSelected = i == index;
        }
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

    public async Task UpdateLevelListAsync()
    {
        //DeselectAll();
        requestCancelToken?.Cancel();
        requestCancelToken = new CancellationTokenSource();
        CancellationToken token = requestCancelToken.Token;

        if (showOnlyDownloadedToggle.isOn)
        {
            isLoading = false;
            DefaultRequest.Offset = 0;
            HasMore = false;

            var levels = Main.DownloadedLevels.Levels;

            var sortedLevels = SortLevels(levels);

            string query = searchField.text;
            bool searchId = query.StartsWith("#");
            int queryId = 0;
            if (searchId && int.TryParse(searchField.text[1..], out int id))
                queryId = id;

            var filteredLevels = sortedLevels.Where(level =>
            {
                // Filter by #ID
                if (searchId && level.ID != queryId)
                    return false;

                // Filter by difficulty
                var diffName = DiffSpriteHelper.DiffIDRegister[level.DiffId];


                if (DiffSpriteHelper.IsQuantumDiff(level.DiffId))
                {
                    if (!DefaultRequest.EnableQRange) return false;
                    if (!DefaultRequest.QDifficulties.Contains(diffName))
                        return false;
                }
                else if (DiffSpriteHelper.IsSpecialDiff(level.DiffId))
                {
                    if (!DefaultRequest.SpecialDifficulties.Contains(diffName))
                        return false;
                }
                else
                {
                    if (level.DiffId < DefaultRequest.MinDiffPGU ||
                        level.DiffId > DefaultRequest.MaxDiffPGU)
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
                // Filter by tags
                if (DefaultRequest.TagsFilter != null && DefaultRequest.TagsFilter.Count > 0)
                {
                    if (level.Tags == null || level.Tags.Count == 0)
                        return false;

                    var levelTags = level.Tags
                        .Where(t => !string.IsNullOrWhiteSpace(t?.Name))
                        .Select(t => t.Name.Trim().ToLower())
                        .ToList();

                    var requiredTags = DefaultRequest.TagsFilter
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .Select(t => t.Trim().ToLower())
                        .ToList();

                    if (!requiredTags.All(tag => levelTags.Contains(tag)))
                        return false;
                }
                // Filter by curations
                if (DefaultRequest.CurationTypes != null && DefaultRequest.CurationTypes.Count > 0)
                {
                    if (level.Curation == null || level.Curation.TypeIds.Count == 0)
                        return false;
                    List<string> curations = new();
                    foreach (var id in level.Curation.TypeIds)
                    {
                        curations.Add(CurationHelper.IDRegister[id]);
                    }

                    var requiredCurations = DefaultRequest.CurationTypes;

                    if (!requiredCurations.All(curation => curations.Contains(curation)))
                        return false;
                }

                return true;
            }).ToList();

            if (showOnlyDownloadedToggle.isOn && showOnlyFavoritesToggle.isOn)
            {
                foreach (var level in filteredLevels.ToArray())
                {
                    if (!Main.Setting.FavoriteLevels.Contains(level.ID)) filteredLevels.Remove(level);
                }
            }

            ViewModel.Clear();
            ViewModel.AddRange(filteredLevels.ToArray());

            return;
        }


        try
        {
            UITransition.Show(loadingIcon);
            isLoading = true;

            if (DefaultRequest.Query.StartsWith("#"))
            {
                string id = DefaultRequest.Query.Substring(1);

                if (!int.TryParse(id, out _))
                {
                    return;
                }

                string url = $"https://api.tuforums.com/v2/database/levels/{id}";

                try
                {
                    using var response = await Main.Client.GetAsync(url, token);

                    response.EnsureSuccessStatusCode();

                    string json = await response.Content.ReadAsStringAsync();

                    var level = JsonConvert.DeserializeObject<LevelListElementId>(json).Level;

                    if (level == null) return;

                    ViewModel.Clear();
                    ViewModel.Add(level);
                    HasMore = false;

                    errorMessage.SetActive(false);
                }
                catch (OperationCanceledException)
                {
                    // Task was canceled via CancellationToken
                }
                catch (Exception ex)
                {
                    Main.Logger.LogException(ex);
                    errorMessage.SetActive(true);
                }
            }
            else
            {
                try
                {
                    await DefaultRequest.GetAnswerAsync(token);
                    errorMessage.SetActive(false);
                }
                catch (OperationCanceledException)
                {

                }
                catch (Exception ex)
                {
                    Main.Logger.LogException(ex);
                    errorMessage.SetActive(true);
                }

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
            UITransition.Hide(loadingIcon);
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
    levelListParent
        .GetComponentsInChildren<LevelPrefabScript>(includeInactive: false);
}
