using System;
using TMPro;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.UI;

public class VisualLevelPrefab : MonoBehaviour
{
    public Image backgroundImage, leftRectangleImage, diffIconImage;
    public TextMeshProUGUI artistAndSongText, creatorText, clearsText, likesText;

    public void SetLevelInfo(LevelListInfoElementJson levelInfo)
    {
        try
        {
            artistAndSongText.text = artistAndSongText.text = $"<b>{levelInfo.Artist}</b> - {levelInfo.Song}";
            creatorText.text = levelInfo.Creator;
            clearsText.text = levelInfo.Clears.ToString();
            likesText.text = levelInfo.Likes.ToString();
            diffIconImage.sprite = Main.GetSpriteFromAssets(DiffSpriteHelper.GetSpriteFromId(levelInfo.DiffId));

            Color _diffColor;
            if (ColorUtility.TryParseHtmlString(levelInfo.Difficulty?.Color, out Color parsedColor))
            {
                _diffColor = parsedColor;
            }
            else
            {
                _diffColor = Color.white;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = new Color(_diffColor.r, _diffColor.g, _diffColor.b, 20f / 255f);
            }

            if (leftRectangleImage != null)
            {
                leftRectangleImage.color = new Color(_diffColor.r, _diffColor.g, _diffColor.b, 128f / 255f);
            }
        }
        catch (Exception ex)
        {
            Main.Logger.LogException(ex);
        }
    }
}
