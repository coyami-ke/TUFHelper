using System.Collections;
using System.Collections.Generic;
using TUFHelper;
using UnityEngine;
using UnityEngine.UI;

public class GroupByFoldersToggle : MonoBehaviour
{
    public Toggle toggle;

    public void Start()
    {
        toggle.isOn = Main.Setting.GroupByFolders;
        this.gameObject.SetActive(Main.Setting.ShowOnlyDownloaded);
    }

    public void OnValueChanged(bool value)
    {
        if (value)
        {
            WindowsManager.instance.MoveToFolderList();
            FolderList.instance.UpdateFolderList();
            LevelListScript.instance.GroupByFolder = true;
            LevelListScript.instance.UpdateLevelList();
        }
        else
        {
            WindowsManager.instance.MoveToLevelList();
            LevelListScript.instance.GroupByFolder = false;
            LevelListScript.instance.UpdateLevelList();
        }

        Main.Setting.GroupByFolders = value;
        Main.Setting.Save(Main.ModEntry);
    }
}
