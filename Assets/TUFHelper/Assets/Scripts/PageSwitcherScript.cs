using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PageSwitcherScript : MonoBehaviour
{

    public static PageSwitcherScript instance;

    public Button prevPageButton, nextPageButton;
    public TextMeshProUGUI pageText;

    public int currentPage = 1, totalPages = 1;

    public void Awake()
    {
        instance = this;
    }

    public void Update()
    {
        totalPages = (LevelSelectScript.availableLevels / 6) + (LevelSelectScript.availableLevels % 6 == 0 ? 0 : 1);
        currentPage = Math.Max(Math.Min(currentPage, totalPages), 1);

        pageText.text = currentPage + " / " + totalPages;
    }

    public void PrevPageButtonClick()
    {
        currentPage = Math.Max(currentPage - 1, 1);
        StartCoroutine(LevelSelectScript.instance.LoadLevelListCo());
    }

    public void NextPageButtonClick()
    {
        currentPage = Math.Min(currentPage + 1, totalPages);
        StartCoroutine(LevelSelectScript.instance.LoadLevelListCo());
    }

}
