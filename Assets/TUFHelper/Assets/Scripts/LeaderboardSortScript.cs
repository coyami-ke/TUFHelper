using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LeaderboardSortScript : MonoBehaviour
{

    public TextMeshProUGUI xAccLabel, scoreLabel, dateLabel, speedLabel;

    public static LeaderboardSortScript instance;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetAllLabels()
    {
        xAccLabel.color = new Color(1, 1, 1, 0.5f);
        scoreLabel.color = new Color(1, 1, 1, 0.5f);
        dateLabel.color = new Color(1, 1, 1, 0.5f);
        speedLabel.color = new Color(1, 1, 1, 0.5f);
    }

    public void XAccuraySort()
    {
        LeaderboardScript.passes = LeaderboardScript.passes.OrderByDescending(x => x.getXAcc()).ToList();
        StartCoroutine(LeaderboardScript.instance.LoadLevelPassesCo(false));

        ResetAllLabels();
        xAccLabel.color = new Color(1, 1, 1, 1f);
    }

    public void ScoreV2Sort()
    {
        LeaderboardScript.passes = LeaderboardScript.passes.OrderByDescending(x => x.GetScoreV2()).ToList();
        StartCoroutine(LeaderboardScript.instance.LoadLevelPassesCo(true));

        ResetAllLabels();
        scoreLabel.color = new Color(1, 1, 1, 1f);
    }

    public void DateSort()
    {
        LeaderboardScript.passes = LeaderboardScript.passes.OrderByDescending(x => Helper.getTimeStamp(x.vidUploadTime)).Reverse().ToList();
        StartCoroutine(LeaderboardScript.instance.LoadLevelPassesCo(false));

        ResetAllLabels();
        dateLabel.color = new Color(1, 1, 1, 1f);
    }

    public void SpeedSort()
    {
        LeaderboardScript.passes = LeaderboardScript.passes.OrderByDescending(x => x.GetSpeed()).ToList();
        StartCoroutine(LeaderboardScript.instance.LoadLevelPassesCo(false));

        ResetAllLabels();
        speedLabel.color = new Color(1, 1, 1, 1f);
    }
    
}
