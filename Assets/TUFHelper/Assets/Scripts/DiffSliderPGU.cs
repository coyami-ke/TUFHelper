using System.Collections;
using System.Collections.Generic;
using TUFHelper;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

public class DiffSliderPGU : DiffSlider
{
    public Sprite emptySprite;

    private float _maxWidth = 400f;
    public override float MaxWidth
    {
        get => _maxWidth;
        protected set => _maxWidth = value;
    }

    public void Start()
    {
        List<DiffSpritePair> diffs = new();
        Debug.Log(diffs.Count); // shows
        foreach (var diff in DiffSpriteHelper.DiffIDRegister)
        {
            if (!DiffSpriteHelper.IsSpecialDiff(diff.Value))
            {
                string path = DiffSpriteHelper.GetSpriteFromId(diff.Key);
                Sprite sprite;
                if (Main.assets == null) 
                    sprite = emptySprite;
                else 
                    sprite = Main.GetSpriteFromAssets(path);
                diffs.Add(new(diff.Value, sprite));
            }
        }
        
        Init(diffs);
        Debug.LogWarning("warn"); // doesnt show
    }
}
