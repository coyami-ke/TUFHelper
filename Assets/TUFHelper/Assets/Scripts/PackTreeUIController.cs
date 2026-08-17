using System.Collections.Generic;
using DG.Tweening;
using TUFHelper.ModScripts.Json;
using UnityEngine;

public class PackTreeUIController : MonoBehaviour
{
    public GameObject folderPrefab;
    public GameObject levelPrefab;
    public RectTransform rootContainer;

    private const float ItemHeight = 50f;
    private const float IndentWidth = 24f;
    private const float AnimationDuration = 0.25f;

    private List<PackItemNode> _rootNodes = new();

    public void BuildTree(List<PackItemNode> rootNodes, string packId)
    {
        ClearTree();
        _rootNodes = rootNodes;

        foreach (var node in _rootNodes)
        {
            InstantiateNodeRecursive(node, 0, packId);
        }

        UpdateLayout(animated: false);
    }

    private void InstantiateNodeRecursive(PackItemNode node, int depth, string packId)
    {
        GameObject go;

        if (node.IsFolder)
        {
            go = Instantiate(folderPrefab, rootContainer, false);
            FolderInPackScript folderScript = go.GetComponent<FolderInPackScript>();
            folderScript.SetFolderInfo(node);
            folderScript.InitTreeController(this, node);
            node.SpawnedUIScript = folderScript;
        }
        else
        {
            go = Instantiate(levelPrefab, rootContainer, false);
            LevelInPackScript levelScript = go.GetComponent<LevelInPackScript>();
            levelScript.SetLevelInfo(node, packId);
            node.SpawnedUIScript = levelScript;
        }

        RectTransform rect = go.GetComponent<RectTransform>();

        rect.offsetMin = new Vector2(depth * IndentWidth, rect.offsetMin.y);
        rect.offsetMax = new Vector2(0f, rect.offsetMax.y);
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, ItemHeight);

        if (!go.TryGetComponent<CanvasGroup>(out _))
        {
            go.AddComponent<CanvasGroup>();
        }

        if (node.IsFolder && node.Children != null)
        {
            foreach (var child in node.Children)
            {
                InstantiateNodeRecursive(child, depth + 1, packId);
            }
        }
    }

    public void UpdateLayout(bool animated = true)
    {
        float currentY = 0f;

        foreach (var node in _rootNodes)
        {
            PositionNodeRecursive(node, ref currentY, isParentVisible: true, parentTargetY: 0f, animated);
        }

        if (animated)
        {
            rootContainer.DOSizeDelta(new Vector2(rootContainer.sizeDelta.x, Mathf.Abs(currentY)), AnimationDuration)
                .SetEase(Ease.OutCubic);
        }
        else
        {
            rootContainer.sizeDelta = new Vector2(rootContainer.sizeDelta.x, Mathf.Abs(currentY));
        }
    }

    private void PositionNodeRecursive(PackItemNode node, ref float currentY, bool isParentVisible, float parentTargetY, bool animated)
    {
        float thisNodeY = currentY;

        if (node.SpawnedUIScript != null)
        {
            GameObject obj = node.SpawnedUIScript.gameObject;
            RectTransform rect = obj.GetComponent<RectTransform>();
            CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();

            rect.DOKill();
            canvasGroup.DOKill();

            if (isParentVisible)
            {
                if (!obj.activeSelf)
                {
                    obj.SetActive(true);
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, parentTargetY);
                    canvasGroup.alpha = 0f;
                }

                if (animated)
                {
                    rect.DOAnchorPosY(currentY, AnimationDuration).SetEase(Ease.OutCubic);
                    canvasGroup.DOFade(1f, AnimationDuration).SetEase(Ease.OutCubic);
                }
                else
                {
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, currentY);
                    canvasGroup.alpha = 1f;
                }

                currentY -= ItemHeight;
            }
            else
            {
                if (obj.activeSelf)
                {
                    if (animated)
                    {
                        rect.DOAnchorPosY(parentTargetY, AnimationDuration).SetEase(Ease.InCubic);
                        canvasGroup.DOFade(0f, AnimationDuration)
                            .SetEase(Ease.InCubic)
                            .OnComplete(() => obj.SetActive(false));
                    }
                    else
                    {
                        canvasGroup.alpha = 0f;
                        obj.SetActive(false);
                    }
                }
            }
        }

        bool shouldShowChildren = isParentVisible && node.IsFolder && node.IsExpanded;

        if (node.Children != null)
        {
            foreach (var child in node.Children)
            {
                PositionNodeRecursive(child, ref currentY, shouldShowChildren, thisNodeY, animated);
            }
        }
    }

    public void ClearTree()
    {
        foreach (Transform child in rootContainer)
        {
            child.DOKill();
            Destroy(child.gameObject);
        }
    }
}