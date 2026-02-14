using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using TUFHelper;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TagPrefabScript : MonoBehaviour, IPointerClickHandler
{
    public Image iconTag;
    public TextMeshProUGUI tagText;
    public string tagTUF;

    public Image backgroundImage;

    private Color selectedColor = new(1, 1, 1, 40 / 255f);
    private Color unselectedColor = new(1, 1, 1, 20 / 255f);

    private bool isSelected = false;
    public bool IsSelected
    {
        get => isSelected;
        private set
        {
            isSelected = value;
            backgroundImage.color = value ? selectedColor : unselectedColor;
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
            Main.Setting.SelectedQDiifs.Add(tagTUF);
            LevelListScript.DefaultRequest.TagsFilter.Add(tagTUF);
        }
        else
        {
            Main.Setting.SelectedQDiifs.Remove(tagTUF);
            LevelListScript.DefaultRequest.TagsFilter.Remove(tagTUF);
        }

        Main.Setting.Save(Main.ModEntry);
    }




    private bool isUpdating = false;

    private async Task UpdateLevelList()
    {
        if (isUpdating) return;
        isUpdating = true;

        LevelListScript.instance.ClearLevels();
        await LevelListScript.instance.UpdateLevelListAsync();

        Main.Logger.Log("Tags: " + string.Join(',', LevelListScript.DefaultRequest.TagsFilter));

        isUpdating = false;
    }


    public void SetTagInfo(bool selected, string tag, Sprite icon)
    {
        tagText.text = tag;
        iconTag.sprite = icon;
        tagTUF = tag;

        isSelected = selected;
        backgroundImage.color = selected ? selectedColor : unselectedColor;
    }
}
