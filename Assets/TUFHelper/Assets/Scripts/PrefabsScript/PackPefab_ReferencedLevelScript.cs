using TMPro;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.UI;

public class PackPefab_ReferencedLevelScript : MonoBehaviour
{
    public TextMeshProUGUI songText;
    public Image diffImage;

    public void SetLevelInfo(PackReferenceLevelJson info)
    {
        songText.text = info.ReferencedLevel.Song;
        diffImage.sprite = Main.GetSpriteFromAssets(DiffSpriteHelper.GetSpriteFromId(info.ReferencedLevel.DiffID));
    }
}
