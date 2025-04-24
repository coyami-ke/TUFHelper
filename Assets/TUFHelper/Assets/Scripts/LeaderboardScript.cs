using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public void Awake()
    {
        instance = this;
    }

    public string GetDefaultUrl(int levelID)
    {
        return $"https://api.tuforums.com/v2/database/passes/level/{levelID}";
    }
    public async void LoadPasses(int levelID)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(GetDefaultUrl(levelID));
        webRequest.certificateHandler = new CertificateWhore();

        var operation = webRequest.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Main.Logger.Error("Passes Request Error: " + webRequest.error);
            return;
        }

        List<PassesListInfoElementJson> passes;

        passes = JsonConvert.DeserializeObject<List<PassesListInfoElementJson>>(webRequest.downloadHandler.text);
        passes = passes.OrderByDescending(p => p.ScoreV2).ToList();

        Main.Logger.Log($"Passes: {passes.Count}");

        foreach (Transform child in passListParent.transform)
            Destroy(child.gameObject);

        int rank = 1;
        foreach (var pass in passes)
        {
            var rps = Instantiate(prefab).GetComponent<RankPrefabScript>();
            rps.SetPassInfo(pass, rank);

            RectTransform rect = rps.GetComponent<RectTransform>();
            rect.SetParent(passListParent.transform, false);
            rect.localScale = Vector3.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.sizeDelta = new Vector2(0, 60);
            rect.anchoredPosition = new Vector3(0, (rank - 1) * -75 - 30);
            rank++;
        }

        RectTransform contentRect = passListParent.GetComponent<RectTransform>();
        float totalHeight = (rank - 1) * 75 + 30;
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);
    }
}
