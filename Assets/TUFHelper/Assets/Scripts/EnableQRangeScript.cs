using System.Collections;
using System.Collections.Generic;
using TUFHelper;
using UnityEngine;
using UnityEngine.UI;

public class EnableQRangeScript : MonoBehaviour
{
    public Toggle toggle;
    public void Start()
    {
        toggle.isOn = Main.Setting.EnableQRange;
        toggle.onValueChanged.AddListener(OnValueChanged);
    }
    public async void OnValueChanged(bool value)
    {
        Main.Setting.EnableQRange = value;
        LevelListScript.DefaultRequest.EnableQRange = value;

        LevelListScript.instance.ClearLevels();
        await LevelListScript.instance.UpdateLevelListAsync();
    }

    private void OnDestroy()
    {
        toggle.onValueChanged.RemoveAllListeners();
    }
}
