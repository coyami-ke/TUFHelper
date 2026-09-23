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
        // toggle.isOn = Main.Setting.ShowOnlyDownloaded;
    }
    public void Update()
    {
        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                toggle.isOn = !toggle.isOn;
            }
        }   
    }
    public async void OnValueChanged(bool value)
    {
        LevelListScript.instance.ShowOnlyDownloaded = value;
        Main.Setting.ShowOnlyDownloaded = value;

        if (value)
        {
            UITransition.Show(favoriteToggle, 0.2f, 0f);
            UITransition.Show(updateLevelsButton, 0.2f, 0.035f);
            UITransition.Show(groupByFoldersToggle, 0.2f, 0.07f);
        }
        else
        {
            UITransition.Hide(groupByFoldersToggle, 0.16f, 0f);
            UITransition.Hide(updateLevelsButton, 0.16f, 0.02f);
            UITransition.Hide(favoriteToggle, 0.16f, 0.04f);
        }

        LevelListScript.instance.ClearLevels();
        await LevelListScript.instance.UpdateLevelListAsync();
    }
}
