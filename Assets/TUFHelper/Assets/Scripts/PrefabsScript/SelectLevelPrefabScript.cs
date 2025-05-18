using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectLevelPrefabScript : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI levelName;
    public Image background;

    private string fullPath;

    public void OnPointerClick(PointerEventData eventData)
    {
        UIScript.SwipeToBlack(() => LevelPrefabScript.TryToLoadLevel(fullPath));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        background.DOColor(new Color(1, 1, 1, 20 / 255f), 0.4f).SetEase(Ease.OutExpo);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        background.DOColor(new Color(1, 1, 1, 10 / 255f), 0.4f).SetEase(Ease.OutExpo);
    }

    public void SetLevel(string levelName)
    {
        this.levelName.text = Path.GetFileName(levelName);
        fullPath = levelName;
    }
}
