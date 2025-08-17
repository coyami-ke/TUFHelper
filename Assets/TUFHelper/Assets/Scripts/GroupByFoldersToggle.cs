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
        //if (toggle != null) toggle.isOn = Main.Setting.GroupByFolders;
        this.gameObject.SetActive(Main.Setting.ShowOnlyDownloaded);
    }

    public async void OnValueChanged(bool value)
    {
        if (value)
        {
            WindowsManager.instance.MoveToFolderList();
            FolderList.instance.UpdateFolderList();
            LevelListScript.instance.GroupByFolder = true;
            await LevelListScript.instance.UpdateLevelListAsync();
        }
        else
        {
            WindowsManager.instance.MoveToLevelList();
            LevelListScript.instance.GroupByFolder = false;
            await LevelListScript.instance.UpdateLevelListAsync();
        }

        Main.Setting.GroupByFolders = value;
        Main.Setting.Save(Main.ModEntry);
    }
}
