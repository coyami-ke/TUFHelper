using System.Collections.Generic;
using DG.Tweening;
using TUFHelper;
using UnityEngine;

public class WindowsManager : MonoBehaviour
{
    public static WindowsManager instance { get; private set; }

    [Header("UI Panels")]
    public GameObject LevelList;
    public GameObject PassesList;
    public GameObject PassInfo;
    public GameObject FolderList;
    public GameObject RatingPanel;

    public List<GameObject> levelSearchElements;
    public List<GameObject> ratingSearchElements;

    private Dictionary<GameObject, RectTransform> panelTransforms;
    private Dictionary<GameObject, Vector2> initialPositions;
    private Dictionary<GameObject, Vector2> initialSize;

    private void Awake()
    {
        instance = this;
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

        initialPositions = new Dictionary<GameObject, Vector2>();
        initialSize = new Dictionary<GameObject, Vector2>();
        foreach (var kvp in panelTransforms)
        {
            initialPositions[kvp.Key] = kvp.Value.anchoredPosition;
            initialSize[kvp.Key] = kvp.Value.sizeDelta;
        }

        PassInfo.SetActive(false);
    }

    public bool FolderListActive { get; private set; } = false;

    public void MoveToPassInfo()
    {
        PassInfo.SetActive(true);

        AnimatePanelPos(LevelList, 1185f, 1f, Ease.OutExpo, 0f);
        AnimatePanelPos(PassesList, 1155f, 1f, Ease.OutExpo, 0.2f);
        AnimatePanelPos(PassInfo, 1175f, 1f, Ease.OutExpo, 0.4f);

        FolderListActive = false;
        global::FolderList.instance.IsShow = false;
    }

    public void MoveToLevelList()
    {
        AnimatePanelPos(PassInfo, 0f, 1f, Ease.OutExpo, 0f);
        AnimatePanelPos(PassesList, 0f, 1f, Ease.OutExpo, 0.2f);
        AnimatePanelPos(LevelList, 0f, 1f, Ease.OutExpo, 0.4f, () => PassInfo.SetActive(false));
        AnimatePanelPos(FolderList, 0f, 1f, Ease.OutExpo, 0.4f, () => FolderList.SetActive(false));

        FolderListActive = false;
        global::FolderList.instance.IsShow = false;
        RatingPanel.SetActive(false);
        PassesList.SetActive(true);
    }

    private bool isTransitioning = false;

    public void MoveToFolderList()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        if (!FolderList.activeSelf)
            FolderList.SetActive(true);

        AnimatePanelPos(PassInfo, 0f, 1f, Ease.OutExpo);
        AnimatePanelPos(PassesList, 0f, 1f, Ease.OutExpo);
        AnimatePanelPos(LevelList, 1185f, 1f, Ease.OutExpo);
        AnimatePanelPos(FolderList, -1185f, 1f, Ease.OutExpo, 0f, () =>
        {
            isTransitioning = false;
        });

        FolderListActive = true;
        global::FolderList.instance.IsShow = true;
    }

    private void AnimatePanelPos(GameObject panel, float deltaX, float duration, Ease ease, float delay = 0f, TweenCallback onComplete = null)
    {
        if (panel == null)
        {
            Main.Logger.Error("AnimatePanelPos: panel is null.");
            return;
        }

        if (!panelTransforms.TryGetValue(panel, out var rect) || rect == null)
        {
            Main.Logger.Error($"AnimatePanelPos: RectTransform not found for panel '{panel.name}'");
            return;
        }

        float targetX = initialPositions[panel].x + deltaX;

        var tween = rect.DOAnchorPosX(targetX, duration)
            .SetEase(ease)
            .SetDelay(delay);

        if (onComplete != null)
            tween.OnComplete(onComplete);
    }


    // Added Size Changer + Y axis support too just for the future
    private void AnimatePanelSize(GameObject panel, float deltaX, float deltaY, float duration, Ease ease, float delay = 0f, TweenCallback onComplete = null)
    {
        if (!panelTransforms.ContainsKey(panel)) return;

        Vector2 targetSize = new Vector2(initialSize[panel].x + deltaX, initialSize[panel].y + deltaY);
        var tween = panelTransforms[panel].DOSizeDelta(targetSize, duration)
            .SetEase(ease)
            .SetDelay(delay);

        if (onComplete != null)
            tween.OnComplete(onComplete);
    }

    public void ShowRatingPanel()
    {
        MoveToLevelList();
        PassesList.SetActive(false);
        PassInfo.SetActive(false);
        global::FolderList.instance.IsShow = false;

        LevelList.SetActive(false);

        RatingPanel.SetActive(true);

        foreach (var element in levelSearchElements)
        {
            element.SetActive(false);
        }
        foreach (var element in ratingSearchElements)
        {
            element.SetActive(true);
        }
    }
    public void HideRatingPanel()
    {
        LevelList.SetActive(true);

        foreach (var element in levelSearchElements)
        {
            element.SetActive(true);
        }
        foreach (var element in ratingSearchElements)
        {
            element.SetActive(false);
        }

        MoveToLevelList();
        AddLevelToFolder.instance.IsShow = false;
        // PassesList.SetActive(false);
        // PassInfo.SetActive(false);
        // FolderList.SetActive(false);
    }

    // public void ShowPassList()
    // {
    //     var list = LevelListScript.instance.GetLevelPrefabScripts();
    //     bool flag = false;
    //     for (int i = 0; i < list.Length; i++)
    //     {
    //         if (list[i].IsSelected) { flag = true; break; }
    //     }
    //     if (flag)
    //     {
    //         AnimatePanelPos(PassesList, 0f, 1f, Ease.OutExpo);
    //         AnimatePanelPos(LevelList, 0f, 1f, Ease.OutExpo);
    //         AnimatePanelSize(LevelList, 0f, 0f, 1f, Ease.OutExpo);
    //     }
    //     else
    //     {
    //         AnimatePanelPos(PassesList, -800f, 1f, Ease.OutExpo);
    //         AnimatePanelPos(LevelList, -365f, 1f, Ease.OutExpo);
    //         AnimatePanelSize(LevelList, 710f, 0f, 1f, Ease.OutExpo);
    //     }
    // }
}
