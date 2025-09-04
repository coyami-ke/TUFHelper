using System.Collections;
using System.Collections.Generic;
using TMPro;
using TUFHelper;
using UnityEngine;
using UnityEngine.UI;

public class BootstrapSettings : MonoBehaviour
{
    public Toggle startWithGameToggle;
    public TMP_InputField levelSavePathField;
    public void Start()
    {
        startWithGameToggle.isOn = Main.Setting.StartWithGame;
        levelSavePathField.text = Main.Setting.LevelSaveFolder;
    }

    public void OnLevelSavePathChanged(string value)
    {
        Main.Setting.LevelSaveFolder = value;
        Main.Setting.Save(Main.ModEntry);
    }

    public void OnStartWithGameChanged(bool value)
    {
        Main.Setting.StartWithGame = value;
        Main.Setting.Save(Main.ModEntry);
    }
}
