using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VideoClickScript : MonoBehaviour, IPointerClickHandler
{

    public void Awake()
    {
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (LevelInfoSceneScript.currentLevelInfo != null)
        {
            Application.OpenURL(LevelInfoSceneScript.currentLevelInfo.vidLink);
        }
    }

}
