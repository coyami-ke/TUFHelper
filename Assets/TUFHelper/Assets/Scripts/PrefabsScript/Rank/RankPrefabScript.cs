using TMPro;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.UI;

public class RankPrefabScript : MonoBehaviour
{

    public TextMeshProUGUI rankText,
        playerText,
        judgementsText,
        accuracyText,
        date,
        speedText;
    public Image countryIcon;

    private PassInfo pi;


    public void Awake()
    {
        
    }

    public void SetPassInfo(int rank, string country, PassInfo pi, bool isScoreV2)
    {
        this.pi = pi;

        if (rank == 1)
        {
            rankText.text = "<color=#FFDA00>#1</color>";
        }
        else
        {
            rankText.text = "#" + rank;
        }

        if (pi.player.Equals(LeaderboardScript.firstClear))
        {
            playerText.text = "<color=#FFDA00>" + pi.player + "</color>";
        }
        else
        {
            playerText.text = pi.player;
        }

        if (pi.vidUploadTime.Equals(LeaderboardScript.firstClearTime))
        {
            date.text = "<color=#FFDA00>" + Helper.getDate(pi.vidUploadTime) + "<>";
        }
        else
        {
            date.text = Helper.getDate(pi.vidUploadTime);
        }

        countryIcon.sprite = Helper.getFlagSprite(country);

        judgementsText.text = "<color=#ED3E3E>" + pi.GetJudgements()[0] + " </color> <color=#EB9A46>" + pi.GetJudgements()[1] +
            " </color> <color=#E3E370>" + pi.GetJudgements()[2] + " </color> <color=#86E370>" + pi.GetJudgements()[3] + " </color> <color=#E3E370>" +
            pi.GetJudgements()[4] + " </color> <color=#EB9A46>" + pi.GetJudgements()[5] + " </color> <color=#ED3E3E> 0</color>";

        string accuracy = (pi.getXAcc() * 100).ToString("F2") + "%";
        if (isScoreV2)
        {
            accuracyText.text = pi.GetScoreV2().ToString("F3") + "";

        } 
        else if (accuracy.Equals("100.00%")) 
        {
            accuracyText.text = "<color=#FFDA00>100.00%</color>";
        } 
        else
        {
            accuracyText.text = accuracy;
        }
        speedText.text = pi.speed == null ? "1.00x" : (float.Parse(pi.speed).ToString("F2") + "x");



    }

    public void InfoButtonClick()
    {
        if (pi.vidLink.Contains("http"))
        {
            Application.OpenURL(pi.vidLink);
        }
    }

    public PassInfo GetPassInfo() { return pi; }
        

}
