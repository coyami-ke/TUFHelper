using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using TMPro;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectLevelPrefabScript : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI levelName;
    public Image background;

    private string fullPath;
    private LevelListInfoElementJson levelInfo;
    private string packID;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(packID)) UIScript.SwipeToBlack(() => ADOFAIGameplayHandler.OpenLevel(fullPath, levelInfo));
        else UIScript.SwipeToBlack(() => ADOFAIGameplayHandler.OpenLevel(fullPath, levelInfo, packID));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        background.DOColor(new Color(1, 1, 1, 20 / 255f), 0.4f).SetEase(Ease.OutExpo);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        background.DOColor(new Color(1, 1, 1, 10 / 255f), 0.4f).SetEase(Ease.OutExpo);
    }

    public void SetLevel(string levelName, LevelListInfoElementJson info, string packId)
    {
        this.levelName.text = Path.GetFileName(levelName);
        LanguageManager.ApplyChineseJapaneseFont(this.levelName);
        fullPath = levelName;
        levelInfo = info;

        packID = packId;
    }
}
