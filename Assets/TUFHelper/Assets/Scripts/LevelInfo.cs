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
        IsShow = Main.Setting.ShowOnlyDownloaded;
    }

    private bool isShow;
    public bool IsShow
    {
        get => isShow;
        set
        {
            isShow = value;
            gameObject.SetActive(value);
        }
    }

    public void LoadInfoFromFile(CustomLevelInfoJson info)
    {
        bpm.text = info.BPM.ToString();
        tiles.text = info.Tiles.ToString();

        int minutes = Mathf.FloorToInt(info.Lenght / 60f);
        int seconds = Mathf.FloorToInt(info.Lenght % 60f);
        lenght.text = $"{minutes}:{seconds:D2}";
    }

}
