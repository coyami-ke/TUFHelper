using System.Collections;
using System.Collections.Generic;
using TUFHelper;
using UnityEngine;
using UnityEngine.UI;

public class ShowDownloadedToggle : MonoBehaviour
{
    public Toggle toggle;
    public GameObject favoriteToggle;
    public void Start()
    {
        toggle.isOn = Main.Setting.ShowOnlyDownloaded;
    }
    public void OnValueChanged(bool value)
    {
        LevelListScript.instance.ShowOnlyDownloaded = value;
        Main.Setting.ShowOnlyDownloaded = value;

        favoriteToggle.SetActive(value);

        LevelListScript.instance.UpdateLevelList();
        
    }
}
