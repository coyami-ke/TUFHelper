using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using UnityEngine;

public class LevelInfo : MonoBehaviour
{
    public static LevelInfo instance;

    public TextMeshProUGUI bpm, lenght, tiles;

    public void Awake()
    {
        if (instance == null) instance = this;
    }
    public void Start()
    {
    }

    private bool isShow = true;
    public bool IsShow
    {
        get => isShow;
        set
        {
            isShow = value;
            gameObject.SetActive(value);
        }
    }

    public void LoadLevelInfo(LevelListInfoElementJson info)
    {
        if (info == null)
        {
            bpm.text = "unknown";
            tiles.text = "unknown";
            lenght.text = "unknown";
        }
        if (info.BPM != null) bpm.text = info.BPM.ToString();
        else bpm.text = "unknown";
        if (info.TileCount != null) tiles.text = info.TileCount.ToString();
        else tiles.text = "unknown";

        if (info.LevelLengthInMs.HasValue)
        {
            TimeSpan time = TimeSpan.FromMilliseconds(info.LevelLengthInMs.Value);

            lenght.text = string.Format("{0}:{1:D2}", (int)time.TotalMinutes, time.Seconds);
        }
        else
        {
            lenght.text = "unknown";
        }
    }
}
