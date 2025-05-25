using DG.Tweening;
using TUFHelper;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MusicControlScript : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public static MusicControlScript instance;
    public Image muteIcon, image;
    public AudioSource audioSource;
    private bool isPlaying = true;
    public void Awake()
    {
        instance = this;        
    }
    public void Start()
    {
        if (!Main.Setting.PlayBackgroundMusic) 
        {
            audioSource.Pause();
            isPlaying = false;
            muteIcon.gameObject.SetActive(true);
        }
        else
        {
            audioSource.Play();
            isPlaying = true;
            muteIcon.gameObject.SetActive(false);
        }
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        if (isPlaying)
        {
            audioSource.Pause();
            isPlaying = false;
            muteIcon.gameObject.SetActive(true);
            Main.Setting.PlayBackgroundMusic = false;
        }
        else
        {
            audioSource.Play();
            isPlaying = true;
            muteIcon.gameObject.SetActive(false);
            Main.Setting.PlayBackgroundMusic = true;
        }
        Main.Setting.Save(Main.ModEntry);
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