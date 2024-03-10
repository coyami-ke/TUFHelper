using DG.Tweening;
using System;
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
        instance = this;
        SwipeFromBlack();

        var buttons = FindObjectsOfType<Button>();
        foreach (var b in buttons)
        {
            b.onClick.AddListener(() =>
            {
                scrSfx.instance?.PlaySfx(SfxSound.MobileButton);
            });
        }
    }

    internal static void SwipeFromBlack(Action onComplete = null)
    {
        if(instance.isDuringTransition)
        {
            return;
        }
        if (instance.panelRectTransform == null)
        {
            return;
        }
        instance.isDuringTransition = true;
        
        JobDispatcher.AddJob(() =>
        {
            scrSfx.instance?.PlaySfx(SfxSound.ScreenWipeIn);
            
            instance.panelRectTransform.gameObject.SetActive(true);
            instance.panelRectTransform.DOAnchorMax(new Vector2(0, 1), 0.3f).onComplete += () =>
            {
                instance.isDuringTransition = false;
                instance.panelRectTransform.gameObject.SetActive(false);
                if (onComplete != null)
                {
                    onComplete.Invoke();
                }
            };
        });
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
        JobDispatcher.AddJob(() =>
        {
            scrSfx.instance?.PlaySfx(SfxSound.ScreenWipeOut);
            
            instance.panelRectTransform.gameObject.SetActive(true);
            instance.panelRectTransform.DOAnchorMax(new Vector2(1, 1), 0.3f).onComplete += () =>
            {
                instance.isDuringTransition = false;
                if (onComplete != null)
                {
                    onComplete.Invoke();
                }
            };
        });
    }
}
