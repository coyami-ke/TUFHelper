using System.Collections;
using System.Collections.Generic;
using TMPro;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelFolderPrefabScript : MonoBehaviour, IPointerClickHandler
{
    public LevelFolder FolderInfo { get; private set; }

    public TextMeshProUGUI nameFolderText;
    public void SetFolderInfo(LevelFolder info)
    {
        FolderInfo = info;
        nameFolderText.text = info.Name;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        LevelListScript.instance.GroupByFolder = true;
        LevelListScript.instance.LevelFolder = FolderInfo;

        LevelListScript.instance.ClearLevels();
        LevelListScript.instance.UpdateLevelList();

        WindowsManager.instance.MoveToLevelList();
    }
}
