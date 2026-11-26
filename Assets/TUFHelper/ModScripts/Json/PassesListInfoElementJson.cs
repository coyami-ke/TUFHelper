using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TUFHelper.ModScripts.Json
{
    public class PassesListInfoElementJson
    {
        public int id;
        public float speed;
        public string feelingRating;
        public string videoLink;
        public float accuracy;
        public float scoreV2;
        public DateTime createdAt;
        public PassesListInfoElementPlayerJson player;
        public PassesListInfoElementJudgementsJson judgements;
        public PassesListInfoElementScoreJson scoreInfo;
        public PassesListInfoElementRanksJson ranks;
    }

    public class PassesListInfoElementPlayerJson
    {
        public string name;
        public string country;
        public string pfp;
    }

    public class PassesListInfoElementJudgementsJson
    {
        public int earlyDouble;
        public int earlySingle;
        public int ePerfect;
        public int perfect;
        public int lPerfect;
        public int lateSingle;
        public int lateDouble;
        public int deaths;
    }

    public class PassesListInfoElementScoreJson
    {
        public float currentRankedScore;
        public float previousRankedScore;
        public float impact;
    }

    public class PassesListInfoElementRanksJson
    {
        public int rankedScoreRank;
    }
}