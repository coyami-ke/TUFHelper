using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TUFHelper;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

public class DiffSliderPGU : DiffSlider
{
    public class DiffSliderPGUArgs : EventArgs
    {
        public int MinDiff { get; }
        public int MaxDiff { get; }
        public DiffSliderPGUArgs(int minDiff, int maxDiff)
        {
            MinDiff = minDiff;
            MaxDiff = maxDiff;
        }
    }

    public event EventHandler<DiffSliderPGUArgs> DiffSliderPGUChanged;

    public Sprite emptySprite;

    private float _maxWidth = 500f;
    public override float MaxWidth
    {
        get => _maxWidth;
        protected set => _maxWidth = value;
    }
    public override async void OnMouseUp()
    {
        DiffSliderPGUChanged?.Invoke(this, new(SelectedMinDiff + 1, SelectedMaxDiff + 1));
    }
    public async void Start()
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

        SelectedMinDiff = Main.Setting.MinDiff - 1;
        SelectedMaxDiff = Main.Setting.MaxDiff - 1;
    }
}
