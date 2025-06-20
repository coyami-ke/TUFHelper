using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class WindowsManager : MonoBehaviour
{
    public static WindowsManager instance { get; private set; }

    [Header("UI Panels")]
    public GameObject LevelList;
    public GameObject PassesList;
    public GameObject PassInfo;
    public GameObject FolderList;

    private Dictionary<GameObject, RectTransform> panelTransforms;
    private Dictionary<GameObject, float> initialXPositions;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        panelTransforms = new Dictionary<GameObject, RectTransform>
        {
            { LevelList, LevelList.GetComponent<RectTransform>() },
            { PassesList, PassesList.GetComponent<RectTransform>() },
            { PassInfo, PassInfo.GetComponent<RectTransform>() },
            { FolderList, FolderList.GetComponent<RectTransform>() }
        };

        initialXPositions = new Dictionary<GameObject, float>();
        foreach (var kvp in panelTransforms)
        {
            initialXPositions[kvp.Key] = kvp.Value.anchoredPosition.x;
        }

        PassInfo.SetActive(false);
    }

    public bool FolderListActive { get; private set; } = false;

    public void MoveToPassInfo()
    {
        PassInfo.SetActive(true);

        AnimatePanel(LevelList, 1185f, 1f, Ease.OutExpo, 0f);
        AnimatePanel(PassesList, 1155f, 1f, Ease.OutExpo, 0.2f);
        AnimatePanel(PassInfo, 1175f, 1f, Ease.OutExpo, 0.4f);

        FolderListActive = false;
        global::FolderList.instance.IsShow = false;
    }

    public void MoveToLevelList()
    {
        AnimatePanel(PassInfo, 0f, 1f, Ease.OutExpo, 0f);
        AnimatePanel(PassesList, 0f, 1f, Ease.OutExpo, 0.2f);
        AnimatePanel(LevelList, 0f, 1f, Ease.OutExpo, 0.4f, () => PassInfo.SetActive(false));
        AnimatePanel(FolderList, 0f, 1f, Ease.OutExpo, 0.4f, () => FolderList.SetActive(false));

        FolderListActive = false;
        global::FolderList.instance.IsShow = false;
    }

    public void MoveToFolderList()
    {
        if (!FolderList.activeSelf)
            FolderList.SetActive(true);

        AnimatePanel(PassInfo, 0f, 1f, Ease.OutExpo);
        AnimatePanel(PassesList, 0f, 1f, Ease.OutExpo);
        AnimatePanel(LevelList, 1185f, 1f, Ease.OutExpo);
        AnimatePanel(FolderList, -1185f, 1f, Ease.OutExpo);

        FolderListActive = true;
        global::FolderList.instance.IsShow = true;
    }


    private void AnimatePanel(GameObject panel, float deltaX, float duration, Ease ease, float delay = 0f, TweenCallback onComplete = null)
    {
        if (!panelTransforms.ContainsKey(panel)) return;

        float targetX = initialXPositions[panel] + deltaX;
        var tween = panelTransforms[panel].DOAnchorPosX(targetX, duration)
            .SetEase(ease)
            .SetDelay(delay);

        if (onComplete != null)
            tween.OnComplete(onComplete);
    }
}
