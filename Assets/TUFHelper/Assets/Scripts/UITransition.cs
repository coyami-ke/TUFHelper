using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public static class UITransition
{
    private const float DefaultDuration = 0.22f;
    private const float DefaultOffsetY = -8f;

    public static void Show(GameObject target, float duration = DefaultDuration, float delay = 0f)
    {
        if (target == null) return;

        CanvasGroup canvasGroup = GetCanvasGroup(target);
        RectTransform rect = target.transform as RectTransform;
        Transform targetTransform = target.transform;

        canvasGroup.DOKill();
        targetTransform.DOKill();
        if (rect != null) rect.DOKill();

        Vector3 targetScale = targetTransform.localScale;
        Vector2 targetPosition = rect != null ? rect.anchoredPosition : Vector2.zero;

        target.SetActive(true);

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        targetTransform.localScale = targetScale * 0.988f;

        canvasGroup.DOFade(1f, duration)
            .SetDelay(delay)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
        {
            if (canvasGroup == null) return;
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        });

        targetTransform.DOScale(targetScale, duration + 0.02f)
            .SetDelay(delay)
            .SetEase(Ease.OutCubic)
            .OnKill(() =>
        {
            if (targetTransform != null)
                targetTransform.localScale = targetScale;
        });

        if (rect != null)
        {
            rect.anchoredPosition = targetPosition + new Vector2(-16f, DefaultOffsetY * 0.5f);
            rect.DOAnchorPos(targetPosition, duration + 0.04f)
                .SetDelay(delay)
                .SetEase(Ease.OutCubic)
                .OnKill(() =>
            {
                if (rect != null)
                    rect.anchoredPosition = targetPosition;
            });
        }
    }

    public static void Hide(GameObject target, float duration = DefaultDuration, float delay = 0f)
    {
        if (target == null || !target.activeSelf) return;

        CanvasGroup canvasGroup = GetCanvasGroup(target);
        RectTransform rect = target.transform as RectTransform;
        Transform targetTransform = target.transform;

        canvasGroup.DOKill();
        targetTransform.DOKill();
        if (rect != null) rect.DOKill();

        Vector2 targetPosition = rect != null ? rect.anchoredPosition : Vector2.zero;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        canvasGroup.DOFade(0f, duration)
            .SetDelay(delay)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
        {
            if (target == null) return;
            target.SetActive(false);
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            if (rect != null)
                rect.anchoredPosition = targetPosition;
        });

        if (rect != null)
        {
            rect.DOAnchorPos(targetPosition + new Vector2(0f, DefaultOffsetY), duration)
                .SetDelay(delay)
                .SetEase(Ease.InCubic)
                .OnKill(() =>
            {
                if (rect != null)
                    rect.anchoredPosition = targetPosition;
            });
        }
    }

    public static void SetVisible(GameObject target, bool visible, float delay = 0f)
    {
        if (visible)
            Show(target, DefaultDuration, delay);
        else
            Hide(target, DefaultDuration, delay);
    }

    public static void ShowFront(GameObject target)
    {
        if (target == null) return;
        target.SetActive(true);
        Graphic[] graphics = target.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            Graphic graphic = graphics[i];
            graphic.DOKill();
            float alpha = graphic.color.a;
            Color color = graphic.color;
            color.a = 0f;
            graphic.color = color;
            graphic.DOFade(alpha, 0.26f).SetDelay(Mathf.Min(i * 0.006f, 0.08f)).SetEase(Ease.OutCubic);
        }
    }

    public static void AnimateLevelListItem(GameObject target, int order, float offsetX)
    {
        if (target == null) return;
        RectTransform rect = target.transform as RectTransform;
        if (rect == null) return;

        float delay = Mathf.Min(order * 0.028f, 0.34f);
        Vector2 finalPosition = rect.anchoredPosition;
        Vector3 finalScale = rect.localScale;
        rect.DOKill();
        rect.anchoredPosition = finalPosition + new Vector2(offsetX, 0f);
        rect.localScale = finalScale * 0.992f;
        rect.DOAnchorPos(finalPosition, 0.26f).SetDelay(delay).SetEase(Ease.OutCubic);
        rect.DOScale(finalScale, 0.24f).SetDelay(delay).SetEase(Ease.OutCubic);

        foreach (Graphic graphic in target.GetComponentsInChildren<Graphic>(true))
        {
            graphic.DOKill();
            float alpha = graphic.color.a;
            Color color = graphic.color;
            color.a = 0f;
            graphic.color = color;
            graphic.DOFade(alpha, 0.20f).SetDelay(delay).SetEase(Ease.OutCubic);
        }
    }

    public static void AnimateLeaderboardItem(Component target)
    {
        if (target == null) return;
        Transform root = target.transform;
        int order = Mathf.Abs(root.GetSiblingIndex()) % 50;
        float delay = Mathf.Min(order * 0.028f, 0.34f);
        Vector3 finalScale = root.localScale;
        root.DOKill();
        root.localScale = finalScale * 0.992f;
        root.DOScale(finalScale, 0.24f).SetDelay(delay).SetEase(Ease.OutCubic);

        for (int i = 0; i < root.childCount; i++)
        {
            RectTransform rect = root.GetChild(i) as RectTransform;
            if (rect == null) continue;
            Vector2 finalPosition = rect.anchoredPosition;
            rect.DOKill();
            rect.anchoredPosition = finalPosition + new Vector2(30f, 0f);
            rect.DOAnchorPos(finalPosition, 0.26f).SetDelay(delay).SetEase(Ease.OutCubic);
        }

        foreach (Graphic graphic in target.gameObject.GetComponentsInChildren<Graphic>(true))
        {
            graphic.DOKill();
            float alpha = graphic.color.a;
            Color color = graphic.color;
            color.a = 0f;
            graphic.color = color;
            graphic.DOFade(alpha, 0.20f).SetDelay(delay).SetEase(Ease.OutCubic);
        }
    }

    public static void AnimateRankSelection(Transform row, bool selected)
    {
        if (row == null) return;
        row.DOKill();
        row.DOScale(selected ? Vector3.one * 1.012f : Vector3.one, 0.18f).SetEase(Ease.OutCubic);
    }

    public static void AnimateDetailLoaded(Component target)
    {
        if (target == null) return;
        Graphic[] graphics = target.gameObject.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            Graphic graphic = graphics[i];
            graphic.DOKill();
            float alpha = graphic.color.a;
            Color color = graphic.color;
            color.a = alpha * 0.18f;
            graphic.color = color;
            graphic.DOFade(alpha, 0.24f).SetDelay(Mathf.Min(i * 0.003f, 0.045f)).SetEase(Ease.OutCubic);
        }
    }

    public static void AnimatePackSelection(GameObject panel, Component selectedPack)
    {
        if (selectedPack != null)
        {
            Transform selected = selectedPack.transform;
            selected.DOKill();
            Vector3 finalScale = selected.localScale;
            selected.localScale = finalScale * 0.992f;
            selected.DOScale(finalScale, 0.18f).SetEase(Ease.OutCubic);
        }

        if (panel == null) return;
        Graphic[] graphics = panel.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            Graphic graphic = graphics[i];
            graphic.DOKill();
            float alpha = graphic.color.a;
            Color color = graphic.color;
            color.a = alpha * 0.16f;
            graphic.color = color;
            graphic.DOFade(alpha, 0.22f).SetDelay(Mathf.Min(i * 0.0035f, 0.06f)).SetEase(Ease.OutCubic);
        }
    }

    public static void AnimateLobbyIn(GameObject panel)
    {
        if (panel == null) return;
        Graphic[] graphics = panel.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            Graphic graphic = graphics[i];
            graphic.DOKill();
            float alpha = graphic.color.a;
            Color color = graphic.color;
            color.a = 0f;
            graphic.color = color;
            graphic.DOFade(alpha, 0.24f).SetDelay(Mathf.Min(i * 0.008f, 0.12f)).SetEase(Ease.OutCubic);
        }
    }

    private static CanvasGroup GetCanvasGroup(GameObject target)
    {
        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = target.AddComponent<CanvasGroup>();
        return canvasGroup;
    }
}
