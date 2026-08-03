using System.Collections;
using System.Linq;
using TMPro;
using TUFHelper;
using TUFHelper.Utils;
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

    private float CurrentLevelSpeed
    {
        get
        {
            if (scnGame.instance == null || scnGame.instance.levelData == null)
                return 1.0f;

            float pitchFactor = scnGame.instance.levelData.pitch / 100f;
            float editorSpeed = (scnEditor.instance != null) ? scnEditor.instance.playbackSpeed : 1.0f;

            return pitchFactor * editorSpeed;
        }
    }

    protected override float MaxValue => _maxBPM;
    protected override float MinValue => _minBPM;

    private float _minBPM = 0f;
    private float _maxBPM = 100f;
    private Coroutine _calculateBoundsCoroutine;

    protected override void OnPlay(PlayButtonEventArgs e)
    {
        base.OnPlay(e);

        if (_calculateBoundsCoroutine != null)
        {
            StopCoroutine(_calculateBoundsCoroutine);
        }

        _calculateBoundsCoroutine = StartCoroutine(CalculateLevelBpmBoundsRoutine());
    }

    private IEnumerator CalculateLevelBpmBoundsRoutine()
    {
        yield return null;

        if (scnGame.instance == null || scnGame.instance.levelData == null ||
            scrLevelMaker.instance == null || scrLevelMaker.instance.listFloors == null ||
            scrLevelMaker.instance.listFloors.Count == 0)
        {
            _minBPM = 0f;
            _maxBPM = 100f;
            yield break;
        }

        float baseBpm = scnGame.instance.levelData.bpm * CurrentLevelSpeed;

        float min_bpm = float.MaxValue;
        float max_bpm = float.MinValue;

        foreach (var floor in scrLevelMaker.instance.listFloors)
        {
            if (floor == null) continue;

            float floorBpm = baseBpm * floor.speed;

            if (floorBpm < min_bpm) min_bpm = floorBpm;
            if (floorBpm > max_bpm) max_bpm = floorBpm;
        }

        _minBPM = min_bpm == float.MaxValue ? 0f : min_bpm;
        _maxBPM = max_bpm == float.MinValue ? 100f : max_bpm;

        if (Mathf.Approximately(_minBPM, _maxBPM))
        {
            _maxBPM += 0.01f;
        }
    }

    protected override void OnHit(HitMargin hit)
    {
        scrController controller = scrController.instance;
        if (controller == null || controller.currFloor == null) return;

        scrFloor targetFloor = controller.currFloor.nextfloor != null ? controller.currFloor.nextfloor : controller.currFloor;

        float baseBpm = scnGame.instance.levelData.bpm * CurrentLevelSpeed;
        float bpm = baseBpm * targetFloor.speed;

        UpdateText(bpm, "F2");
    }
}