using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RankPrefabClickScript : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    public RankPrefabScript rps;
    public Image image;

    public void Awake()
    {
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        rps.InfoButtonClick();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (rps.GetPassInfo().vidLink.Contains("http"))
        {
            image.DOColor(new Color(1f, 1f, 1f, 80 / 255f), 0.5f).SetEase(Ease.OutExpo);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (rps.GetPassInfo().vidLink.Contains("http"))
        {
            image.DOColor(new Color(1f, 1f, 1f, 40 / 255f), 0.5f).SetEase(Ease.OutExpo);
        }

    }

    public void Update()
    {
        
    }
}
