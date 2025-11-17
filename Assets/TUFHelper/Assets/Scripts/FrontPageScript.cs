using System.Collections;
using System.Collections.Generic;
using TUFHelper;
using UnityEngine;

public class FrontPageScript : MonoBehaviour
{
    public GameObject frontPageObject;

    public GameObject ratingPageObject;

    public bool IsRatingPageActive { get; private set; } = false;

    public static FrontPageScript instance { get; private set; }

    public void Awake()
    {
        instance = this;
    }

    public void OnDestroy()
    {
        IsRatingPageActive = ratingPageObject.activeSelf;
    }
}
