using System.Collections;
using System.Collections.Generic;
using TUFHelper;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

public class DiffSliderPGU : DiffSlider
{
    public static DiffSliderPGU instance;

    public Sprite emptySprite;

    private float _maxWidth = 500f;
    public override float MaxWidth
    {
        get => _maxWidth;
        protected set => _maxWidth = value;
    }
    public override void OnMouseUp()
    {
        LevelListScript.instance.ClearLevels();

        LevelListScript.DefaultRequest.MinDiffPGU = SelectedMinDiff + 1;
        LevelListScript.DefaultRequest.MaxDiffPGU = SelectedMaxDiff + 1;

        LevelListScript.instance.UpdateLevelList();
    }
    public void Awake()
    {
        instance = this;
    }
    public void Start()
    {
        List<DiffSpritePair> diffs = new();
        
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
    }
}
