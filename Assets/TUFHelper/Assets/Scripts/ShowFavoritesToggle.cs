using System.Collections;
using System.Collections.Generic;
using TUFHelper;
using UnityEngine;
using UnityEngine.UI;

public class ShowFavoritesToggle : MonoBehaviour
{
    public Toggle toggle;
    public void Start()
    {
        this.gameObject.SetActive(Main.Setting.ShowOnlyDownloaded);
        toggle.isOn = Main.Setting.ShowOnlyFavorites;
    }
    public async void OnValueChanged(bool value)
    {
        LevelListScript.instance.ShowOnlyFavorites = value;
        Main.Setting.ShowOnlyFavorites = value;

        await LevelListScript.instance.UpdateLevelListAsync();

        Main.Logger.Log("value changed");
    }
}
