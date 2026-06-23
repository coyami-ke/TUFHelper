using System;
using System.ComponentModel;
using TUFHelper;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public abstract class BasicIngameElement : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    private RectTransform rectTransform;
    private RectTransform parentRectTransform;
    private Canvas canvas;
    private Vector2 dragOffset;

    public IngameElementModel Model { get; private set; }

    public virtual string ID => GetType().Name;

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
            Model = new IngameElementModel();
            Main.Setting.IngameElementsSettings[ID] = Model;
        }

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
        bool ratingPageBlocking = FrontPageScript.instance != null;
        bool shouldShow = globalOverlayerActive && Model.IsShowed && !ratingPageBlocking && ShouldElementBeVisible();

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

    #endregion

    #region Drag Implementation
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
        if (canvas == null || Model == null) return;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, eventData.position, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out Vector2 localMousePos))
        {
            Model.Position = localMousePos.ToSystem() + dragOffset.ToSystem();
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