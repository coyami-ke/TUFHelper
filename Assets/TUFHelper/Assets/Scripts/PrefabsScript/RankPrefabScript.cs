using DirectLevel;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using TUFHelper;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RankPrefabScript : MonoBehaviour
{

    public TextMeshProUGUI rankText,
        playerText,
        judgementsText,
        accuracyText,
        speedText;
    public Image countryIcon;

    private PassInfo pi;

    public void Awake()
    {
        
    }

    public void SetPassInfo(int rank, string country, PassInfo pi)
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

        countryIcon.sprite = Helper.getFlagSprite(country);
        playerText.text = pi.player;
        judgementsText.text = "<color=#ED3E3E>" + pi.judgements[0] + " </color> <color=#EB9A46>" + pi.judgements[1] +
            " </color> <color=#E3E370>" + pi.judgements[2] + " </color> <color=#86E370>" + pi.judgements[3] + " </color> <color=#E3E370>" +
            pi.judgements[4] + " </color> <color=#EB9A46>" + pi.judgements[5] + " </color>";

        string accuracy = (Helper.calculatePercentXAcc(pi.judgements, 0) * 100).ToString("F2") + "%";
        if (accuracy.Equals("100.00%")) 
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
        Application.OpenURL(pi.vidLink);
    }

}
