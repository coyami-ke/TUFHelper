using System.Collections;
using System.Collections.Generic;
using TMPro;
using TUFHelper;
using UnityEngine;

public class SortBySearch : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    public void Start()
    {
        dropdown.value = Main.Setting.SortBy;
        dropdown.RefreshShownValue();
    }
    public void Update()
    {
        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (dropdown.options.Count - 1 != dropdown.value) dropdown.value++;
                else dropdown.value = 0;
                dropdown.RefreshShownValue();
            }
        }  
    }
    public void OnValueChanged(int index)
    {
        Main.Setting.SortBy = index;
        Main.Setting.Save(Main.ModEntry);
        switch (index)
        {
            case 0:
                SetWebRequest("RECENT");
                break;
            case 1:
                SetWebRequest("DIFF");
                break;
            case 2:
                SetWebRequest("CLEARS");
                break;
            case 3:
                SetWebRequest("LIKES");
                break;
        }
    }
    public void SetWebRequest(string sortBy)
    {

        LevelListScript.DefaultRequest.SortBy = sortBy;

        LevelListScript.instance.ClearLevels();
        LevelListScript.instance.UpdateLevelList();
    }
}
