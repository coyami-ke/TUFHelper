using System.Collections;
using System.Collections.Generic;
using TUFHelper.Utils;
using UnityEngine;
using TUFHelper;

public class TagContraierScript : MonoBehaviour
{
    public GameObject tagPrefab;

    public float tagHeight = 40;
    public float tagWidth = 130;

    public string groupName;

    public void Start()
    {
        AddTags();
        SyncFiltersFromSettings();
    }

    public void AddTags()
    {
        int i = 0;
        foreach (var pair in TagSpriteHelper.SpriteRegister)
        {
            string[] words = pair.Value.Split('_');

            if (words[1] == groupName)
            {
                GameObject obj = Instantiate(tagPrefab, gameObject.transform);
                RectTransform rect = obj.GetComponent<RectTransform>();

                var script = obj.GetComponent<TagPrefabScript>();

                var sprite = TagSpriteHelper.GetSpriteFromTag(pair.Value);
                if (sprite == null)
                {
                    Main.Logger.Error($"Sprite not found: {pair.Value}");
                }
                script.SetTagInfo(Main.Setting.SelectedQDiifs.Contains(pair.Key), pair.Key, sprite);


                rect.localScale = Vector3.one;
                rect.anchoredPosition = new Vector2(i % 3 * tagWidth, (int)(i / 3) * -tagHeight);

                i++;
            }
            else
                continue;
        }
    }
    public static void SyncFiltersFromSettings()
    {
        LevelListScript.DefaultRequest.TagsFilter =
            new List<string>(Main.Setting.SelectedQDiifs);
    }
}