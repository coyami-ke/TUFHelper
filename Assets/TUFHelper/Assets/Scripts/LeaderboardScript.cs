using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using Together.Utils;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using TUFHelper;

public class LeaderboardScript : MonoBehaviour
{

    public RectTransform contentRect;
    public GameObject loadingText, rankPrefab;

    public static List<PassInfo> passes = new List<PassInfo>();

    public void Awake()
    {
        passes.Clear();

        StartCoroutine(RequestAllPasses());
    }

    public void Update()
    {
        
    }

    public IEnumerator RequestAllPasses()
    {
        UnityWebRequest www = UnityWebRequest.Get("https://be.t21c.kro.kr/passes?levelId=" + LevelInfoSceneScript.currentLevelInfo.id);
        www.certificateHandler = new CertificateWhore();

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Passes Request Error: " + www.error);
        }
        else
        {
            JObject jo = JsonConvert.DeserializeObject<JObject>(www.downloadHandler.text);
            JArray ja = jo.Value<JArray>("results");

            passes = JsonConvert.DeserializeObject<List<PassInfo>>(ja.ToString()).OrderByDescending(x => x.getXAcc()).ToList();

            StartCoroutine(LoadLevelPassesCo());
        }
    }

    public IEnumerator LoadLevelPassesCo()
    {
        yield return new WaitForEndOfFrame();

        loadingText.SetActive(false);

        for (int i = 0; i < contentRect.transform.childCount; i++)
        {
            Destroy(contentRect.transform.GetChild(i).gameObject);
        }

        List<string> addedPlayers = new List<string>();

        int rank = 1;
        foreach (PassInfo pi in passes)
        {
            if (addedPlayers.Contains(pi.player))
            {
                continue;
            }
            addedPlayers.Add(pi.player);

            if (!Main.playerData.ContainsKey(pi.player))
            {
                continue;
            }

            PlayerInfo playerInfo = Main.playerData[pi.player];
            if (playerInfo.isBanned)
            {
                continue;
            }

            RankPrefabScript rps = Instantiate(rankPrefab).GetComponent<RankPrefabScript>();
            rps.SetPassInfo(rank, playerInfo.country, pi);

            RectTransform rect = rps.GetComponent<RectTransform>();
            rect.SetParent(contentRect.transform, false);
            rect.localScale = Vector3.one;
            rect.offsetMin = new Vector2(0, 0);
            rect.offsetMax = new Vector2(0, 0);
            rect.sizeDelta = new Vector2(0, 60);
            rect.anchoredPosition = new Vector3(0, (rank - 1) * -60);

            rank++;
        }

        contentRect.sizeDelta = new Vector2(0, 60 * (rank - 1));
    }
}
