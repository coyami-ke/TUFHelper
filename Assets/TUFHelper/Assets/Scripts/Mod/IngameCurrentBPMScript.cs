using TMPro;
using TUFHelper;
using UnityEngine;

[RegisterIngameElement("CurrentBPM", "assets/tufhelper/assets/prefabs/ingamecurrentbpm.prefab")]
public class IngameCurrentBPMScript : BasicIngameElement
{
    public TextMeshProUGUI text;

    public override bool IsShownOnlyInTUFHelper => false;

    public override string NameInSettings => "Current BPM";
    public override string ID => "CurrentBPM";
    public override Sprite Icon => Main.assets.LoadAsset<Sprite>("assets/tufhelper/assets/sprites/speed.png");
    public override Vector2 DefaultPosition => new(0, -25);

    public override void OnSettingsOpened()
    {
    }
    protected override void OnHit(HitMargin hit)
    {
        scrFloor currentFloor = scrController.instance.currFloor.nextfloor;
        float bpm = scnGame.instance.levelData.bpm * currentFloor.speed;
        text.text = $"CBPM: " + bpm.ToString("F2");
    }
}
