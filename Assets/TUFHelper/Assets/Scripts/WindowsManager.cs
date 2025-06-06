using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class WindowsManager : MonoBehaviour
{
    public static WindowsManager instance { get; private set; }

    public GameObject LevelList, PassesList, PassInfo;

    private RectTransform levelListTransform, passesListTransform, passInfoTransform;

    private float levelListX, passesListX, passInfoX;

    public void Awake()
    {
        if (instance == null) instance = this;
    }
    public void Start()
    {
        levelListTransform = LevelList.GetComponent<RectTransform>();
        passesListTransform = PassesList.GetComponent<RectTransform>();
        passInfoTransform = PassInfo.GetComponent<RectTransform>();

        levelListX = levelListTransform.anchoredPosition.x;
        passesListX = passesListTransform.anchoredPosition.x;
        passInfoX = passInfoTransform.anchoredPosition.x;

        PassInfo.SetActive(false);
    }
    public void MoveToPassInfo()
    {
        PassInfo.SetActive(true);
        levelListTransform.DOAnchorPosX(levelListX + 1185f, 1f).SetEase(Ease.InOutExpo).SetDelay(0f);
        passesListTransform.DOAnchorPosX(passesListX + 1155f, 1f).SetEase(Ease.InOutExpo).SetDelay(0.2f);
        passInfoTransform.DOAnchorPosX(passInfoX + 1175f, 1f).SetEase(Ease.InOutExpo).SetDelay(0.4f);
    }
    public void MoveToLevelList()
    {
        passInfoTransform.DOAnchorPosX(passInfoX, 1f).SetEase(Ease.InOutExpo).SetDelay(0.0f);
        passesListTransform.DOAnchorPosX(passesListX, 1f).SetEase(Ease.InOutExpo).SetDelay(0.2f);
        levelListTransform.DOAnchorPosX(levelListX, 1f).SetEase(Ease.InOutExpo).SetDelay(0.4f).OnComplete(() => PassInfo.SetActive(false));
    }
}
