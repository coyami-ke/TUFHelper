using System.Collections;
using System.Collections.Generic;
using TMPro;
using TUFHelper;
using TUFHelper.ModScripts.Web;
using UnityEngine;

public class SortOrderSearch : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    public void Start()
    {
        dropdown.value = (int)Main.Setting.SortOrder;
        dropdown.RefreshShownValue();
    }
    public void OnValueChanged(int index)
    {
        switch (index)
        {
            case 0:
                SetWebRequest(AscendingOrDescending.Ascending);
                break;
            case 1:
                SetWebRequest(AscendingOrDescending.Descending);
                break;
        }
    }
    public async void SetWebRequest(AscendingOrDescending order)
    {
        LevelListScript.DefaultRequest.SortAsc = order;
        Main.Setting.SortOrder = order;
        Main.Setting.Save(Main.ModEntry);

        LevelListScript.instance.ClearLevels();
        await LevelListScript.instance.UpdateLevelListAsync();
    }
}
