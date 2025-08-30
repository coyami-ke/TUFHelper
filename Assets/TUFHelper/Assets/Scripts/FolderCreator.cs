using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TUFHelper;
using UnityEngine;

public class FolderCreator : MonoBehaviour
{
    public GameObject menuObject;
    public TMP_InputField inputField;
    private bool _isMenuActive = true;
    public bool IsMenuActive
    {
        get => _isMenuActive;
        set
        {
            if (value == _isMenuActive) return;
            _isMenuActive = value;
            menuObject.SetActive(value);
        }
    }
    public void Start()
    {
        IsMenuActive = false;
    }
    public void ShowMenu()
    {
        IsMenuActive = !IsMenuActive;
    }
    public async void AddFolder()
    {
        IsMenuActive = false;

        Main.Setting.LevelFolders.Add(new TUFHelper.Utils.LevelFolder(Array.Empty<int>(), inputField.text));

        Main.Setting.Save(Main.ModEntry);

        Main.Logger.Log($"The folder {inputField.text} has been created and saved to the settings file");

        LevelListScript.instance.ClearLevels();
        await LevelListScript.instance.UpdateLevelListAsync();
    }
}
