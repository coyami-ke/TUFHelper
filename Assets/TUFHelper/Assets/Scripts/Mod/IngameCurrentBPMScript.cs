using TMPro;
using TUFHelper;
using UnityEngine;

[RegisterIngameElement("CurrentBPM", "assets/tufhelper/assets/prefabs/ingamecurrentbpm.prefab")]
public class IngameCurrentBPMScript : IngameVariableScript
{
    public TextMeshProUGUI text;

    public override bool IsShownOnlyInTUFHelper => false;

    public override string NameInSettings => "Current BPM";
    public override string ID => "CurrentBPM";
    public override Sprite Icon => Main.assets.LoadAsset<Sprite>("assets/tufhelper/assets/sprites/speed.png");
    public override Vector2 DefaultPosition => new(0, -12);
    public override Anchor DefaultAnchor => Anchor.LeftTop;

    protected override TextMeshProUGUI GetText => text;

    protected override string DefaultVariableName => "CBPM";

    protected override void OnHit(HitMargin hit)
    {
        scrFloor currentFloor = scrController.instance.currFloor.nextfloor;
        float bpm = scnGame.instance.levelData.bpm * currentFloor.speed;

        UpdateText(bpm.ToString("F2"));
    }
}
