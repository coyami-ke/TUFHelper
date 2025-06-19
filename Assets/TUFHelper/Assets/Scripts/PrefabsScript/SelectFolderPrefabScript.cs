using System.Collections;
using System.Collections.Generic;
using TMPro;
using TUFHelper;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.UI;

public class SelectFolderPrefabScript : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Toggle toggle;
    private LevelFolder folderInfo;
    private int _idLevel;

    public void SetFolderInfo(LevelFolder folder, int idLevel)
    {
        folderInfo = folder;
        text.text = folder.Name;
        _idLevel = idLevel;

        toggle.isOn = folderInfo.Levels.Contains(_idLevel);
    }

    public void OnValueChanged(bool value)
    {
        if (value)
        {
            folderInfo.Levels.Add(_idLevel);
            Main.Logger.Log("added?");
        }
        else
        {
            folderInfo.Levels.Remove(_idLevel);
            Main.Logger.Log("removed?");
        }

        Main.Setting.Save(Main.ModEntry);
    }
}
