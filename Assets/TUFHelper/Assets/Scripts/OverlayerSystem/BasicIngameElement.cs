using System;
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

    public IngameElementModel Model { get; private set; }

    public virtual string ID => GetType().Name;
    public virtual string NameInSettings => ID;
    public virtual Sprite Icon => null;
    public virtual Vector2 DefaultPosition => Vector2.zero;

    protected virtual void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        parentRectTransform = transform.parent as RectTransform;
        canvas = GetComponentInParent<Canvas>();

        if (Main.Setting.IngameElementsSettings.ContainsKey(ID))
        {
            Model = Main.Setting.IngameElementsSettings[ID];
        }
        else
        {
            Model = new IngameElementModel() { Position = DefaultPosition.ToSystem() };
            Main.Setting.IngameElementsSettings[ID] = Model;
        }

        OnLoadCustomSettings(Model);

        rectTransform.anchoredPosition = Model.Position.ToUnity();
        rectTransform.localScale = new Vector3(Model.Scale, Model.Scale, 1f);

        UpdateVisibility();

        Model.PropertyChanged += Model_PropertyChanged;

        ADOFAIGameplayHandler.Editor_PlayButtonPressed += HandlePlay;
        ADOFAIGameplayHandler.Editor_Hit += HandleHit;
        ADOFAIGameplayHandler.Editor_ScnGameTransferToEditor += HandleReturnToEditor;

        if (gameObject.activeSelf && ADOFAIGameplayHandler.EditorPlayPatch.CurrentLevelInfo != null)
        {
            var dummyArgs = new PlayButtonEventArgs(ADOFAIGameplayHandler.EditorPlayPatch.CurrentLevelInfo);
            OnPlay(dummyArgs);
        }
    }

    private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "IsShowed": UpdateVisibility(); break;
            case "Position": rectTransform.anchoredPosition = Model.Position.ToUnity(); break;
            case "Scale": rectTransform.localScale = new Vector3(Model.Scale, Model.Scale, 1f); break;
        }
        Main.Setting.Save(Main.ModEntry);
    }

    public void UpdateVisibility()
    {
        if (Model == null) return;

        bool globalOverlayerActive = Main.Setting.ShowTUFHelperOverlayer;
        bool shouldShow = globalOverlayerActive && Model.IsShowed && ShouldElementBeVisible();

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

    private void HandleReturnToEditor(object sender, ScnGameTransferToEditorEventArgs e)
    {
        gameObject.SetActive(false);
        OnReturnToEditor(e);
    }

    #endregion

    #region Virtual Lifecycle Hooks 

    protected virtual void OnPlay(PlayButtonEventArgs e) { }

    protected virtual void OnHit(HitMargin hit) { }

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
    }
}