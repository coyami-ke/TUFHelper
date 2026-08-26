using System.Collections;
using System.Linq;
using TMPro;
using TUFHelper;
using TUFHelper.Utils;
using UnityEngine;

[RegisterIngameElement("Progress", "assets/tufhelper/assets/prefabs/ingameprogress.prefab")]
public class IngameProgressScript : IngameVariableScript
{
    public TextMeshProUGUI text;

    public override bool IsShownOnlyInTUFHelper => false;

    public override string NameInSettings => "Progress";
    public override string ID => "Progress";
    public override Sprite Icon => Main.assets.LoadAsset<Sprite>("assets/tufhelper/assets/sprites/number.png");
    public override Vector2 DefaultPosition => new(2.5f, -2.5f - 24f);
    public override Anchor DefaultAnchor => Anchor.LeftTop;
    public override TextMeshProUGUI[] Texts => new TextMeshProUGUI[1] { text };

    protected override TextMeshProUGUI GetText => text;

    protected override string DefaultVariableName => "Progress";

    protected override float MaxValue => 100;
    protected override float MinValue => 0;

    public override void OnSettingsOpened()
    {
        UpdateText(50, "F2");
    }

    protected override void OnPlay(PlayButtonEventArgs e)
    {
        try
        {
            float current = scrController.instance.currFloor.seqID;
            float total = scrLevelMaker.instance.listFloors.Last().seqID;

            UpdateText((current / (total - 1)) * 100.0f, "F2", "%");
        }
        catch { }
    }

    protected override void OnHit(HitMargin hit)
    {
        float current = scrController.instance.currFloor.seqID;
        float total = scrLevelMaker.instance.listFloors.Last().seqID;

        UpdateText((current / (total - 1)) * 100.0f, "F2", "%");
    }
}