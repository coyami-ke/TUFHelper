using System.Collections;
using System.Collections.Generic;
using TUFHelper;
using TUFHelper.Utils;
using UnityEngine;

public class FrontPageScript : MonoBehaviour
{
    public GameObject frontPageObject, playCanvasObject;

    public GameObject ratingPageObject;

    public static bool isFirstRun = true;

    public bool IsRatingPageActive { get; private set; } = false;

    public static FrontPageScript instance { get; private set; }

    public void Awake()
    {
        instance = this;

        if (isFirstRun)
        {
            isFirstRun = false;
            return;
        }

        if (Main.isInTUFHelper)
        {
            frontPageObject.SetActive(false);
            playCanvasObject.SetActive(true);
        }

    }
    //public void Start()
    //{
        
    //}

    public void OnDestroy()
    {
        IsRatingPageActive = ratingPageObject.activeSelf;
    }
}
