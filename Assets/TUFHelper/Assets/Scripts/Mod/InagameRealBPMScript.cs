using System.Collections;
using System.Linq;
using TMPro;
using TUFHelper;
using TUFHelper.Utils;
using UnityEngine;

[RegisterIngameElement("RealBPM", "assets/tufhelper/assets/prefabs/ingamerealbpm.prefab")]
public class IngameRealBPMScript : IngameVariableScript
{
    public TextMeshProUGUI text;

    private float _minBPM = 0f;
    private float _maxBPM = 100f;
    private Coroutine _calculateBoundsCoroutine;

    public override bool IsShownOnlyInTUFHelper => false;

    public override string NameInSettings => "Real BPM";
    public override string ID => "RealBPM";
    public override Sprite Icon => Main.assets.LoadAsset<Sprite>("assets/tufhelper/assets/sprites/speed.png");
    public override Vector2 DefaultPosition => new(5, -5);
    public override Anchor DefaultAnchor => Anchor.LeftTop;

    protected override TextMeshProUGUI GetText => text;

    protected override string DefaultVariableName => "TBPM";

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

        if (scrLevelMaker.instance == null || scrLevelMaker.instance.listFloors == null || scrLevelMaker.instance.listFloors.Count == 0)
        {
            _minBPM = 0f;
            _maxBPM = 100f;
            yield break;
        }

        float baseBpm = (scnGame.instance != null && scnGame.instance.levelData != null)
            ? scnGame.instance.levelData.bpm * CurrentLevelSpeed
            : 100f * CurrentLevelSpeed;

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

        scrFloor currentFloor = controller.currFloor.nextfloor;

        if (currentFloor == null)
        {
            currentFloor = controller.currFloor;
        }

        scrFloor nextFloor = currentFloor.nextfloor;

        float bpm;

        if (nextFloor == null)
        {
            float baseBpm = (scnGame.instance != null && scnGame.instance.levelData != null)
                ? scnGame.instance.levelData.bpm * CurrentLevelSpeed
                : 100f * CurrentLevelSpeed;

            bpm = baseBpm * currentFloor.speed;
        }
        else
        {
            float rawDeltaTime = (float)(nextFloor.entryTime - currentFloor.entryTime);
            float speedAdjustedDeltaTime = rawDeltaTime / CurrentLevelSpeed;

            if (Mathf.Approximately(speedAdjustedDeltaTime, 0f))
            {
                bpm = _minBPM;
            }
            else
            {
                bpm = Mathf.Abs(60f / speedAdjustedDeltaTime);
            }
        }

        UpdateText(bpm, "F2");
    }
}