using DG.Tweening;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using TUFHelper;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TagsScript : MonoBehaviour, IPointerClickHandler
{
    public string pathToJSON;
    public RectTransform menuTransform;
    public RectTransform arrowTransfrom;

    public GameObject tagPrefab, groupPrefab;

    public Image backgroundImage;

    private float maxHeight;

    private const float selectedRotation = -90;
    private const float unselectedRotation = 90;

    private readonly Color selectedColor = new(165f / 255, 183f / 255, 255f / 255, 30f / 255f);
    private readonly Color unselectedColor = new(165f / 255, 183f / 255, 255f / 255, 20f / 255f);

    public float MenuWidth
    {
        get => menuTransform.sizeDelta.x;
    }

    private bool isSelected = false;
    public bool IsSelected
    {
        get { return isSelected; }
        set
        {
            isSelected = value;
            AnimateDropdown(value);
        }
    }
    public void Start()
    {
        menuTransform.sizeDelta = new(menuTransform.sizeDelta.x, 0);

        string json = (Main.assets.LoadAsset(TagSpriteHelper.PATH_TO_JSON) as TextAsset).text;
        List<TUFTag> tags = JsonConvert.DeserializeObject<List<TUFTag>>(json);

        Dictionary<string, List<TagPrefabScript>> scripts = new();

        foreach (var tag in tags)
        {
            var obj = Instantiate(tagPrefab, menuTransform);
            var script = obj.GetComponent<TagPrefabScript>();
            script.SetTagInfo(tag);
            if (!scripts.ContainsKey(tag.Group))
            {
                scripts.Add(tag.Group, new());
            }
            scripts[tag.Group].Add(script);
        }

        if (scripts.Count == 0) return;

        float currentPosX = 10;
        int currentRow = 0;
        float prefabHeight = (scripts.ToArray()[0].Value[0].transform as RectTransform).sizeDelta.y;
        float spacingX = 2.5f;
        float spacingY = 2.5f;

        int j = 0;
        foreach (var list in scripts)
        {
            var groupObj = Instantiate(groupPrefab, menuTransform);
            var groupScript = groupObj.GetComponent<TagGroupPrefabScript>();
            (groupObj.transform as RectTransform).AnchorPosY(-(currentRow * (prefabHeight + spacingY) + j * 60 + 30));
            groupScript.SetGroupInfo(list.Key.ToUpper());

            for (int i = 0; i < list.Value.Count; i++)
            {
                var current = list.Value[i];
                var rect = current.transform as RectTransform;

                if (currentPosX + current.Width > MenuWidth && currentPosX > 0)
                {
                    currentPosX = 10;
                    currentRow++;
                }

                rect.anchoredPosition = new Vector2(currentPosX, -(currentRow * (prefabHeight + spacingY)) - ((j + 1) * 60));

                currentPosX += current.Width + spacingX;
            }
            currentPosX = 10;
            j++;
        }
        float totalHeight = (currentRow + 1) * (prefabHeight + spacingY) + (j + 0.5f) * 60;
        menuTransform.sizeDelta = new(menuTransform.sizeDelta.x, 0);
        maxHeight = totalHeight;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        IsSelected = !IsSelected;
    }

    private async void AnimateDropdown(bool show)
    {
        backgroundImage.DOColor(show ? selectedColor : unselectedColor, 0.4f).SetEase(Ease.OutExpo);
        arrowTransfrom.DORotate(new Vector3(0f, 0f, show ? selectedRotation : unselectedRotation), 0.4f).SetEase(Ease.OutExpo);
        //menuTransform.DOScaleY(show ? 1f : 0f, 0.4f).SetEase(Ease.OutExpo);
        menuTransform.DOSizeDelta(new(menuTransform.sizeDelta.x, show ? maxHeight : 0), 0.4f).SetEase(Ease.OutExpo);
    }
}
