using OggVorbisEncoder.Setup;
using TMPro;
using TUFHelper;
using UnityEngine;

[RegisterIngameElement("RealBPM", "assets/tufhelper/assets/prefabs/ingamerealbpm.prefab")]
public class IngameRealBPMScript : BasicIngameElement
{
    public TextMeshProUGUI text;

    public override bool IsShownOnlyInTUFHelper => false;

    public override string NameInSettings => "Real BPM";
    public override string ID => "RealBPM";
    public override Sprite Icon => Main.assets.LoadAsset<Sprite>("assets/tufhelper/assets/sprites/speed.png");

    public override void OnSettingsOpened()
    {
    }
    protected override void OnHit(HitMargin hit)
    {
        scrFloor currentFloor = scrController.instance.currFloor.nextfloor;
        if (currentFloor == null || currentFloor.nextfloor == null) return;
        float bpm = Mathf.Abs((float)(60f / (currentFloor.nextfloor.entryTime - currentFloor.entryTime)));
        text.text = $"TBPM: " + bpm.ToString("F2");
    }
}
