using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FrontPageButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Components")]
    private Image image;
    public Image lineImage;
    public GameObject showableCanvas;
    public TextMeshProUGUI text;
    public Image icon;

    [Header("Animation Settings")]
    [SerializeField] private float hoverScaleFactor = 1.125f;
    [SerializeField] private float colorDuration = 0.3f;
    [SerializeField] private float scaleDuration = 0.25f;
    [SerializeField] private float textDelay = 0.08f;

    private Vector3 originalIconScale;
    private Vector3 originalTextScale;
    private bool isHovered;

    private void Awake()
    {
        image = GetComponent<Image>();

        if (icon != null) originalIconScale = icon.rectTransform.localScale;
        if (text != null) originalTextScale = text.transform.localScale;
    }

    private void Start()
    {
        SetAlphaImmediate(image, 60 / 255f);
        if (lineImage != null) SetAlphaImmediate(lineImage, 50 / 255f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;

        AnimateAlpha(image, 80 / 255f, colorDuration);
        if (lineImage != null) AnimateAlpha(lineImage, 172 / 255f, colorDuration);

        if (icon != null)
        {
            icon.rectTransform.DOKill();
            icon.rectTransform.DOScale(originalIconScale * hoverScaleFactor, scaleDuration)
                .SetEase(Ease.OutBack, 1.4f);
        }

        if (text != null)
        {
            text.transform.DOKill();
            text.transform.DOScale(originalTextScale * hoverScaleFactor, scaleDuration)
                .SetEase(Ease.OutBack, 1.4f)
                .SetDelay(textDelay);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;

        AnimateAlpha(image, 60 / 255f, colorDuration);
        if (lineImage != null) AnimateAlpha(lineImage, 50 / 255f, colorDuration);

        if (icon != null)
        {
            icon.rectTransform.DOKill();
            icon.rectTransform.DOScale(originalIconScale, scaleDuration)
                .SetEase(Ease.OutQuad);
        }

        if (text != null)
        {
            text.transform.DOKill();
            text.transform.DOScale(originalTextScale, scaleDuration)
                .SetEase(Ease.OutQuad);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        AnimateAlpha(image, 120 / 255f, 0.15f);
        if (lineImage != null) AnimateAlpha(lineImage, 220 / 255f, 0.15f);

        if (icon != null)
        {
            icon.rectTransform.DOKill();
            icon.rectTransform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 10, 1f);
        }

        if (showableCanvas != null) showableCanvas.SetActive(true);
        if (FrontPageScript.instance != null && FrontPageScript.instance.frontPageObject != null)
        {
            FrontPageScript.instance.frontPageObject.SetActive(false);
        }

        ResetToDefaultState();
    }

    private void OnDisable()
    {
        KillAllTweens();
        ResetToDefaultState();
    }

    private void AnimateAlpha(Image targetImage, float targetAlpha, float duration)
    {
        if (targetImage == null) return;
        targetImage.DOKill();
        targetImage.DOFade(targetAlpha, duration).SetEase(Ease.OutCubic);
    }

    private void SetAlphaImmediate(Image targetImage, float alpha)
    {
        if (targetImage == null) return;
        Color c = targetImage.color;
        c.a = alpha;
        targetImage.color = c;
    }

    private void ResetToDefaultState()
    {
        isHovered = false;
        if (icon != null) icon.rectTransform.localScale = originalIconScale;
        if (text != null) text.transform.localScale = originalTextScale;
        SetAlphaImmediate(image, 60 / 255f);
        if (lineImage != null) SetAlphaImmediate(lineImage, 50 / 255f);
    }

    private void KillAllTweens()
    {
        if (image != null) image.DOKill();
        if (lineImage != null) lineImage.DOKill();
        if (icon != null) icon.rectTransform.DOKill();
        if (text != null) text.transform.DOKill();
    }
}