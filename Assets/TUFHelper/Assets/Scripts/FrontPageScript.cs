using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrontPageScript : MonoBehaviour
{
    public GameObject playButton, settingsButton, leaderboardButton, ratingButton;

    public static FrontPageScript instance { get; private set; }

    public void Awake()
    {
        instance = this;
    }

    public void OnPlayButtonClicked()
    {

    }
    public void OnSettingsButtonClicked()
    {

    }
    public void OnLeaderboardButtonClicked()
    {

    }
}
