using TMPro;
using TUFHelper;
using TUFHelper.Utils;
using UnityEngine;

[RegisterIngameElement("Judgements", "assets/tufhelper/assets/prefabs/ingamejudgements.prefab")]
public class IngameJudgementsScript : BasicIngameElement
{
    public TextMeshProUGUI text;

    private int tooEarlyCount = 0;
    private int veryEarlyCount = 0;
    private int earlyPerfectCount = 0;
    private int plusPerfectCount = 0;
    private int xPerfectCount = 0;
    private int minusPerfectCount = 0;
    private int latePerfectCount = 0;
    private int veryLateCount = 0;
    private int tooLateCount = 0;
    private int missOverloadCount = 0;
    private int missDeathCount = 0;

    public override bool IsShownOnlyInTUFHelper => false;

    public override string NameInSettings => "Judgements";
    public override string ID => "Judgements";
    public override Sprite Icon => Main.assets.LoadAsset<Sprite>("assets/tufhelper/assets/sprites/number.png");
    public override Anchor DefaultAnchor => Anchor.MiddleBottom;

    private void ResetCounters()
    {
        tooEarlyCount = 0;
        veryEarlyCount = 0;
        earlyPerfectCount = 0;
        plusPerfectCount = 0;
        xPerfectCount = 0;
        minusPerfectCount = 0;
        latePerfectCount = 0;
        veryLateCount = 0;
        tooLateCount = 0;
        missOverloadCount = 0;
        missDeathCount = 0;
    }

    public override void OnSettingsOpened()
    {
        ResetCounters();
        UpdateTextDisplay();
    }

    protected override void OnPlay(PlayButtonEventArgs e)
    {
        ResetCounters();
        UpdateTextDisplay();
    }

    protected override void OnHitMargin(HitMarginEventArgs e)
    {
        if (e.Hit == HitMargin.Auto) return;

        switch (e.Hit)
        {
            case HitMargin.Perfect:
                if (e.DetailedJudge == DetailedJudge.XPerfect)
                {
                    xPerfectCount++;
                }
                else if (e.DetailedJudge == DetailedJudge.PlusPerfect)
                {
                    plusPerfectCount++;
                }
                else if (e.DetailedJudge == DetailedJudge.MinusPerfect)
                {
                    minusPerfectCount++;
                }
                break;

            case HitMargin.EarlyPerfect:
                earlyPerfectCount++;
                break;

            case HitMargin.LatePerfect:
                latePerfectCount++;
                break;

            case HitMargin.VeryEarly:
                veryEarlyCount++;
                break;

            case HitMargin.VeryLate:
                veryLateCount++;
                break;

            case HitMargin.TooEarly:
                tooEarlyCount++;
                break;

            case HitMargin.TooLate:
                tooLateCount++;
                break;

            case HitMargin.FailMiss:
                missDeathCount++;
                break;

            case HitMargin.FailOverload:
            case HitMargin.Multipress:
            case HitMargin.OverPress:
                missOverloadCount++;
                break;
        }

        UpdateTextDisplay();
    }

    private void UpdateTextDisplay()
    {
        if (text == null) return;

        text.text = $"<color=#ca69ff>{missDeathCount}</color> " +
                    $"<color=red>{tooEarlyCount}</color> " +
                    $"<color=orange>{veryEarlyCount}</color> " +
                    $"<color=yellow>{earlyPerfectCount}</color> " +
                    $"<color=green>{plusPerfectCount}</color> " +
                    $"<color=#69afff>{xPerfectCount}</color> " +
                    $"<color=green>{minusPerfectCount}</color> " +
                    $"<color=yellow>{latePerfectCount}</color> " +
                    $"<color=orange>{veryLateCount}</color> " +
                    $"<color=red>{tooLateCount}</color> " +
                    $"<color=#ca69ff>{missOverloadCount}</color>";
    }
}