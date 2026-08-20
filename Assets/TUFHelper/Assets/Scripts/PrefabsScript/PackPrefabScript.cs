using DG.Tweening;
using System;
using System.Threading;
using TMPro;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PackPrefabScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image backgroundImage, iconImage, pfpImage;
    public TextMeshProUGUI nameText, levelsNumberText, favoritesText, nicknameText, hasMoreText;

    public PackPefab_ReferencedLevelScript level1, level2, level3;

    private CancellationTokenSource imageCancelToken;

    public PackListElementJson PackInfo { get; private set; }

    public void SetPackInfo(PackListElementJson info)
    {
        PackInfo = info;

        nameText.text = info.Name;
        levelsNumberText.text = info.LevelCount + " level(s)";
        favoritesText.text = info.FavoritesCount.ToString();

        if (info.PackOwner != null)
        {
            nicknameText.text = !string.IsNullOrEmpty(info.PackOwner.Nickname)
                ? info.PackOwner.Nickname
                : info.PackOwner.Username;
        }

        int remindedLevels = Mathf.Max(0, info.LevelCount - 3);
        hasMoreText.text = remindedLevels > 0 ? $"+{remindedLevels} more" : "";

        if (info.PackItems != null && info.PackItems.Length >= 1)
        {
            level1.gameObject.SetActive(true);
            level1.SetLevelInfo(info.PackItems[0]);
        }
        else level1.gameObject.SetActive(false);

        if (info.PackItems != null && info.PackItems.Length >= 2)
        {
            level2.gameObject.SetActive(true);
            level2.SetLevelInfo(info.PackItems[1]);
        }
        else level2.gameObject.SetActive(false);

        if (info.PackItems != null && info.PackItems.Length >= 3)
        {
            level3.gameObject.SetActive(true);
            level3.SetLevelInfo(info.PackItems[2]);
        }
        else level3.gameObject.SetActive(false);

        imageCancelToken?.Cancel();
        imageCancelToken = new CancellationTokenSource();

        if (info.PackOwner != null && !string.IsNullOrEmpty(info.PackOwner.AvatarURL))
        {
            _ = ImageUtils.LoadImageToUIAsync(info.PackOwner.AvatarURL, pfpImage, imageCancelToken.Token);
        }

        if (!string.IsNullOrEmpty(info.IconURL))
        {
            _ = ImageUtils.LoadImageToUIAsync(info.IconURL, iconImage, imageCancelToken.Token);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        backgroundImage.DOColor(new Color(1, 1, 1, 22f / 255f), 0.25f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        backgroundImage.DOColor(new Color(1, 1, 1, 10f / 255f), 0.25f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        try
        {
            PackListScript.Instance.ShowPackView();
            PackListScript.Instance.SetPackInfo(PackInfo, pfpImage != null ? pfpImage.sprite : null, iconImage != null ? iconImage.sprite : null);
        }
        catch (Exception ex)
        {
            Main.Logger.Error($"[PackPrefabScript] Error opening pack view: {ex.Message}");
        }
    }

    private void OnDestroy()
    {
        imageCancelToken?.Cancel();
    }
}