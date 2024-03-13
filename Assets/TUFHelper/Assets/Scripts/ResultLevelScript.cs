using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultLevelScript : MonoBehaviour
{
    public static ResultLevelScript instance
    {
        get
        {
            if (_instance == null)
                _instance = GameObject.Find("Canvas").transform.Find("ResultLevelList").GetComponent<ResultLevelScript>();
            
            if (_instance == null)
                _instance = FindObjectOfType<ResultLevelScript>();
            
            return _instance;
        }
    }

    private static ResultLevelScript _instance;

    public GameObject LevelNamePrefab;
    public Transform ContentParent;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            gameObject.SetActive(false);
        }
    }

    public void Close()
    {
        DownloadPopupScript.IsDownloading = false;
        
        foreach(Transform child in ContentParent)
        {
            Destroy(child.gameObject);
        }
        
        gameObject.SetActive(false);
    }


    public static void ShowList(List<string> files)
    {
        foreach (var f in files)
        {
            var name = Instantiate(instance.LevelNamePrefab, instance.ContentParent);

            name.TryGetComponent(out Button b);
            b.onClick.AddListener(() =>
            {
                scrSfx.instance?.PlaySfx(SfxSound.MobileButton);
                UIScript.SwipeToBlack(()=>LevelPrefabScript.TryToLoadLevel(f));
            });
            
            name.transform.Find("Text").TryGetComponent(out TextMeshProUGUI text);
            text.text = $"{Path.GetFileNameWithoutExtension(f)} ({DirectLevel.Utils.ByteToStringUnit(new FileInfo(f).Length)})";
        }
        
        instance.gameObject.SetActive(true);

    }
}
