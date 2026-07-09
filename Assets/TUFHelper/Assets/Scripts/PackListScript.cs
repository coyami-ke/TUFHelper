using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using TUFHelper.ModScripts.Web;
using UnityEngine;

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

    public GameObject packPrefab;

    public TUFAPIRequest_Packs WebRequest { get; private set; }
    public PackListViewModel ViewModel { get; private set;  }

    public void Awake()
    {
        WebRequest = new(30);
        ViewModel = new();
    }
    public void Start()
    {
        Main.Logger.Log("start");
    }
    public async void OnSearchBarEnter(string text)
    {
        await UpdatePackListAsync();

        foreach (var pack in ViewModel.PackPrefabScripts)
        {
            Main.Logger.Log(pack.Name);
        }

        Main.Logger.Log("request");
    }
    public void OnSortByChanged(int index)
    {
    }
    public void OnSortOrderChanged(int index)
    {
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
    }
}
