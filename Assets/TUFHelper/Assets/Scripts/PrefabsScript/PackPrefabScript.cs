using System;
using System.Net.Http;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using Newtonsoft.Json;
using TMPro;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using TUFHelper.ModScripts.Web;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PackPrefabScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image backgroundImage, iconImage, pfpImage;
    public TextMeshProUGUI nameText, levelsNumberText, favoritesText, nicknameText, hasMoreText;

    public PackPefab_ReferencedLevelScript level1, level2, level3;

    public PackListElementJson PackInfo { get; private set; }

    public void SetPackInfo(PackListElementJson info)
    {
        PackInfo = info;

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

        if (info.PackItems.Length >= 1)
        {
            level1.gameObject.SetActive(true);
            level1.SetLevelInfo(info.PackItems[0]);
        }
        else level1.gameObject.SetActive(false);
        if (info.PackItems.Length >= 2)
        {
            level2.SetLevelInfo(info.PackItems[1]);
            level2.gameObject.SetActive(true);
        }
        else level2.gameObject.SetActive(false);
        if (info.PackItems.Length >= 3)
        {
            level3.SetLevelInfo(info.PackItems[2]);
            level3.gameObject.SetActive(true);
        }
        else level3.gameObject.SetActive(false);

        if (info.PackOwner != null && !string.IsNullOrEmpty(info.PackOwner.AvatarURL)) LoadProfilePicture(info.PackOwner.AvatarURL);
        if (!string.IsNullOrEmpty(info.IconURL)) LoadIconPicture(info.IconURL);
    }

    public async void LoadProfilePicture(string url)
    {
        byte[] imageData = await GetImageFromURL(url);
        if (imageData == null) return;

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

        Sprite sprite = await CreateSpriteAsync(imageData);
        if (sprite != null && iconImage != null)
        {
            iconImage.sprite = sprite;
        }
    }

    private async Task<Sprite> CreateSpriteAsync(byte[] imageData)
    {
        Texture2D texture = new Texture2D(2, 2);

        if (texture.LoadImage(imageData))
        {
            Rect rect = new Rect(0, 0, texture.width, texture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);

            return Sprite.Create(texture, rect, pivot);
        }

        Main.Logger.Error("Failed to load PNG byte array into Texture2D.");
        return null;
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        backgroundImage.DOColor(new Color(1, 1, 1, 22f / 255), 0.25f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        backgroundImage.DOColor(new Color(1, 1, 1, 10f / 255), 0.25f);
    }

    public async void OnPointerClick(PointerEventData eventData)
    {
        try 
        {
            HttpResponseMessage response = await Main.Client.GetAsync($"{TUFAPIRequest_Packs.DEFAULT_URL}/{PackInfo.ID}?tree=true");
            string json = await response.Content.ReadAsStringAsync();

            PackRootJson pack = JsonConvert.DeserializeObject<PackRootJson>(json);

            ProcessNode(pack.Items[0]);
        }
        catch (HttpRequestException ex)
        {
            Main.Logger.Error($"[TUFAPIRequest] Network HTTP failure: {ex.Message}");
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Main.Logger.Error($"[TUFAPIRequest] Unexpected error: {ex.Message}");
            throw;
        }
    }

    private void ProcessNode(PackItemNode node, int depth = 0)
    {
        string indent = new string('-', depth * 2);

        if (node.IsFolder)
        {
            Main.Logger.Log($"{indent}[Folder] Name: {node.Name} (ID: {node.Id})");

            foreach (var childNode in node.Children)
            {
                ProcessNode(childNode, depth + 1);
            }
        }
        else if (node.IsLevel)
        {
            var track = node.ReferencedLevel;
            Main.Logger.Log($"{indent}[Level] Track: {track?.Artist} - {track?.Song} (Diff: {track?.DiffID})");
        }
    }
}
