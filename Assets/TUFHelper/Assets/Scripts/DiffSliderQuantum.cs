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
    public static DiffSliderQuantum instance;

    public Sprite emptySprite;

    private float _maxWidth = 330f;
    public override float MaxWidth
    {
        get => _maxWidth;
        protected set => _maxWidth = value;
    }
    public override async void OnMouseUp()
    {
        LevelListScript.instance.ClearLevels();

        //LevelListScript.DefaultRequest.MinDiffPGU = SelectedMinDiff + 1;
        //LevelListScript.DefaultRequest.MaxDiffPGU = SelectedMaxDiff + 1;

        Main.Setting.MaxQDiff = SelectedMaxDiff + 1;
        Main.Setting.MinQDiff = SelectedMinDiff + 1;
        Main.Setting.Save(Main.ModEntry);

        Main.Logger.Log(string.Join(',', GetSelectedQDiffs(SelectedMinDiff, SelectedMaxDiff)));

        LevelListScript.DefaultRequest.QDifficulties = GetSelectedQDiffs(SelectedMinDiff, SelectedMaxDiff).ToList();

        await LevelListScript.instance.UpdateLevelListAsync();
    }

    public void Awake()
    {
        instance = this;
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

        //LevelListScript.DefaultRequest.MinDiffPGU = SelectedMinDiff + 1;
        //LevelListScript.DefaultRequest.MaxDiffPGU = SelectedMaxDiff + 1;

        LevelListScript.DefaultRequest.QDifficulties = GetSelectedQDiffs(SelectedMinDiff, SelectedMaxDiff).ToList();

        //LevelListScript.DefaultRequest.QDifficulties = SelectedMinDiff; // QDifficulties is actually the list of strings
        //LevelListScript.DefaultRequest.QDifficulties = SelectedMaxDiff;

        //await LevelListScript.instance.UpdateLevelListAsync();
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
    public string[] GetSelectedQDiffs(int min, int max)
    {
        // Clamp values to prevent out-of-range errors
        min = Mathf.Clamp(min, 0, diffPairs.Count - 1);
        max = Mathf.Clamp(max, 0, diffPairs.Count - 1);

        List<string> selected = new List<string>();

        for (int i = min; i <= max; i++)
        {
            selected.Add(diffPairs[i].Name);
        }

        return selected.ToArray();
    }

}
