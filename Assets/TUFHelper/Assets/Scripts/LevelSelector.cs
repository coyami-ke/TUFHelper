using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour
{
    public static LevelSelector instance;

    public GameObject levelPrefab;
    public GameObject levelListParent;

    public RectTransform visualGlow2;

    public RectTransform characterRect;

    public VisualLevelPrefab visualLevel;

    private float progress;
    private Vector2 visualGlow2_originalPos;
    private float character_scale;

    public LevelListInfoElementJson LevelInfo { get; set; }

    private bool _isShow;
    public bool IsShow
    {
        get => _isShow;
        set
        {
            _isShow = value;
            //gameObject.SetActive(value);
            if (value)
            {
                UITransition.Show(this.gameObject);
                CustomMusicPlayer.instance.EnableReverb();
            }
            else
            {
                UITransition.Hide(this.gameObject);
                CustomMusicPlayer.instance.DisableReverb();
            }
        }
    }

    private void Awake()
    {
        instance = this;
        this.gameObject.SetActive(false);
        //IsShow = false;
    }
    private void Start()
    {
        visualGlow2_originalPos = visualGlow2.anchoredPosition;
        character_scale = characterRect.localScale.y;
    }
    private void Update()
    {
        progress += Time.deltaTime / 2.67f;
        visualGlow2.anchoredPosition = new(
            (Mathf.PerlinNoise1D(progress) - 0.5f) * 32 + visualGlow2_originalPos.x, 
            (Mathf.PerlinNoise1D(progress + 125f) - 0.5f) * 128 + visualGlow2_originalPos.y);
        visualGlow2.rotation = Quaternion.Euler(0, 0, (Mathf.PerlinNoise1D(progress + 250) - 0.5f) * 8);

        characterRect.ScaleY(character_scale + ((1 + Mathf.Cos(progress * 4)) / 2 / 24f));
    }

    public void LoadLevels(List<string> levels, LevelListInfoElementJson info, string packId = null, int packLevelID = -1)
    {
        visualLevel.SetLevelInfo(info);

        visualGlow2.GetComponent<Image>().color = new(visualLevel.backgroundImage.color.r, visualLevel.backgroundImage.color.g, visualLevel.backgroundImage.color.b, 78f / 255);

        IsShow = true;

        for (int i = levelListParent.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(levelListParent.transform.GetChild(i).gameObject);
        }

        int count = 0;
        foreach (var levelPath in levels)
        {
            GameObject obj = Instantiate(levelPrefab);

            SelectLevelPrefabScript prefabScript = obj.GetComponent<SelectLevelPrefabScript>();
            prefabScript.SetLevel(levelPath, info, packId, packLevelID);
            prefabScript.levelName.DOText(prefabScript.levelName.text, 0.5f, false).From("").SetEase(Ease.Linear).SetDelay(count / 4);

            CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();
            canvasGroup.DOFade(1f, 0.33f).From(0f).SetEase(Ease.OutExpo).SetDelay(count / 4f);

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.SetParent(levelListParent.transform, false);
            rect.anchoredPosition = new Vector2(-50, count * -120f);
            rect.DOAnchorPosX(0, 0.44f).SetEase(Ease.OutBack).SetDelay(count / 4f);

            count++;
        }

        //RectTransform contentRect = levelListParent.GetComponent<RectTransform>();
        //contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, count * 90f);
    }

    public IEnumerator LoadLevelsCo(List<string> levels, LevelListInfoElementJson levelInfo, string packId = null, int packLevelID = -1)
    {
        yield return new WaitUntil(() => TMPro.TMP_Settings.instance != null);
        yield return new WaitForEndOfFrame();

        LoadLevels(levels, levelInfo, packId, packLevelID);
    }

    public void Hide()
    {
        IsShow = false;
    }
}