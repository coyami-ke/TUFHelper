using System.Collections;
using System.Collections.Generic;
using TMPro;
using TUFHelper;
using UnityEngine;
using UnityEngine.UI;

public class OverlayerSettings : MonoBehaviour
{
    public Slider ppDisplayerSlider, ingameLeaderboardSlider;
    public RectTransform ppDisplayerRect, ingameLeaderboardRect;

    public Toggle showPPDisplayerToggle, showSpeedToggle, showIngameLeaderboardToggle;

    public TextMeshProUGUI ppDisplayerText, speedText;

    public Image leaderboardBackground;

    private Color leaderboardDefaultColor;

    public void Start()
    {
        leaderboardDefaultColor = leaderboardBackground.color;

        ppDisplayerSlider.onValueChanged.AddListener(PPDisplayerScaleChanged);
        ingameLeaderboardSlider.onValueChanged.AddListener(IngameLeaderboardScaleChanged);

        showPPDisplayerToggle.onValueChanged.AddListener(PPDisplayerShowChanged);
        showSpeedToggle.onValueChanged.AddListener(SpeedShowChanged);
        showIngameLeaderboardToggle.onValueChanged.AddListener(LeaderboardShowChanged);

        if (Main.Setting.OverlayerElementsPositions.ContainsKey("PPDisplayer"))
        {
            ppDisplayerRect.localScale = new Vector3(
                Main.Setting.OverlayerElementsPositions["PPDisplayer"].Scale,
                Main.Setting.OverlayerElementsPositions["PPDisplayer"].Scale,
                1
            );
            ppDisplayerSlider.value = ppDisplayerRect.localScale.x;
        }
        if (Main.Setting.OverlayerElementsPositions.ContainsKey("IngameLeaderboard"))
        {
            ingameLeaderboardRect.localScale = new Vector3(
                Main.Setting.OverlayerElementsPositions["IngameLeaderboard"].Scale,
                Main.Setting.OverlayerElementsPositions["IngameLeaderboard"].Scale,
                1
            );
            ingameLeaderboardSlider.value = ingameLeaderboardRect.localScale.x;
        }

        showPPDisplayerToggle.isOn = Main.Setting.ShowIngamePPCounter;
        showSpeedToggle.isOn = Main.Setting.ShowIngameSpeed;
        showIngameLeaderboardToggle.isOn = Main.Setting.ShowIngameLeaderboard;
    }

    private void PPDisplayerScaleChanged(float value)
    {
        Main.Setting.OverlayerElementsPositions["PPDisplayer"].Scale = value;
        ppDisplayerRect.localScale = new Vector3(value, value, 1);
        Main.Setting.Save(Main.ModEntry);
    }

    private void IngameLeaderboardScaleChanged(float value)
    {
        Main.Setting.OverlayerElementsPositions["IngameLeaderboard"].Scale = value;
        ingameLeaderboardRect.localScale = new Vector3(value, value, 1);
        Main.Setting.Save(Main.ModEntry);
    }

    private void PPDisplayerShowChanged(bool value)
    {
        Main.Setting.ShowIngamePPCounter = value;
        if (value)
        {
            ppDisplayerText.color = Color.white;
        }
        else
        {
            ppDisplayerText.color = new(1, 0.5f, 0.5f, 1);
        }
    }

    private void SpeedShowChanged(bool value)
    {
        Main.Setting.ShowIngameSpeed = value;
        if (value)
        {
            speedText.color = Color.white;
        }
        else
        {
            speedText.color = new(1, 0.5f, 0.5f, 1);
        }
    }


    private void LeaderboardShowChanged(bool value)
    {
        Main.Setting.ShowIngameLeaderboard = value;

        if (value)
        {
            leaderboardBackground.color = leaderboardDefaultColor;
        }
        else
        {
            leaderboardBackground.color = new(1, 0.5f, 0.5f, 0.5f);
        }
    }
}
