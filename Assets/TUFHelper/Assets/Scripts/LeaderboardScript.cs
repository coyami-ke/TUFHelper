using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Together.Utils;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using UnityEngine;
using UnityEngine.Networking;

public class LeaderboardScript : MonoBehaviour
{
    public GameObject scrollableParent, prefab, passListParent;
    public static LeaderboardScript instance;

    private CancellationTokenSource currentRequestToken;

    private void Awake()
    {
        instance = this;
    }

    public string GetDefaultUrl(int levelID) => $"https://api.tuforums.com/v2/database/passes/level/{levelID}";

    public async void LoadPasses(int levelID)
    {
        // Cancel any ongoing request
        currentRequestToken?.Cancel();
        currentRequestToken = new CancellationTokenSource();
        CancellationToken token = currentRequestToken.Token;

        string url = GetDefaultUrl(levelID);
        using UnityWebRequest webRequest = UnityWebRequest.Get(url);
        webRequest.certificateHandler = new CertificateWhore();
        webRequest.timeout = 10;

        var operation = webRequest.SendWebRequest();
        while (!operation.isDone)
        {
            await Task.Yield();
            if (token.IsCancellationRequested)
            {
                webRequest.Abort(); // Stop the request
                return;
            }
        }

        if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
            return;

        List<PassesListInfoElementJson> passes = JsonConvert.DeserializeObject<List<PassesListInfoElementJson>>(webRequest.downloadHandler.text);
        passes = passes.OrderByDescending(p => p.ScoreV2).ToList();

        foreach (Transform child in passListParent.transform)
            Destroy(child.gameObject);

        int rank = 1;
        foreach (var pass in passes)
        {
            GameObject obj = Instantiate(prefab, passListParent.transform);
            RectTransform rect = obj.GetComponent<RectTransform>();

            var rps = obj.GetComponent<RankPrefabScript>();
            rps.SetPassInfo(pass, rank);

            rect.localScale = Vector3.one;
            rect.sizeDelta = new Vector2(0, 60);
            rect.anchoredPosition = new Vector2(0, (rank - 1) * -75 - 30);
            rank++;
        }

        RectTransform contentRect = passListParent.GetComponent<RectTransform>();
        float totalHeight = (rank - 1) * 75 + 30;
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);
    }
}
