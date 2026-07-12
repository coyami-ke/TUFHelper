using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
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

    private RectTransform packsParentRect;

    public TUFAPIRequest_Packs WebRequest { get; private set; }
    public PackListViewModel ViewModel { get; private set;  }

    public void Awake()
    {
        WebRequest = new(30);
        ViewModel = new();

        ViewModel.AddedRange += ViewModel_AddedRange;
        ViewModel.Cleared += ViewModel_Cleared;
    }

    public async void Update()
    {
        if (!isLoading && verticalScroll.GetComponent<ScrollRect>().verticalNormalizedPosition <= 0.01f)
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

        verticalScroll.GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
    }

    private void ViewModel_AddedRange(PackListElementJson[] scripts)
    {
        int currentTotalCount = packsParent.transform.childCount;

        float widthPack = 332;   
        float heightPack = 240;  

        float spacingX = 20f;
        float spacingY = 40f;

        float startX = 50f ;
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

    public async void Start()
    {
        packsParentRect = packsParent.GetComponent<RectTransform>();

        await UpdatePackListAsync();
    }
    public async void OnSearchBarEnter(string text)
    {
        WebRequest.Query = SearchScript.NormalizeSearchText(text);

        ClearPacks();
        await UpdatePackListAsync();
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
    }

    public async Task UpdatePackListAsync()
    {
        isLoading = true;

        requestCancelToken?.Cancel();
        requestCancelToken = new CancellationTokenSource();
        CancellationToken token = requestCancelToken.Token;

        await WebRequest.GetAnswerAsync(token);

        if (!string.IsNullOrEmpty(WebRequest.Answer))
        {
            var json = JsonConvert.DeserializeObject<PackListJson>(WebRequest.Answer);
            ViewModel.AddRange(json.Packs);
        }
        else
        {
            Main.Logger.Error("Empty answer");
        }

        isLoading = false;
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
}
