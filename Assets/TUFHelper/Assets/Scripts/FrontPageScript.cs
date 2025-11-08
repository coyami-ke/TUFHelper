using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrontPageScript : MonoBehaviour
{
    public GameObject frontPageObject;

    public static FrontPageScript instance { get; private set; }

    public void Awake()
    {
        instance = this;
    }
}
