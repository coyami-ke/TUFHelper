using DG.Tweening;
using System;
using UnityEngine;

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
