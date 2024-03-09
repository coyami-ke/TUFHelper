using System.Collections;
using System.Collections.Generic;
using TUFHelper;
using UnityEngine;
using UnityEngine.EventSystems;

public class LogoScript : MonoBehaviour, IPointerClickHandler
{

    public void Awake()
    {
        
    }

    public void Update()
    {

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SettingsScript.instance.legacyDiffToggle.isOn = !Main.Setting.showLegacyRating;
    }
}
