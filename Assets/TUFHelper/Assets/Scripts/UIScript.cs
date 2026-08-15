using DG.Tweening;
using System;
using System.Threading;
using TMPro;
using TUFHelper;
using UnityEngine;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{

    public static UIScript instance;

    public GameObject canvasObject;
    public RectTransform panelRectTransform;

    internal bool isDuringTransition = false;

    public void Start()
    {
        if (Main.mainThread == null)
            Main.mainThread = SynchronizationContext.Current;
        
        instance = this;
        SwipeFromBlack();

        //var buttons = FindObjectsOfType<Button>();
        //foreach (var b in buttons)
        //{
        //    b.onClick.AddListener(() =>
        //    {
        //        scrSfx.instance?.PlaySfx(SfxSound.MobileButton);
        //    });
        //}

        //AddByeolToCredits();
    }

    //private static void AddByeolToCredits()
    //{
    //    foreach (TextMeshProUGUI text in FindObjectsOfType<TextMeshProUGUI>(true))
    //    {
    //        if (text == null || text.gameObject.name != "Programmers")
    //        {
    //            continue;
    //        }

    //        if (string.IsNullOrEmpty(text.text) || text.text.Contains("Byeol"))
    //        {
    //            return;
    //        }

    //        if (text.text.Contains("Flower"))
    //        {
    //            text.text = text.text.Replace("Flower", "Flower\nByeol");
    //            ExtendCreditsBox(text.rectTransform);
    //            return;
    //        }
    //    }
    //}

    //private static void ExtendCreditsBox(RectTransform programmersRect)
    //{
    //    if (programmersRect == null)
    //    {
    //        return;
    //    }

    //    if (programmersRect.sizeDelta.y < 170f)
    //    {
    //        programmersRect.sizeDelta = new Vector2(programmersRect.sizeDelta.x, programmersRect.sizeDelta.y + 30f);
    //    }

    //    Transform parent = programmersRect.parent;
    //    while (parent != null)
    //    {
    //        RectTransform rect = parent as RectTransform;
    //        Image image = parent.GetComponent<Image>();
    //        if (rect != null && image != null && parent.name == "Credits" && rect.sizeDelta.y < 690f)
    //        {
    //            rect.sizeDelta = new Vector2(rect.sizeDelta.x, rect.sizeDelta.y + 30f);
    //            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y - 15f);
    //            return;
    //        }

    //        parent = parent.parent;
    //    }
    //}

    internal static void SwipeFromBlack(Action onComplete = null)
    {
        if (instance.isDuringTransition)
        {
            return;
        }
        if (instance.panelRectTransform == null)
        {
            return;
        }
        instance.isDuringTransition = true;

        if (scrSfx.instance != null)
            scrSfx.instance.PlaySfx(SfxSound.ScreenWipeIn);

        instance.panelRectTransform.gameObject.SetActive(true);
        instance.panelRectTransform.DOAnchorMax(new Vector2(0, 1), 0.3f).onComplete += () =>
        {
            instance.isDuringTransition = false;
            instance.panelRectTransform.gameObject.SetActive(false);
            onComplete?.Invoke();
        };
    }

    internal static void SwipeToBlack(Action onComplete = null)
    {
        if (instance.isDuringTransition)
        {
            return;
        }
        if (instance.panelRectTransform == null)
        {
            return;
        }
        instance.isDuringTransition = true;
        //JobDispatcher.AddJob(() =>
        //{
        //    
        //});

        if (scrSfx.instance != null)
            scrSfx.instance.PlaySfx(SfxSound.ScreenWipeOut);

        instance.panelRectTransform.gameObject.SetActive(true);
        instance.panelRectTransform.DOAnchorMax(new Vector2(1, 1), 0.3f).onComplete += () =>
        {
            instance.isDuringTransition = false;
            onComplete?.Invoke();
        };
    }
}
