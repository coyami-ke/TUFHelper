using System;
using System.Net.Http;
using System.Threading;
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
        if (info.PackOwner.Nickname != null) nicknameText.text = info.PackOwner.Nickname;
        else nicknameText.text = info.PackOwner.Username;

        int remindedLevels = Mathf.Max(0, info.LevelCount - 3);

        if (remindedLevels > 0)
        {
            hasMoreText.text = $"+{remindedLevels} more";
        }
        else
        {
            hasMoreText.text = "";
        }

        if (info.PackOwner != null && !string.IsNullOrEmpty(info.PackOwner.AvatarURL)) LoadProfilePicture(info.PackOwner.AvatarURL);
        if (!string.IsNullOrEmpty(info.IconURL)) LoadIconPicture(info.IconURL);
    }

    public async void LoadProfilePicture(string url)
    {
        byte[] imageData = await GetImageFromURL(url);
        if (imageData == null) return;

        // Create the sprite asynchronously using our background-threaded helper
        Sprite sprite = await CreateSpriteAsync(imageData);
        if (sprite != null && pfpImage != null)
        {
            pfpImage.sprite = sprite;
        }
    }

    public async void LoadIconPicture(string url)
    {
        byte[] imageData = await GetImageFromURL(url);
        if (imageData == null) return;

        // Create the sprite asynchronously using our background-threaded helper
        Sprite sprite = await CreateSpriteAsync(imageData);
        if (sprite != null && iconImage != null)
        {
            iconImage.sprite = sprite;
        }
    }

    private async Task<Sprite> CreateSpriteAsync(byte[] imageData)
    {
        Texture2D texture = new Texture2D(2, 2);

        // 2. Load the raw PNG data into the texture
        if (texture.LoadImage(imageData))
        {
            Rect rect = new Rect(0, 0, texture.width, texture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);

            return Sprite.Create(texture, rect, pivot);
        }

        Debug.LogError("Failed to load PNG byte array into Texture2D.");
        return null;
    }

    private async Task<Sprite> DownloadSpriteAsync(string url)
    {
        if (string.IsNullOrEmpty(url)) return null;

        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);

        request.certificateHandler = new Together.Utils.CertificateWhore();
        request.disposeCertificateHandlerOnDispose = true;

        try
        {
            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Main.Logger.Error($"[ImageDownloader] Error loading image from {url}: {request.error}");
                return null;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            if (texture == null) return null;

            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        catch (Exception ex)
        {
            Main.Logger.Error($"[ImageDownloader] Unexpected exception: {ex.Message}");
            return null;
        }
    }

    public async Task<byte[]> GetImageFromURL(string url, CancellationToken token = default)
    {
        try
        {
            HttpResponseMessage response = await Main.Client.GetAsync(url, HttpCompletionOption.ResponseContentRead, token);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (HttpRequestException ex)
        {
            Main.Logger.Error($"Failed to download profile picture: {ex.Message}");
            return null;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (Exception ex)
        {
            Main.Logger.Error($"Unexpected error downloading profile picture: {ex.Message}");
            return null;
        }
    }
}
