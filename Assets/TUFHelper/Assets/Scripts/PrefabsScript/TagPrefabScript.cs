using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using TUFHelper;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TagPrefabScript : MonoBehaviour, IPointerClickHandler
{
    public Image iconTag;
    public TextMeshProUGUI tagText;
    public TUFTag tagTUF;

    public Image backgroundImage;

    public float Width
    {
        get 
        {
            var rect = transform as RectTransform;
            return rect.sizeDelta.x;
        }
    }

    private bool isSelected = false;
    public bool IsSelected
    {
        get => isSelected;
        private set
        {
            isSelected = value;
            if (value)
            {
                backgroundImage.DOFade(0.4f, 0.22f).SetEase(Ease.OutCubic);
            }
            else
            {
                backgroundImage.DOFade(0.2f, 0.22f).SetEase(Ease.OutCubic);
            }
        }
    }
    public async void OnPointerClick(PointerEventData eventData)
    {
        bool newValue = !IsSelected;

        ApplySelection(newValue);

        await UpdateLevelList();
    }

    private void ApplySelection(bool value)
    {
        IsSelected = value;

        if (value)
        {
            Main.Setting.SelectedTags.Add(tagTUF.Name);
            LevelListScript.DefaultRequest.TagsFilter.Add(tagTUF.Name);
        }
        else
        {
            Main.Setting.SelectedTags.Remove(tagTUF.Name);
            LevelListScript.DefaultRequest.TagsFilter.Remove(tagTUF.Name);
        }

        Main.Setting.Save(Main.ModEntry);
    }

    private async Task UpdateLevelList()
    {

        LevelListScript.instance.ClearLevels();
        await LevelListScript.instance.UpdateLevelListAsync();
    }


    public void SetTagInfo(TUFTag tag)
    {
        tagText.text = tag.Name;
        iconTag.sprite = TagSpriteHelper.GetSpriteFromTag(tag.Name.Replace('/', ' '));
        tagTUF = tag;

        Color color;
        ColorUtility.TryParseHtmlString(tag.Color, out color);

        backgroundImage.color = color;

        IsSelected = Main.Setting.SelectedTags.Contains(tag.Name);
        var rect = GetComponent<RectTransform>();

        Vector2 newSize = new(tagText.preferredWidth + 30, rect.sizeDelta.y);
        rect.sizeDelta = newSize;
    }
}
