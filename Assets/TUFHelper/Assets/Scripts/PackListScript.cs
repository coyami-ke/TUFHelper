using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using TUFHelper.ModScripts.Web;
using UnityEngine;
using UnityEngine.UI;

public class PackListViewModel
{
    public List<PackListElementJson> PackPrefabScripts { get; } = new();
    public delegate void PackListAddedRange(PackListElementJson[] scripts);
    public event Action Cleared;
    public event PackListAddedRange AddedRange;

    public void AddRange(PackListElementJson[] elements)
    {
        PackPrefabScripts.AddRange(elements);
        AddedRange?.Invoke(elements);
    }

    public void Clear()
    {
        PackPrefabScripts.Clear();
        Cleared?.Invoke();
    }
    public void Add(PackListElementJson element)
    {
        PackPrefabScripts.Add(element);
        AddedRange?.Invoke(new[] { element });
    }
}

public class PackListScript : MonoBehaviour
{
    private CancellationTokenSource requestCancelToken;
    private bool isLoading = false;

    public GameObject packPrefab, packsParent, verticalScroll;

    public TMP_InputField searchField;
    public TMP_Dropdown sortByDropdown, sortOrderDropdown;

    private RectTransform packsParentRect;

    public TUFAPIRequest_Packs WebRequest { get; private set; }
    public PackListViewModel ViewModel { get; private set; }

    public void Awake()
    {
        WebRequest = new(15);
        ViewModel = new();

        ViewModel.AddedRange += ViewModel_AddedRange;
        ViewModel.Cleared += ViewModel_Cleared;
    }

    public async void Start()
    {
        packsParentRect = packsParent.GetComponent<RectTransform>();

        // --- DYNAMIC EVENT LISTENER REGISTRATION ---
        if (searchField != null)
        {
            // Fires automatically when the player presses Enter or deselects the box
            searchField.onEndEdit.AddListener(OnSearchBarEnter);
        }

        if (sortByDropdown != null)
        {
            // Fires instantly when a new option item is clicked
            sortByDropdown.onValueChanged.AddListener(OnSortByChanged);
        }

        if (sortOrderDropdown != null)
        {
            sortOrderDropdown.onValueChanged.AddListener(OnSortOrderChanged);
        }

        // Run the initial API request data fetch
        await UpdatePackListAsync();
    }

    public async void Update()
    {
        if (verticalScroll == null) return;

        ScrollRect scroll = verticalScroll.GetComponent<ScrollRect>();
        if (!isLoading && scroll != null && scroll.verticalNormalizedPosition <= 0.01f)
        {
            isLoading = true;
            WebRequest.Offset = GetPackPrefabScripts().Length;
            await UpdatePackListAsync();
        }
    }

    private void ViewModel_Cleared()
    {
        for (int i = packsParent.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = packsParent.transform.GetChild(i);
            child.SetParent(null);
            Destroy(child.gameObject);
        }

        if (verticalScroll != null)
        {
            ScrollRect scroll = verticalScroll.GetComponent<ScrollRect>();
            if (scroll != null) scroll.verticalNormalizedPosition = 1f;
        }
    }

    private void ViewModel_AddedRange(PackListElementJson[] scripts)
    {
        int currentTotalCount = packsParent.transform.childCount;

        float widthPack = 332;
        float heightPack = 240;

        float spacingX = 20f;
        float spacingY = 40f;

        float startX = 50f;
        float startY = -20f;

        foreach (var pack in scripts)
        {
            GameObject gameObject = Instantiate(packPrefab);
            BundleFontFixer.FixFontsIn(gameObject);

            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(packsParent.transform, false);

            PackPrefabScript pps = gameObject.GetComponent<PackPrefabScript>();
            pps.SetPackInfo(pack);

            int column = currentTotalCount % 3;
            int row = currentTotalCount / 3;

            float posX = startX + (column * (widthPack + spacingX));
            float posY = startY - (row * (heightPack + spacingY));

            rect.anchoredPosition = new Vector2(posX, posY);

            currentTotalCount++;
        }

        int totalRows = Mathf.CeilToInt(currentTotalCount / 3f);
        RectTransform contentRect = packsParent.GetComponent<RectTransform>();

        float totalHeight = 20f + (totalRows * (heightPack + spacingY));
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);
    }

    public async void OnSearchBarEnter(string text)
    {
        WebRequest.Query = text;

        ClearPacks();
        await UpdatePackListAsync();

        Main.Logger.Log("Search: " + WebRequest.Query);
    }

    public async void OnSortByChanged(int index)
    {
        switch (index)
        {
            case 0:
                WebRequest.SortBy = "RECENT";
                break;
            case 1:
                WebRequest.SortBy = "NAME";
                break;
            case 2:
                WebRequest.SortBy = "LEVELS";
                break;
            case 3:
                WebRequest.SortBy = "FAVORITES";
                break;
        }

        ClearPacks();
        await UpdatePackListAsync();

        Main.Logger.Log("Sort By: " + WebRequest.SortBy.ToString());
    }

    public async void OnSortOrderChanged(int index)
    {
        switch (index)
        {
            case 0:
                WebRequest.SortAsc = AscendingOrDescending.Ascending;
                break;
            case 1:
                WebRequest.SortAsc = AscendingOrDescending.Descending;
                break;
        }

        ClearPacks();
        await UpdatePackListAsync();

        Main.Logger.Log("Sort Order: " + WebRequest.SortAsc.ToString());
    }

    public async Task UpdatePackListAsync()
    {
        isLoading = true;

        requestCancelToken?.Cancel();
        requestCancelToken = new CancellationTokenSource();
        CancellationToken token = requestCancelToken.Token;

        try
        {
            await WebRequest.GetAnswerAsync(token);

            if (!string.IsNullOrEmpty(WebRequest.Answer))
            {
                var json = JsonConvert.DeserializeObject<PackListJson>(WebRequest.Answer);
                if (json?.Packs != null)
                {
                    ViewModel.AddRange(json.Packs);
                }
            }
            else
            {
                Main.Logger.Error("Empty answer");
            }
        }
        catch (OperationCanceledException)
        {
            // Canceled cleanly
        }
        catch (Exception ex)
        {
            Main.Logger.Error($"Update operation failed: {ex.Message}");
        }
        finally
        {
            isLoading = false;
        }
    }

    public void ClearPacks()
    {
        ViewModel.Clear();
        WebRequest.Offset = 0;
    }

    public PackPrefabScript[] GetPackPrefabScripts()
    {
        return packsParent.GetComponentsInChildren<PackPrefabScript>(includeInactive: false);
    }

    private void OnDestroy()
    {
        if (searchField != null) searchField.onEndEdit.RemoveListener(OnSearchBarEnter);
        if (sortByDropdown != null) sortByDropdown.onValueChanged.RemoveListener(OnSortByChanged);
        if (sortOrderDropdown != null) sortOrderDropdown.onValueChanged.RemoveListener(OnSortOrderChanged);

        requestCancelToken?.Cancel();
    }
}