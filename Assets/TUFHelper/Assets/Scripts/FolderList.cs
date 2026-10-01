using System.Collections;
using System.Collections.Generic;
using TUFHelper;
using UnityEngine;
using UnityEngine.UI;

public class FolderList : MonoBehaviour
{
    public GameObject folderPrefab, listParent, verticalScroll;
    public ScrollRect VerticalScrollComponent { get; private set; }

    private bool isShow;
    public bool IsShow
    {
        get => isShow;
        set
        {
            isShow = value;
            this.gameObject.SetActive(value);
        }
    }

    public static FolderList instance { get; private set; }

    public void Awake()
    {
        if (instance == null) instance = this;

        VerticalScrollComponent = verticalScroll.GetComponent<ScrollRect>();
    }

    public void UpdateFolderList()
    {
        for (int i = 0; i < listParent.transform.childCount; i++)
        {
            Destroy(listParent.transform.GetChild(i).gameObject);
        }

        verticalScroll.GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;

        int j = 0;
        foreach (var folder in Main.Setting.LevelFolders)
        {
            GameObject gameObject = Instantiate(folderPrefab);
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(listParent.transform, false);
            rect.localScale = Vector3.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.sizeDelta = new Vector2(0, 120);

            rect.anchoredPosition = new Vector3(0, (j * -125) - 90);

            LevelFolderPrefabScript lps = gameObject.GetComponent<LevelFolderPrefabScript>();
            lps.SetFolderInfo(folder);

            j++;
        }
    }
}
