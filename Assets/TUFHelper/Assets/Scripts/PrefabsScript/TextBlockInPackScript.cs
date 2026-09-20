using DG.Tweening;
using TMPro;
using TUFHelper.ModScripts.Json;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TextBlockInPackScript : MonoBehaviour
{
    public Image backgroundImage;

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public void SetTextBlockInfo(PackItemNode node)
    {
        nameText.text = node.Name;
        descriptionText.text = node.Description;
    }
}
