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
    public static bool isShowingCleared = true, isShowingUncleared = true;

    public TextMeshProUGUI orderByIDButtonText, orderByDifficultyButtonText, orderByClearsButtonText;
    public Image orderByIDIcon, orderByDifficultyIcon, orderByClearsIcon;
    public TMP_InputField searchField;
    public Toggle showClearedToggle, showUnclearedToggle;

    public void Awake()
    {
        instance = this;
        searchField.text = searchText;
    }

    public void Update()
    {
        orderByIDButtonText.color = Main.Setting.orderMode == 0 ? Color.white : new Color(1, 1, 1, 128 / 256f);
        orderByDifficultyButtonText.color = Main.Setting.orderMode == 1 ? Color.white : new Color(1, 1, 1, 128 / 256f);
        orderByClearsButtonText.color = Main.Setting.orderMode == 2 ? Color.white : new Color(1, 1, 1, 128 / 256f);
        orderByIDIcon.transform.localScale = new Vector2(1, Main.Setting.orderByIDMode);
        orderByDifficultyIcon.transform.localScale = new Vector2(1, Main.Setting.orderByDifficultyMode);
        orderByClearsIcon.transform.localScale = new Vector2(1, Main.Setting.orderByClearsMode);
    }

    public void OnSearchTextChange()
    {
        LevelListScript.instance.UpdateLevelList(searchField.text);
    }

    public void OrderByIDButtonClick()
    {

        if (Main.Setting.orderMode == 0)
        {
            Main.Setting.orderByIDMode = -Main.Setting.orderByIDMode;
        }
        Main.Setting.orderMode = 0;
        Main.Setting.Save(Main.ModEntry);

        LevelListScript.instance.SortLevelList();
        LevelListScript.instance.LoadLevelList();
    }

    public void OrderByDifficultyButtonClick()
    {

        if (Main.Setting.orderMode == 1)
        {
            Main.Setting.orderByDifficultyMode = -Main.Setting.orderByDifficultyMode;
        }
        Main.Setting.orderMode = 1;
        Main.Setting.Save(Main.ModEntry);

        LevelListScript.instance.SortLevelList();
        LevelListScript.instance.LoadLevelList();
    }
    public void OrderByClearsButtonClick()
    {

        if (Main.Setting.orderMode == 2)
        {
            Main.Setting.orderByClearsMode = -Main.Setting.orderByClearsMode;
        }
        Main.Setting.orderMode = 2;
        Main.Setting.Save(Main.ModEntry);
        
        LevelListScript.instance.SortLevelList();
        LevelListScript.instance.LoadLevelList();
    }

    public void ShowClearedToggle()
    {

        isShowingCleared = showClearedToggle.isOn;
        LevelListScript.instance.LoadLevelList();
    }

    public void ShowUnclearedToggle()
    {

        isShowingUncleared = showUnclearedToggle.isOn;
        LevelListScript.instance.LoadLevelList();
    }
}
