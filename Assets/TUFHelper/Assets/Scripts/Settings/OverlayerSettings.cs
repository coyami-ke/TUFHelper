using System;
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

    public Toggle showPPDisplayerToggle, showSpeedToggle, showIngameLeaderboardToggle, showOverlayerToggle, showIngameLevelInfoToggle;

    public TextMeshProUGUI ppDisplayerText, speedText;

    public Image leaderboardBackground, levelInfoBackground;

    public GameObject blockUIImage;

    private Color leaderboardDefaultColor;

    // Cache the canvas scale factor to make calculations lighter
    private float GetCanvasScale()
    {
        Canvas rootCanvas = GetComponentInParent<Canvas>();
        return rootCanvas != null ? rootCanvas.scaleFactor : 1f;
    }

    public void Start()
    {
        leaderboardDefaultColor = leaderboardBackground.color;

        // Register slider listeners
        ppDisplayerSlider.onValueChanged.AddListener(PPDisplayerScaleChanged);
        ingameLeaderboardSlider.onValueChanged.AddListener(IngameLeaderboardScaleChanged);

        // Register toggle listeners
        showPPDisplayerToggle.onValueChanged.AddListener(PPDisplayerShowChanged);
        showSpeedToggle.onValueChanged.AddListener(SpeedShowChanged);
        showIngameLeaderboardToggle.onValueChanged.AddListener(LeaderboardShowChanged);
        showOverlayerToggle.onValueChanged.AddListener(ShowOverlayerChanged);
        showIngameLevelInfoToggle.onValueChanged.AddListener(ShowIngameLevelInfo);

        float canvasScaleFactor = GetCanvasScale();

        // Initialize PPDisplayer Scale
        if (Main.Setting.OverlayerElementsPositions.ContainsKey("PPDisplayer"))
        {
            float savedScale = Main.Setting.OverlayerElementsPositions["PPDisplayer"].Scale;
            ppDisplayerSlider.value = savedScale;

            // Counteract canvas scaling factor for the preview element
            float finalScale = (1f / canvasScaleFactor) * savedScale;
            ppDisplayerRect.localScale = new Vector3(finalScale, finalScale, 1);
        }
        else
        {
            ppDisplayerSlider.value = 1f;
            float finalScale = 1f / canvasScaleFactor;
            ppDisplayerRect.localScale = new Vector3(finalScale, finalScale, 1);
        }

        // Initialize IngameLeaderboard Scale
        if (Main.Setting.OverlayerElementsPositions.ContainsKey("IngameLeaderboard"))
        {
            float savedScale = Main.Setting.OverlayerElementsPositions["IngameLeaderboard"].Scale;
            ingameLeaderboardSlider.value = savedScale;

            // Counteract canvas scaling factor for the preview element
            float finalScale = (1f / canvasScaleFactor) * savedScale;
            ingameLeaderboardRect.localScale = new Vector3(finalScale, finalScale, 1);
        }
        else
        {
            ingameLeaderboardSlider.value = 1f;
            float finalScale = 1f / canvasScaleFactor;
            ingameLeaderboardRect.localScale = new Vector3(finalScale, finalScale, 1);
        }

        // Initialize Toggles
        showPPDisplayerToggle.isOn = Main.Setting.ShowIngamePPCounter;
        showSpeedToggle.isOn = Main.Setting.ShowIngameSpeed;
        showIngameLeaderboardToggle.isOn = Main.Setting.ShowIngameLeaderboard;
        showOverlayerToggle.isOn = Main.Setting.ShowTUFHelperOverlayer;
        showIngameLevelInfoToggle.isOn = Main.Setting.ShowIngameLevelInfo;
    }

    private void ShowIngameLevelInfo(bool value)
    {
        Main.Setting.ShowIngameLevelInfo = value;

        if (value)
        {
            levelInfoBackground.color = leaderboardDefaultColor;
        }
        else
        {
            levelInfoBackground.color = new Color(1, 0.5f, 0.5f, 0.5f);
        }
        Main.Setting.Save(Main.ModEntry);
    }

    private void PPDisplayerScaleChanged(float value)
    {
        // 1. Check if dictionary key exists, if not initialize it safely
        if (!Main.Setting.OverlayerElementsPositions.ContainsKey("PPDisplayer"))
        {
            Main.Setting.OverlayerElementsPositions["PPDisplayer"] = new() { X = 0, Y = 0 };
        }

        // 2. Save the pure config value (unaffected by canvas factor) to JSON
        Main.Setting.OverlayerElementsPositions["PPDisplayer"].Scale = value;

        // 3. Scale the visual element while counteracting the canvas engine
        float finalScale = (1f / GetCanvasScale()) * value;
        ppDisplayerRect.localScale = new Vector3(finalScale, finalScale, 1);

        Main.Setting.Save(Main.ModEntry);
    }

    private void IngameLeaderboardScaleChanged(float value)
    {
        // 1. Check if dictionary key exists, if not initialize it safely
        if (!Main.Setting.OverlayerElementsPositions.ContainsKey("IngameLeaderboard"))
        {
            Main.Setting.OverlayerElementsPositions["IngameLeaderboard"] = new() { X = 0, Y = 0 };
        }

        // 2. Save the pure config value to JSON
        Main.Setting.OverlayerElementsPositions["IngameLeaderboard"].Scale = value;

        // 3. Scale the visual element while counteracting the canvas engine
        float finalScale = (1f / GetCanvasScale()) * value;
        ingameLeaderboardRect.localScale = new Vector3(finalScale, finalScale, 1);

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
            ppDisplayerText.color = new Color(1, 0.5f, 0.5f, 1);
        }

        Main.Setting.Save(Main.ModEntry);
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
            speedText.color = new Color(1, 0.5f, 0.5f, 1);
        }

        Main.Setting.Save(Main.ModEntry);
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
            leaderboardBackground.color = new Color(1, 0.5f, 0.5f, 0.5f);
        }

        Main.Setting.Save(Main.ModEntry);
    }

    private void ShowOverlayerChanged(bool value)
    {
        Main.Setting.ShowTUFHelperOverlayer = value;
        blockUIImage.SetActive(!value);

        Main.Setting.Save(Main.ModEntry);
    }
}