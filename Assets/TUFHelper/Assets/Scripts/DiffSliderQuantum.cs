using System.Collections;
using System.Collections.Generic;
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

        //Main.Setting.MaxDiff = LevelListScript.DefaultRequest.MaxDiffPGU;
        //Main.Setting.MinDiff = LevelListScript.DefaultRequest.MinDiffPGU;
        //Main.Setting.Save(Main.ModEntry);

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

        int count = diffs.Count;

        SelectedMinDiff = Mathf.Clamp(0, 0, count - 1);
        SelectedMaxDiff = Mathf.Clamp(1, 0, count - 1);

        /*SelectedMinDiff = 0;*//*Main.Setting.MinDiff - 1;*/
        /*SelectedMaxDiff = 1;*//*Main.Setting.MaxDiff - 1;*/

        //LevelListScript.DefaultRequest.MinDiffPGU = SelectedMinDiff + 1;
        //LevelListScript.DefaultRequest.MaxDiffPGU = SelectedMaxDiff + 1;

        await LevelListScript.instance.UpdateLevelListAsync();
    }
}
