using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TUFHelper;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

public class DiffSliderQuantum : DiffSlider
{
    public class DiffSliderQuantumArgs : EventArgs
    {
        public int MinDiff { get; }
        public int MaxDiff { get; }
        public DiffSliderQuantumArgs(int minDiff, int maxDiff)
        {
            MinDiff = minDiff;
            MaxDiff = maxDiff;
        }
    }

    public event EventHandler<DiffSliderQuantumArgs> DiffSliderQuantumChanged;

    public Sprite emptySprite;

    private float _maxWidth = 330f;
    public override float MaxWidth
    {
        get => _maxWidth;
        protected set => _maxWidth = value;
    }
    public override async void OnMouseUp()
    {
        DiffSliderQuantumChanged?.Invoke(this, new(SelectedMinDiff + 1, SelectedMaxDiff + 1));
    }
    public async void Start()
    {
        List<DiffSpritePair> diffs = new();
        
        foreach (var diff in DiffSpriteHelper.DiffIDRegister)
        {
            if (DiffSpriteHelper.IsQuantumDiff(diff.Value))
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

        SelectedMinDiff = Main.Setting.MinQDiff - 1;
        SelectedMaxDiff = Main.Setting.MaxQDiff - 1;
    }

    public string[] GetQDiffs()
    {
        List<string> diffs = new();

        foreach (var diff in DiffSpriteHelper.DiffIDRegister)
        {
            if (DiffSpriteHelper.IsQuantumDiff(diff.Value))
            {
                diffs.Add($"{diff.Value}");
            }
        }

        return diffs.ToArray();
    }

    /// <summary>
    /// Returns quantum difficulties as an array of strings. The lowest minimum must be 0
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static string[] GetSelectedQDiffs(int min, int max)
    {
        string[] existingDiffs = DiffSpriteHelper.GetQuantumDiffs();

        List<string> selected = new();
        for (int i = min; i <= max; i++)
        {
            selected.Add(existingDiffs[i]);
        }

        return selected.ToArray();
    }
}
