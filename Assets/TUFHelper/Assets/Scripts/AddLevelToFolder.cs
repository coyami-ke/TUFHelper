using System.Collections;
using System.Collections.Generic;
using TMPro;
using TUFHelper;
using UnityEngine;

public class AddLevelToFolder : MonoBehaviour
{
    public static AddLevelToFolder instance { get; private set; }

    public GameObject folderPrefab, listParent, verticalScroll;

    private bool isShow;
    public bool IsShow
    {
        get => isShow;
        set
        {
            isShow = value;
            gameObject.SetActive(value);
        }
    }

    public void SetInfo(int levelID)
    {
        IsShow = true;
        for (int i = 0; i < listParent.transform.childCount; i++)
        {
            Destroy(listParent.transform.GetChild(i).gameObject);
        }

        
        int count = 0;
        foreach (var folder in Main.Setting.LevelFolders)
        {
            GameObject obj = Instantiate(folderPrefab);
            BundleFontFixer.FixFontsIn(obj);
            obj.GetComponent<SelectFolderPrefabScript>().SetFolderInfo(folder, levelID);

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.SetParent(listParent.transform);

            rect.localScale = Vector3.one;
            rect.offsetMin = new Vector2(0, 0);
            rect.offsetMax = new Vector2(0, 0);
            rect.sizeDelta = new Vector2(0, 120);
            rect.sizeDelta = new(400, 62.5f);
            rect.anchoredPosition = new Vector3(0, (count * - 62.5f) -50);

            count++;
        }

        RectTransform contentRect = listParent.GetComponent<RectTransform>();
        float totalHeight = count * 62.5f + 70;
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);
    }

    public void Awake()
    {
        if (instance == null) instance = this;
    }
    public void Start()
    {
        IsShow = false;
    }
    public void Hide()
    {
        IsShow = false;
    }
}
