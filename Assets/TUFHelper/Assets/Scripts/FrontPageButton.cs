using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FrontPageButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Image image;

    public Image lineImage;

    public GameObject showableCanvas;

    public void Start()
    {
        image = GetComponent<Image>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        image.DOColor(new Color(image.color.r, image.color.g, image.color.b, 100 / 255f), 0.35f).SetEase(Ease.OutCubic);
        lineImage.DOColor(new Color(lineImage.color.r, lineImage.color.g, lineImage.color.b, 200 / 255f), 0.35f).SetEase(Ease.OutCubic);

        showableCanvas.SetActive(true);
        FrontPageScript.instance.frontPageObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        image.DOColor(new Color(image.color.r, image.color.g, image.color.b, 80 / 255f), 0.35f).SetEase(Ease.OutCubic);
        lineImage.DOColor(new Color(lineImage.color.r, lineImage.color.g, lineImage.color.b, 172 / 255f), 0.35f).SetEase(Ease.OutCubic);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        image.DOColor(new Color(image.color.r, image.color.g, image.color.b, 60 / 255f), 0.35f).SetEase(Ease.OutCubic);
        lineImage.DOColor(new Color(lineImage.color.r, lineImage.color.g, lineImage.color.b, 50 / 255f), 0.35f).SetEase(Ease.OutCubic);
    }
}
