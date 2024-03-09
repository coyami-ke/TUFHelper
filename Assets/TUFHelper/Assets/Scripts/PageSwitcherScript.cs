using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PageSwitcherScript : MonoBehaviour
{

    public static PageSwitcherScript instance;
    public static int currentPage = 1, totalPages = 1;

    public static int cachedPage = 1; // go to this page after coming back from editor

    public Button prevPageButton, nextPageButton;
    public TMP_InputField currentPageField;
    public TextMeshProUGUI totalPagesText;


    public void Awake()
    {
        instance = this;
    }

    public void Update()
    {
        if (LevelListScript.availableLevels > 0)
        {
            if (cachedPage != currentPage)
            {
                currentPage = cachedPage;
            }

            totalPages = (LevelListScript.availableLevels / 6) + (LevelListScript.availableLevels % 6 == 0 ? 0 : 1);
            currentPage = Math.Max(Math.Min(currentPage, totalPages), 1);

            if (!currentPageField.isFocused)
            {
                currentPageField.text = currentPage + "";
            }
            totalPagesText.text = totalPages + "";
        }
    }

    public void PrevPageButtonClick()
    {
        currentPage = Math.Max(currentPage - 1, 1);
        cachedPage = currentPage;
        StartCoroutine(LevelListScript.instance.LoadLevelListCo());
    }

    public void NextPageButtonClick()
    {
        currentPage = Math.Min(currentPage + 1, totalPages);
        cachedPage = currentPage;
        StartCoroutine(LevelListScript.instance.LoadLevelListCo());
    }

    public void OnPageEdit()
    {
        currentPage = int.Parse(currentPageField.text);

        currentPage = Math.Min(currentPage, totalPages);
        cachedPage = currentPage;
        StartCoroutine(LevelListScript.instance.LoadLevelListCo());
    }

}
