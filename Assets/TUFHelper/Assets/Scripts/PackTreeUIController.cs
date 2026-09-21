using System.Collections.Generic;
using DG.Tweening;
using TUFHelper.ModScripts.Json;
using UnityEngine;

public class PackTreeUIController : MonoBehaviour
{
    public GameObject folderPrefab;
    public GameObject levelPrefab;
    public GameObject textBlockPrefab;
    public RectTransform rootContainer;
    public RectTransform treeViewport; // Assign your ScrollRect Viewport in the Unity Inspector

    private const float ItemHeight = 50f;
    private const float TextBlockHeight = 100f;
    private const float TextBlockBlankHeight = 50f;
    private const float IndentWidth = 24f;
    private const float AnimationDuration = 0.25f;

    private List<PackItemNode> _rootNodes = new();

    private float GetNodeHeight(PackItemNode node)
    {
        if (node == null) return ItemHeight;

        if (node.IsFolder || node.IsLevel)
            return ItemHeight;

        bool isBlank = string.IsNullOrWhiteSpace(node.Description);
        return isBlank ? TextBlockBlankHeight : TextBlockHeight;
    }

    public void BuildTree(List<PackItemNode> rootNodes, string packId, int? autoSelectLevelId = null)
    {
        ClearTree();
        _rootNodes = rootNodes;

        foreach (var node in _rootNodes)
        {
            InstantiateNodeRecursive(node, depth: 0, packId: packId, parent: null);
        }

        if (autoSelectLevelId != null)
        {
            ExpandAndScrollToNode(autoSelectLevelId.Value);
        }
        else
        {
            UpdateLayout(animated: false);
        }
    }

    private void InstantiateNodeRecursive(PackItemNode node, int depth, string packId, PackItemNode parent)
    {
        node.Parent = parent;
        GameObject go;

        if (node.IsFolder)
        {
            go = Instantiate(folderPrefab, rootContainer, false);
            FolderInPackScript folderScript = go.GetComponent<FolderInPackScript>();
            folderScript.SetFolderInfo(node);
            folderScript.InitTreeController(this, node);
            node.SpawnedUIScript = folderScript;
        }
        else if (node.IsLevel)
        {
            go = Instantiate(levelPrefab, rootContainer, false);
            LevelInPackScript levelScript = go.GetComponent<LevelInPackScript>();
            levelScript.SetLevelInfo(node, packId);
            node.SpawnedUIScript = levelScript;
        }
        else
        {
            go = Instantiate(textBlockPrefab, rootContainer, false);
            TextBlockInPackScript textBlockScript = go.GetComponent<TextBlockInPackScript>();
            textBlockScript.SetTextBlockInfo(node);
            node.SpawnedUIScript = textBlockScript;
        }

        RectTransform rect = go.GetComponent<RectTransform>();
        float height = GetNodeHeight(node);

        rect.offsetMin = new Vector2(depth * IndentWidth, rect.offsetMin.y);
        rect.offsetMax = new Vector2(0f, rect.offsetMax.y);
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);

        if (!go.TryGetComponent<CanvasGroup>(out _))
        {
            go.AddComponent<CanvasGroup>();
        }

        if (node.IsFolder && node.Children != null)
        {
            foreach (var child in node.Children)
            {
                InstantiateNodeRecursive(child, depth + 1, packId, node);
            }
        }
    }

    public void ExpandAndScrollToNode(int targetId)
    {
        PackItemNode targetNode = FindNodeById(_rootNodes, targetId);
        if (targetNode == null)
        {
            UpdateLayout(animated: false);
            return;
        }

        // Expand all ancestor folders
        PackItemNode currentParent = targetNode.Parent;
        while (currentParent != null)
        {
            currentParent.IsExpanded = true;
            currentParent = currentParent.Parent;
        }

        UpdateLayout(animated: false);

        // Scroll to position target node in the viewport
        if (targetNode.SpawnedUIScript != null && treeViewport != null)
        {
            RectTransform targetRect = targetNode.SpawnedUIScript.GetComponent<RectTransform>();

            // Calculate absolute Y coordinate relative to rootContainer
            float targetY = Mathf.Abs(targetRect.anchoredPosition.y);
            float viewportHeight = treeViewport.rect.height;

            // Center target node in viewport
            float scrollPosY = Mathf.Max(0, targetY - (viewportHeight / 2f) + (GetNodeHeight(targetNode) / 2f));
            rootContainer.DOAnchorPosY(scrollPosY, AnimationDuration).SetEase(Ease.OutCubic);
        }
    }

    private PackItemNode FindNodeById(List<PackItemNode> nodes, int targetId)
    {
        if (nodes == null) return null;

        foreach (var node in nodes)
        {
            if (node.Id == targetId) return node;

            if (node.IsFolder && node.Children != null)
            {
                var found = FindNodeById(node.Children, targetId);
                if (found != null) return found;
            }
        }

        return null;
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
        float nodeHeight = GetNodeHeight(node);

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

                currentY -= nodeHeight;
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