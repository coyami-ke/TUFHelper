using System.Collections;
using System.Collections.Generic;
using TMPro;
using TUFHelper;
using UnityEngine;
using UnityEngine.UI;

public class SearchScript : MonoBehaviour
{

    public static SearchScript instance;
    public static string searchText = "";

    public TextMeshProUGUI orderByIDButtonText, orderByDifficultyButtonText;
    public TMP_InputField searchField;
    public Image orderByIDIcon, orderByDifficultyIcon;

    public void Awake()
    {
        instance = this;
        searchField.text = searchText;
    }

    public void Update()
    {
        orderByIDButtonText.color = Main.Setting.orderMode == 0 ? Color.white : new Color(1, 1, 1, 128 / 256f);
        orderByDifficultyButtonText.color = Main.Setting.orderMode == 1 ? Color.white : new Color(1, 1, 1, 128 / 256f);
        orderByIDIcon.transform.localScale = new Vector2(1, Main.Setting.orderByIDMode);
        orderByDifficultyIcon.transform.localScale = new Vector2(1, Main.Setting.orderByDifficultyMode);
    }

    public void OnSearchTextChange()
    {
        PageSwitcherScript.currentPage = 1;
        searchText = searchField.text;
        StartCoroutine(LevelListScript.instance.LoadLevelListCo());
    }

    public void OrderByIDButtonClick()
    {
        PageSwitcherScript.currentPage = 1;

        if (Main.Setting.orderMode == 0)
        {
            Main.Setting.orderByIDMode = -Main.Setting.orderByIDMode;
        }
        Main.Setting.orderMode = 0;
        Main.Setting.Save(Main.ModEntry);

        LevelListScript.instance.SortLevelList();
        StartCoroutine(LevelListScript.instance.LoadLevelListCo());
    }

    public void OrderByDifficultyButtonClick()
    {
        PageSwitcherScript.currentPage = 1;

        if (Main.Setting.orderMode == 1)
        {
            Main.Setting.orderByDifficultyMode = -Main.Setting.orderByDifficultyMode;
        }
        Main.Setting.orderMode = 1;
        Main.Setting.Save(Main.ModEntry);

        LevelListScript.instance.SortLevelList();
        StartCoroutine(LevelListScript.instance.LoadLevelListCo());
    }
}
