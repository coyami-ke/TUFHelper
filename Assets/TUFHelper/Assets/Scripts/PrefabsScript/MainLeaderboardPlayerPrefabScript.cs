using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

public class MainLeaderboardPlayerPrefabScript : MonoBehaviour
{
    public Image pfp;

    public TextMeshProUGUI playerName, generalScore, rankedScore, xacc, rank;

    public async void SetPlayerInfo(MainLeaderboardScript.MainLeaderboardPlayerJson info)
    {
        playerName.text = info.Player.Name;
        generalScore.text = info.GeneralScore.ToString("F2");
        rankedScore.text = info.RankedScore.ToString("F2");
        xacc.text = (info.AverageXAccuracy * 100).ToString("F2") + "%";
        rank.text = "#" + info.GeneralScoreRank;

        var pfpData = await AccountScript.instance.TokenRequest.GetPfpFromURL(info.Player.PFP);
        if (pfpData == null) return;

        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(pfpData);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        pfp.sprite = sprite;
    }
}
