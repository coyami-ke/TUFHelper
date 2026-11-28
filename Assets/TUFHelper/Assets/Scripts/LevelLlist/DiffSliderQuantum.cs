using System;
using System.Collections.Generic;
using TUFHelper;
using TUFHelper.Utils;
using UnityEngine;

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

    public override void OnMouseUp()
    {
        DiffSliderQuantumChanged?.Invoke(
            this,
            new DiffSliderQuantumArgs(SelectedMinDiff + 1, SelectedMaxDiff + 1)
        );
    }

    public void Start()
    {
        List<DiffSpritePair> diffs = new();

        foreach (var diff in DiffSpriteHelper.DiffIDRegister)
        {
            if (DiffSpriteHelper.IsQuantumDiff(diff.Value))
            {
                string path = DiffSpriteHelper.GetSpriteFromId(diff.Key);
                Sprite sprite = Main.assets == null
                    ? emptySprite
                    : Main.GetSpriteFromAssets(path) ?? emptySprite;

                diffs.Add(new DiffSpritePair(diff.Value, sprite));
            }
        }

        Init(diffs);

        if (diffs.Count == 0)
            return;

        int maxIndex = diffs.Count - 1;
        int minIdx = Mathf.Clamp(Main.Setting.MinQDiff - 1, 0, maxIndex);
        int maxIdx = Mathf.Clamp(Main.Setting.MaxQDiff - 1, minIdx, maxIndex);

        SelectedMinDiff = minIdx;
        SelectedMaxDiff = maxIdx;
    }

    public string[] GetQDiffs()
    {
        List<string> diffs = new();

        foreach (var diff in DiffSpriteHelper.DiffIDRegister)
        {
            if (DiffSpriteHelper.IsQuantumDiff(diff.Value))
            {
                diffs.Add(diff.Value);
            }
        }

        return diffs.ToArray();
    }

    /// <summary>
    /// Converts 1-based slider values (SelectedMinDiff + 1, SelectedMaxDiff + 1)
    /// into selected quantum difficulty names.
    /// </summary>
    public static string[] GetSelectedQDiffs(int minOneBased, int maxOneBased)
    {
        string[] existingDiffs = DiffSpriteHelper.GetQuantumDiffs();
        if (existingDiffs == null || existingDiffs.Length == 0)
            return Array.Empty<string>();

        int lastIndex = existingDiffs.Length - 1;
        int startIdx = Mathf.Clamp(minOneBased - 1, 0, lastIndex);
        int endIdx = Mathf.Clamp(maxOneBased - 1, startIdx, lastIndex);

        List<string> selected = new(endIdx - startIdx + 1);
        for (int i = startIdx; i <= endIdx; i++)
        {
            selected.Add(existingDiffs[i]);
        }

        return selected.ToArray();
    }
}