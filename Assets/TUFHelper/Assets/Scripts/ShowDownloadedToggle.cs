using System.Collections;
using System.Collections.Generic;
using TUFHelper;
using UnityEngine;
using UnityEngine.UI;

public class ShowDownloadedToggle : MonoBehaviour
{
    public Toggle toggle;
    public GameObject favoriteToggle, updateLevelsButton, groupByFoldersToggle;
    public void Start()
    {
        toggle.isOn = Main.Setting.ShowOnlyDownloaded;
    }
    public void Update()
    {
        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
        {
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                toggle.isOn = !toggle.isOn;
            }
        }   
    }
    public void OnValueChanged(bool value)
    {
        LevelListScript.instance.ShowOnlyDownloaded = value;
        Main.Setting.ShowOnlyDownloaded = value;

        favoriteToggle.SetActive(value);
        updateLevelsButton.SetActive(value);
        groupByFoldersToggle.SetActive(value);

        LevelListScript.instance.UpdateLevelList();

    }
}
