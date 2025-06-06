using System.Collections;
using System.Collections.Generic;
using TMPro;
using TUFHelper;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour
{
    public static LevelSelector instance;

    public GameObject levelPrefab, levelListParent, verticalScroll;

    private bool _isShow;
    public bool IsShow
    {
        get => _isShow;
        set
        {
            _isShow = value;
            if (value)
            {
                this.gameObject.SetActive(true);
            }
            else
            {
                this.gameObject.SetActive(false);
            }
        }
    }

    public void Awake()
    {
        instance = this;
        IsShow = false;
    }

    public void LoadLevels(List<string> levels)
    {
        IsShow = true;
        for (int i = 0; i < levelListParent.transform.childCount; i++)
        {
            Destroy(levelListParent.transform.GetChild(i).gameObject);
        }

        int count = 0;
        foreach (var level in levels)
        {
            GameObject obj = Instantiate(levelPrefab);
            obj.GetComponent<SelectLevelPrefabScript>().SetLevel(level);

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.SetParent(levelListParent.transform);

            rect.localScale = Vector3.one;
            rect.offsetMin = new Vector2(0, 0);
            rect.offsetMax = new Vector2(0, 0);
            rect.sizeDelta = new Vector2(0, 120);
            rect.sizeDelta = new(400, 62.5f);
            rect.anchoredPosition = new Vector3(0, (count * - 62.5f) -50);



            count++;
        }

        RectTransform contentRect = levelListParent.GetComponent<RectTransform>();
        float totalHeight = count * 62.5f + 70;
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);
    }
    public IEnumerator LoadLevelsCo(List<string> levels)
    {
        yield return new WaitUntil(() => TMPro.TMP_Settings.instance != null);
        yield return new WaitForEndOfFrame();

        LoadLevels(levels);

        Main.Logger.Log("Yippie");
    }

    public void Hide()
    {
        IsShow = false;
    }
}
