using System.Collections;
using System.Collections.Generic;
using TUFHelper;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FavouriteScript : MonoBehaviour, IPointerClickHandler
{

    public Image favImage;
    public Sprite starSprite, fullStarSprite;
    public LevelPrefabScript lps;

    public void Awake()
    {
        StartCoroutine(UpdateIconCo());
    }

    public IEnumerator UpdateIconCo()
    {
        yield return new WaitForEndOfFrame();

        bool isFav = Main.Setting.favouritesID.Contains(lps.levelInfo.ID);
        favImage.sprite = isFav ? fullStarSprite : starSprite;
        favImage.color = isFav ? new Color(255 / 255f, 193 / 255f, 7 / 255f, 1) : new Color(1, 1, 1, 128 / 255f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Main.Setting.favouritesID.Contains(lps.levelInfo.ID))
        {
            Main.Setting.favouritesID.Remove(lps.levelInfo.ID);
        }
        else
        {
            Main.Setting.favouritesID.Add(lps.levelInfo.ID);
        }
        Main.Setting.Save(Main.ModEntry);
        StartCoroutine(UpdateIconCo());
    }

}
