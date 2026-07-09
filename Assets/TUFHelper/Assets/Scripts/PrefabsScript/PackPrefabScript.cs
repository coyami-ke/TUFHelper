using System.Threading.Tasks;
using TMPro;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PackPrefabScript : MonoBehaviour
{
    public Image backgroundImage, iconImage, pfpImage;
    public TextMeshProUGUI nameText, levelsNumberText, favoritesText, nicknameText, hasMoreText;

    public void SetPackInfo(PackListElementJson info)
    {
        nameText.text = info.Name;
        levelsNumberText.text = info.LevelCount + " level(s)";
        favoritesText.text = info.FavoritesCount.ToString();
        nicknameText.text = info.PackOwner.Nickname;

        int remindedLevels = Mathf.Max(0, info.LevelCount - 3);

        if (remindedLevels > 0)
        {
            hasMoreText.text = $"+{remindedLevels} more";
        }
        else
        {
            hasMoreText.text = "";
        }

        LoadProfilePicture(info.PackOwner.AvatarURL);
    }

    public async void LoadProfilePicture(string url)
    {
        byte[] imageData = await GetImageFromURL(url);
        if (imageData == null) return;

        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageData);

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        pfpImage.sprite = sprite;
    }

    public async void LoadIconPicture(string url)
    {
        byte[] imageData = await GetImageFromURL(url);
        if (imageData == null) return;

        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageData);

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        iconImage.sprite = sprite;
    }

    public async Task<byte[]> GetImageFromURL(string url)
    {
        using UnityWebRequest www = UnityWebRequest.Get(url);
        var operation = www.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Main.Logger.Error($"Failed to download profile picture: {www.error}");
            return null;
        }

        return www.downloadHandler.data;
    }
}
