using System.Collections;
using System.Collections.Generic;
using TMPro;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.UI;

public class IngameRatingVotePrefabScript : MonoBehaviour
{
    public Image pfp;
    public TextMeshProUGUI ratingText, commentText, managerText;

    public DifficultyDetail Detail { get; private set; }

    public async void SetVoteInfo(DifficultyDetail detail)
    {
        ratingText.text = detail.Rating;
        commentText.text = detail.Comment;
        managerText.text = detail.User.Nickname;

        var pfpData = await AccountScript.instance.TokenRequest.GetPfpFromURL(detail.User.AvatarUrl);
        if (pfpData == null) return;

        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(pfpData);

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        pfp.sprite = sprite;

        Detail = detail;
    }
}
