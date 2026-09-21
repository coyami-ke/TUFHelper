using System.Collections;
using System.Collections.Generic;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using UnityEngine;

public class LevelSelector : MonoBehaviour
{
    public static LevelSelector instance;

    public GameObject levelPrefab;
    public GameObject levelListParent;

    public LevelListInfoElementJson LevelInfo { get; set; }

    private bool _isShow;
    public bool IsShow
    {
        get => _isShow;
        set
        {
            _isShow = value;
            gameObject.SetActive(value);
        }
    }

    private void Awake()
    {
        instance = this;
        IsShow = false;
    }

    public void LoadLevels(List<string> levels, LevelListInfoElementJson info, string packId = null, int packLevelID = -1)
    {
        IsShow = true;

        for (int i = levelListParent.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(levelListParent.transform.GetChild(i).gameObject);
        }

        int count = 0;
        foreach (var levelPath in levels)
        {
            GameObject obj = Instantiate(levelPrefab);
            BundleFontFixer.FixFontsIn(obj);

            SelectLevelPrefabScript prefabScript = obj.GetComponent<SelectLevelPrefabScript>();
            prefabScript.SetLevel(levelPath, info, packId, packLevelID);

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.SetParent(levelListParent.transform, false);
            rect.anchoredPosition = new Vector2(0, count * -90f);

            count++;
        }

        RectTransform contentRect = levelListParent.GetComponent<RectTransform>();
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, count * 90f);
    }

    public IEnumerator LoadLevelsCo(List<string> levels, LevelListInfoElementJson levelInfo, string packId = null, int packLevelID = -1)
    {
        yield return new WaitUntil(() => TMPro.TMP_Settings.instance != null);
        yield return new WaitForEndOfFrame();

        LoadLevels(levels, levelInfo, packId, packLevelID);
    }

    public void Hide()
    {
        IsShow = false;
    }
}