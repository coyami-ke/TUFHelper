using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MusicControlScript : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image muteIcon, image;
    public AudioSource audioSource;
    bool isPlaying = true;

    void Start()
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isPlaying)
        {
            audioSource.Pause();
            isPlaying = false;
            muteIcon.gameObject.SetActive(true);
        } else
        {
            audioSource.Play();
            isPlaying = true;
            muteIcon.gameObject.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        image.DOColor(new Color(0f, 0f, 0f, 60 / 255f), 0.5f).SetEase(Ease.OutExpo);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        image.DOColor(new Color(0f, 0f, 0f, 40 / 255f), 0.5f).SetEase(Ease.OutExpo);
    }

}