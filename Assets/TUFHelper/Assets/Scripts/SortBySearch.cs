using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                if (dropdown.options.Count - 1 != dropdown.value) dropdown.value++;
                else dropdown.value = 0;
                dropdown.RefreshShownValue();
            }
        }  
    }
    public async void OnValueChanged(int index)
    {
        Main.Setting.SortBy = index;
        Main.Setting.Save(Main.ModEntry);
        switch (index)
        {
            case 0:
                await SetWebRequest("RECENT");
                break;
            case 1:
                await SetWebRequest("DIFF");
                break;
            case 2:
                await SetWebRequest("CLEARS");
                break;
            case 3:
                await SetWebRequest("LIKES");
                break;
        }
    }
    public async Task SetWebRequest(string sortBy)
    {

        LevelListScript.DefaultRequest.SortBy = sortBy;

        LevelListScript.instance.ClearLevels();
        await LevelListScript.instance.UpdateLevelListAsync();
    }
}
