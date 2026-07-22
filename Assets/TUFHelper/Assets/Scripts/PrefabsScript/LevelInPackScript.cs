using DG.Tweening;
using TMPro;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelInPackScript : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image diffIconImage, backgroundImage;
    public TextMeshProUGUI levelNameText, artistText, creatorText, idText, clearsText, likesText;

    public LevelListInfoElementJson LevelInfo { get; private set; }

    public void SetLevelInfo(PackItemNode node)
    {
        if (!node.IsLevel) return;

        levelNameText.text = node.ReferencedLevel.Song;
        artistText.text = node.ReferencedLevel.Artist;
        creatorText.text = node.ReferencedLevel.Creator;
        diffIconImage.sprite = Main.GetSpriteFromAssets(DiffSpriteHelper.GetSpriteFromId(node.ReferencedLevel.DiffId));
        idText.text = "#" + node.ReferencedLevel.ID;
        clearsText.text = "Clears: " + node.ReferencedLevel.Clears;
        likesText.text = node.ReferencedLevel.Likes.ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        backgroundImage.DOColor(new(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, 30 / 255f), 0.5f).SetEase(Ease.OutExpo);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        backgroundImage.DOColor(new(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, 10 / 255f), 0.5f).SetEase(Ease.OutExpo);
    }
}
