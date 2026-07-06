using TMPro;
using TUFHelper;
using UnityEngine;

public class LanguageSettings : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    public void Start()
    {
        //dropdown.value = Main.Setting.Language;
        dropdown.RefreshShownValue();
    }
}
