using System.Collections;
using System.Collections.Generic;
using TUFHelper;
using TUFHelper.Utils;
using UnityEngine;

public class FrontPageScript : MonoBehaviour
{
    public GameObject frontPageObject, playCanvasObject, packsCanvasObject;

    public GameObject ratingPageObject;

    public static bool isFirstRun = true;

    public bool IsRatingPageActive { get; private set; } = false;
    public static bool IsPackListActive { get; set; } = false;
    public static string LastOpenedPackId { get; set; } = string.Empty;

    public static FrontPageScript instance { get; private set; }

    public async void Awake()
    {
        instance = this;

        if (isFirstRun)
        {
            isFirstRun = false;
            return;
        }

        if (Main.isInTUFHelper)
        {
            if (!IsPackListActive)
            {
                frontPageObject.SetActive(false);
                playCanvasObject.SetActive(true);
            }
            else
            {
                frontPageObject.SetActive(false);
                packsCanvasObject.SetActive(true);

                await PackListScript.Instance.ShowPackView(LastOpenedPackId);
            }
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
