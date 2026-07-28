using System;
using System.Collections.Generic;
using System.ComponentModel;
using TUFHelper;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public abstract class BasicIngameElement : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    private RectTransform rectTransform;
    private RectTransform parentRectTransform;
    private Canvas canvas;
    private Vector2 dragOffset;
    private GameObject[] settingsHandles;

    private bool isSelected = false;
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            if (value == isSelected) return;
            foreach (var handle in settingsHandles)
            {
                if (value) handle.GetComponent<Image>().color = Color.yellow;
                else handle.GetComponent<Image>().color = Color.green;
            }
            isSelected = value;
        }
    }

    public IngameElementModel Model { get; private set; }
    public bool IsInSettings { get; set; } = false;

    public virtual string ID => GetType().Name;
    public virtual string NameInSettings => ID;
    public virtual Sprite Icon => null;
    public virtual Vector2 DefaultPosition => Vector2.zero;
    public virtual Anchor DefaultAnchor => Anchor.Center;
    public abstract bool IsShownOnlyInTUFHelper { get; }

    // Change Start to Awake for structural caching
    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentRectTransform = transform.parent as RectTransform;
        canvas = GetComponentInParent<Canvas>();
    }

    protected virtual void Start()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (parentRectTransform == null) parentRectTransform = transform.parent as RectTransform;

        if (Main.Setting.IngameElementsSettings.ContainsKey(ID))
        {
            Model = Main.Setting.IngameElementsSettings[ID];
        }
        else
        {
            Model = new IngameElementModel() { Position = DefaultPosition.ToSystem(), Anchor = DefaultAnchor };
            Main.Setting.IngameElementsSettings[ID] = Model;
            Main.Setting.Save(Main.ModEntry);
        }

        OnLoadCustomSettings(Model);

        rectTransform.anchoredPosition = Model.Position.ToUnity();
        UpdateAnchorAndPivot(Model.Anchor);
        rectTransform.localScale = new Vector3(Model.Scale, Model.Scale, 1f);

        Canvas.ForceUpdateCanvases();

        UpdateVisibility();

        Model.PropertyChanged += Model_PropertyChanged;

        ADOFAIGameplayHandler.Editor_PlayButtonPressed += HandlePlay;
        ADOFAIGameplayHandler.Editor_Hit += HandleHit;
        ADOFAIGameplayHandler.Editor_ScnGameTransferToEditor += HandleReturnToEditor;
        ADOFAIGameplayHandler.Editor_HitMargin += HandleHitMargin;

        if (gameObject.activeSelf && ADOFAIGameplayHandler.EditorPlayPatch.CurrentLevelInfo != null)
        {
            var dummyArgs = new PlayButtonEventArgs(ADOFAIGameplayHandler.EditorPlayPatch.CurrentLevelInfo, ADOFAIGameplayHandler.IsFromTUFHelper);
            OnPlay(dummyArgs);
        }
    }

    public readonly Dictionary<Anchor, Vector2> PivotsForAnchors = new()
    {
        { Anchor.LeftTop,      new Vector2(0.0f, 1.0f) },
        { Anchor.MiddleTop,    new Vector2(0.5f, 1.0f) },
        { Anchor.RightTop,     new Vector2(1.0f, 1.0f) },

        { Anchor.LeftMiddle,   new Vector2(0.0f, 0.5f) },
        { Anchor.Center,       new Vector2(0.5f, 0.5f) },
        { Anchor.RightMiddle,  new Vector2(1.0f, 0.5f) },

        { Anchor.LeftBottom,   new Vector2(0.0f, 0.0f) },
        { Anchor.MiddleBottom, new Vector2(0.5f, 0.0f) },
        { Anchor.RightBottom,  new Vector2(1.0f, 0.0f) }
    };
    private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "IsShowed":
                UpdateVisibility();
                break;

            case "Position":
                rectTransform.anchoredPosition = Model.Position.ToUnity();
                break;

            case "Scale":
                rectTransform.localScale = new Vector3(Model.Scale, Model.Scale, 1f);
                break;

            case "Anchor":
                UpdateAnchorAndPivot(Model.Anchor);
                break;
        }

        Main.Setting.Save(Main.ModEntry);
    }

    private void UpdateAnchorAndPivot(Anchor anchor)
    {
        if (PivotsForAnchors.TryGetValue(anchor, out Vector2 targetVector))
        {
            rectTransform.anchorMin = targetVector;
            rectTransform.anchorMax = targetVector;

            SetPivotKeepPosition(rectTransform, targetVector);

            rectTransform.anchoredPosition = Model.Position.ToUnity();
        }
    }
    private void SetPivotKeepPosition(RectTransform rect, Vector2 newPivot)
    {
        Vector2 size = rect.rect.size;
        Vector2 deltaPivot = rect.pivot - newPivot;
        Vector3 deltaPosition = new(
            deltaPivot.x * size.x * rect.localScale.x,
            deltaPivot.y * size.y * rect.localScale.y,
            0f
        );

        rect.pivot = newPivot;
        rect.localPosition -= deltaPosition;
    }

    public void UpdateVisibility()
    {
        if (Model == null) return;

        bool globalOverlayerActive = Main.Setting.ShowTUFHelperOverlayer;

        bool shouldShowWithoutMod = !IsShownOnlyInTUFHelper || (ADOFAIGameplayHandler.IsFromTUFHelper || IsInSettings);

        bool shouldShow = globalOverlayerActive && Model.IsShowed && ShouldElementBeVisible() && shouldShowWithoutMod;

        if (gameObject.activeSelf != shouldShow)
            gameObject.SetActive(shouldShow);
    }

    protected virtual bool ShouldElementBeVisible() => true;

    #region Gameplay Event Wrappers

    private void HandlePlay(object sender, PlayButtonEventArgs e)
    {
        UpdateVisibility();

        if (gameObject.activeSelf)
        {
            OnPlay(e);
        }
    }

    private void HandleHit(object sender, HitMargin e)
    {
        if (gameObject.activeSelf)
        {
            OnHit(e);
        }
    }

    private void HandleHitMargin(object sender, HitMarginEventArgs e)
    {
        if (gameObject.activeSelf) // 123 line in basicingameelement
        {
            OnHitMargin(e);
        }
    }

    private void HandleReturnToEditor(object sender, ScnGameTransferToEditorEventArgs e)
    {
        gameObject.SetActive(false);
        OnReturnToEditor(e);
    }

    #endregion

    #region Virtual Lifecycle Hooks 

    protected virtual void OnPlay(PlayButtonEventArgs e) { }

    protected virtual void OnHit(HitMargin hit) { }
    protected virtual void OnHitMargin(HitMarginEventArgs e) { }

    protected virtual void OnReturnToEditor(ScnGameTransferToEditorEventArgs e) { }
    protected virtual void OnLoadCustomSettings(IngameElementModel model) { }

    public virtual void OnSettingsOpened() { }

    #endregion
    public void CreateSettingsHandles()
    {
        settingsHandles = new GameObject[8];

        Vector2[] cornerAnchors = {
            new Vector2(0, 0), // Bottom-Left
            new Vector2(1, 0), // Bottom-Right
            new Vector2(0, 1), // Top-Left
            new Vector2(1, 1)  // Top-Right
        };
        string[] cornerNames = { "BL_Handle", "BR_Handle", "TL_Handle", "TR_Handle" };

        for (int i = 0; i < 4; i++)
        {
            GameObject handle = new GameObject(cornerNames[i], typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(transform, false);

            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.anchorMin = cornerAnchors[i];
            handleRect.anchorMax = cornerAnchors[i];
            handleRect.pivot = new Vector2(0.5f, 0.5f);
            handleRect.sizeDelta = new Vector2(10f, 10f); // Size of the dot
            handleRect.anchoredPosition = Vector2.zero;

            Image img = handle.GetComponent<Image>();
            img.color = Color.green;
            img.raycastTarget = false;

            settingsHandles[i] = handle;
        }

        Vector2[] lineAnchorMins = {
            new Vector2(0, 0), // Bottom Line
            new Vector2(0, 1), // Top Line
            new Vector2(0, 0), // Left Line
            new Vector2(1, 0)  // Right Line
        };

        Vector2[] lineAnchorMaxs = {
            new Vector2(1, 0), // Bottom Line
            new Vector2(1, 1), // Top Line
            new Vector2(0, 1), // Left Line
            new Vector2(1, 1)  // Right Line
        };

        string[] lineNames = { "Bottom_Line", "Top_Line", "Left_Line", "Right_Line" };
        float lineThickness = 2f;

        for (int i = 0; i < 4; i++)
        {
            GameObject line = new GameObject(lineNames[i], typeof(RectTransform), typeof(Image));
            line.transform.SetParent(transform, false);

            RectTransform lineRect = line.GetComponent<RectTransform>();
            lineRect.anchorMin = lineAnchorMins[i];
            lineRect.anchorMax = lineAnchorMaxs[i];

            if (i == 0) lineRect.pivot = new Vector2(0.5f, 0f);      // Bottom pushes up
            else if (i == 1) lineRect.pivot = new Vector2(0.5f, 1f); // Top pushes down
            else if (i == 2) lineRect.pivot = new Vector2(0f, 0.5f); // Left pushes right
            else if (i == 3) lineRect.pivot = new Vector2(1f, 0.5f); // Right pushes left

            if (i < 2) // Horizontal lines (Bottom, Top)
            {
                lineRect.sizeDelta = new Vector2(0f, lineThickness);
            }
            else // Vertical lines (Left, Right)
            {
                lineRect.sizeDelta = new Vector2(lineThickness, 0f);
            }

            lineRect.anchoredPosition = Vector2.zero;

            Image img = line.GetComponent<Image>();
            img.color = Color.green;
            img.raycastTarget = false;

            settingsHandles[4 + i] = line;
        }
    }

    #region Drag & Clamp Implementation

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (canvas == null || Model == null) return;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, eventData.position, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out Vector2 localMousePos))
        {
            dragOffset = Model.Position.ToUnity() - localMousePos;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null || Model == null || parentRectTransform == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, eventData.position, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out Vector2 localMousePos))
        {
            Vector2 targetLocalPos = localMousePos + dragOffset;
            rectTransform.anchoredPosition = targetLocalPos;

            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

            Vector3[] elementCorners = new Vector3[4];
            Vector3[] parentCorners = new Vector3[4];

            rectTransform.GetWorldCorners(elementCorners);
            parentRectTransform.GetWorldCorners(parentCorners);

            float elementWidth = elementCorners[2].x - elementCorners[0].x;
            float elementHeight = elementCorners[2].y - elementCorners[0].y;

            float clampedMinX = Mathf.Clamp(elementCorners[0].x, parentCorners[0].x, parentCorners[2].x - elementWidth);
            float clampedMinY = Mathf.Clamp(elementCorners[0].y, parentCorners[0].y, parentCorners[2].y - elementHeight);

            Vector3 worldDelta = new Vector3(clampedMinX - elementCorners[0].x, clampedMinY - elementCorners[0].y, 0f);

            rectTransform.position += worldDelta;

            Model.Position = rectTransform.anchoredPosition.ToSystem();
        }
    }
    #endregion

    protected virtual void OnDestroy()
    {
        if (Model != null) Model.PropertyChanged -= Model_PropertyChanged;

        ADOFAIGameplayHandler.Editor_PlayButtonPressed -= HandlePlay;
        ADOFAIGameplayHandler.Editor_Hit -= HandleHit;
        ADOFAIGameplayHandler.Editor_ScnGameTransferToEditor -= HandleReturnToEditor;
        ADOFAIGameplayHandler.Editor_HitMargin -= HandleHitMargin;
    }
}