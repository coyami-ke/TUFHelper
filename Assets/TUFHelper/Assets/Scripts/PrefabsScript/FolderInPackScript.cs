using DG.Tweening;
using TMPro;
using TUFHelper.ModScripts.Json;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FolderInPackScript : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image backgroundImage, arrowImage;
    public TextMeshProUGUI folderNameText, itemsText;

    private PackTreeUIController _treeController;
    private PackItemNode _node;

    public void InitTreeController(PackTreeUIController controller, PackItemNode node)
    {
        _treeController = controller;
        _node = node;
    }

    public void SetFolderInfo(PackItemNode node)
    {
        folderNameText.text = node.Name;
        itemsText.text = (node.Children != null ? node.Children.Count : 0) + " items";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_node == null) return;

        _node.IsExpanded = !_node.IsExpanded;

        arrowImage.rectTransform.DOKill();
        float targetAngle = _node.IsExpanded ? 90f : 180f;
        arrowImage.rectTransform.DORotate(new Vector3(0, 0, targetAngle), 0.2f).SetEase(Ease.OutBack);

        _treeController?.UpdateLayout();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        backgroundImage.DOKill();
        Color c = backgroundImage.color;
        c.a = 30f / 255f; // Fixed 0-1 float range
        backgroundImage.DOColor(c, 0.2f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        backgroundImage.DOKill();
        Color c = backgroundImage.color;
        c.a = 10f / 255f;
        backgroundImage.DOColor(c, 0.2f);
    }
}