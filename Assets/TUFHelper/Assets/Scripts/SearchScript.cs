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
    public static bool isShowingFavourites = false;

    public TextMeshProUGUI orderByIDButtonText, orderByDifficultyButtonText;
    public Image orderByIDIcon, orderByDifficultyIcon;
    public TMP_InputField searchField;
    public Toggle showFavToggle;

    public void Awake()
    {
        instance = this;
        searchField.text = searchText;
        showFavToggle.isOn = isShowingFavourites;
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
        PageSwitcherScript.cachedPage = 1;
        searchText = searchField.text;
        StartCoroutine(LevelListScript.instance.LoadLevelListCo());
    }

    public void OrderByIDButtonClick()
    {
        PageSwitcherScript.cachedPage = 1;

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
        PageSwitcherScript.cachedPage = 1;

        if (Main.Setting.orderMode == 1)
        {
            Main.Setting.orderByDifficultyMode = -Main.Setting.orderByDifficultyMode;
        }
        Main.Setting.orderMode = 1;
        Main.Setting.Save(Main.ModEntry);

        LevelListScript.instance.SortLevelList();
        StartCoroutine(LevelListScript.instance.LoadLevelListCo());
    }

    public void ShowFavToggle()
    {
        PageSwitcherScript.cachedPage = 1;

        isShowingFavourites = showFavToggle.isOn;
        StartCoroutine(LevelListScript.instance.LoadLevelListCo());
    }
}
