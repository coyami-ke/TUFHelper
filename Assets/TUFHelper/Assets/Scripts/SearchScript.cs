using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using TUFHelper;
using UnityEngine;
using UnityEngine.UI;

public class SearchScript : MonoBehaviour
{

    public static SearchScript instance;
    public static string searchText = "";
    public TMP_InputField searchField;

    public void Awake()
    {
        instance = this;
        searchField.text = searchText;
    }
    
    private CancellationTokenSource searchCancelToken;

    public void OnEndEdit(string text)
    {
        if (searchText == text) return;
        searchText = text;
        LevelListScript.DefaultRequest.Query = text;
        LevelListScript.DefaultRequest.Offset = 0;
        LevelListScript.instance.ClearLevels();
        LevelListScript.instance.UpdateLevelList();
    }
    private string oldQuery = "";
    // public void OnSearchTextChange()
    // {
    //     if (searchField.text == oldQuery) return;
    //     searchCancelToken?.Cancel();
    //     searchCancelToken = new CancellationTokenSource();

    //     _ = DebouncedSearchAsync(searchField.text, searchCancelToken.Token);
    //     oldQuery = searchField.text;
    // }'
    public void Update()
    {
        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                searchField.Select();
            }
        }       
    }

    private async Task DebouncedSearchAsync(string query, CancellationToken token)
    {
        try
        {
            await Task.Delay(300, token);
            if (token.IsCancellationRequested) return;

            SearchScript.searchText = query;
            LevelListScript.DefaultRequest.Query = query;
            LevelListScript.DefaultRequest.Offset = 0;
            LevelListScript.instance.ClearLevels();
            LevelListScript.instance.UpdateLevelList();
        }
        catch (TaskCanceledException)
        {
        }
    }
}
