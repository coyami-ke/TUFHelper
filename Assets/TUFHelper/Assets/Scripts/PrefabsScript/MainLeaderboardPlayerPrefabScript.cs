using TMPro;
using Together.Utils;
using TUFHelper;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MainLeaderboardPlayerPrefabScript : MonoBehaviour
{
    public Image pfp;

    public TextMeshProUGUI playerName, generalScore, rankedScore, xacc, rank;

    public async void SetPlayerInfo(MainLeaderboardScript.MainLeaderboardPlayerJson info, int rankNumber)
    {
        playerName.text = info.Player?.Name ?? "Unknown";
        generalScore.text = info.GeneralScore.ToString("F2");
        rankedScore.text = info.RankedScore.ToString("F2");
        xacc.text = (info.AverageXAccuracy * 100).ToString("F2") + "%";
        rank.text = "#" + rankNumber;

        string pfpUrl = info.Player?.PFP;
        if (string.IsNullOrWhiteSpace(pfpUrl))
        {
            return;
        }

        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(pfpUrl);
        request.certificateHandler = new CertificateWhore();
        request.disposeCertificateHandlerOnDispose = true;

        var operation = request.SendWebRequest();
        while (!operation.isDone)
        {
            await System.Threading.Tasks.Task.Yield();
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            Main.Logger?.Error($"Failed to download leaderboard profile picture: {request.error}");
            return;
        }

        Texture2D texture = DownloadHandlerTexture.GetContent(request);
        if (texture == null)
        {
            return;
        }

        pfp.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
}
