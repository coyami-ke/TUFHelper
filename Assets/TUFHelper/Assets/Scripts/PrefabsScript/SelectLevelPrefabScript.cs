using DG.Tweening;
using TMPro;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Path = System.IO.Path;

public class SelectLevelPrefabScript : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI levelName;
    public Image background;

    private string _fullPath;
    private LevelListInfoElementJson _levelInfo;
    private string _packID;
    private int _packLevelID = -1;

    public void OnPointerClick(PointerEventData eventData)
    {
        ADOFAIGameplayHandler.LaunchLevel(_fullPath, _levelInfo, _packID, _packLevelID);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        background.DOColor(new Color(1f, 1f, 1f, 20f / 255f), 0.4f).SetEase(Ease.OutExpo);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        background.DOColor(new Color(1f, 1f, 1f, 10f / 255f), 0.4f).SetEase(Ease.OutExpo);
    }

    public void SetLevel(string path, LevelListInfoElementJson info, string packId = null, int packLevelId = -1)
    {
        _fullPath = path;
        _levelInfo = info;
        _packID = packId;
        _packLevelID = packLevelId;

        levelName.text = Path.GetFileName(path);
        LanguageManager.ApplyChineseJapaneseFont(levelName);
    }
}