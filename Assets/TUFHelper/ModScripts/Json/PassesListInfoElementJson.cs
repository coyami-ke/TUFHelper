using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace TUFHelper.ModScripts.Json
{
    public class PassesListInfoElementJson
    {
        [JsonProperty("vidUploadTime")]
        public string VidUploadTime { get; set; }

        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("levelId")]
        public int LevelID { get; set; }

        [JsonProperty("speed")]
        public float Speed { get; set; }

        [JsonProperty("playerId")]
        public int PlayerID { get; set; }

        [JsonProperty("feelingRating")]
        public string FeelingRating { get; set; }

        [JsonProperty("vidTitle")]
        public string VidTitle { get; set; }

        [JsonProperty("videoLink")]
        public string VideoLink { get; set; }

        [JsonProperty("is12K")]
        public bool Is12K { get; set; }

        [JsonProperty("is16K")]
        public bool Is16K { get; set; }

        [JsonProperty("isNoHoldTap")]
        public bool IsNoHoldTap { get; set; }

        [JsonProperty("isWorldsFirst")]
        public bool IsWorldsFirst { get; set; }

        [JsonProperty("accuracy")]
        public float Accuracy { get; set; }

        [JsonProperty("scoreV2")]
        public float ScoreV2 { get; set; }

        [JsonProperty("isHidden")]
        public bool IsHidden { get; set; }

        [JsonProperty("isDeleted")]
        public bool IsDeleted { get; set; }

        [JsonProperty("isAnnounced")]
        public bool IsAnnounced { get; set; }

        [JsonProperty("isDuplicate")]
        public bool IsDuplicate { get; set; }

        [JsonProperty("createdAt")]
        public string CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public string UpdatedAt { get; set; }
        [JsonProperty("player")]
        public PassesListInfoElementPlayerJson Player { get; set; }
        [JsonProperty("level")]
        public PassesListInfoElementLevelJson Level { get; set; }
        [JsonProperty("judgements")]
        public PassesListInfoElementJudgementsJson Judgements { get; set; }
    }

    public class PassesListInfoElementPlayerJson
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("isBanned")]
        public bool IsBanned { get; set; }
    }

    public class PassesListInfoElementLevelJson
    {
        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("song")]
        public string Song { get; set; }

        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("creator")]
        public string Creator { get; set; }

        [JsonProperty("charter")]
        public string Charter { get; set; }

        [JsonProperty("vfxer")]
        public string Vfxer { get; set; }

        [JsonProperty("team")]
        public string Team { get; set; }

        [JsonProperty("diffId")]
        public int DiffId { get; set; }

        [JsonProperty("baseScord")]
        public float BaseScord { get; set; }

        [JsonProperty("isCleared")]
        public bool IsCleared { get; set; }

        [JsonProperty("clears")]
        public int Clears { get; set; }

        [JsonProperty("videoLink")]
        public string VideoLink { get; set; }

        [JsonProperty("dlLink")]
        public string DlLink { get; set; }

        [JsonProperty("workshopLink")]
        public string WorkshopLink { get; set; }

        [JsonProperty("publicComments")]
        public string PublicComments { get; set; }

        [JsonProperty("submitterDiscordId")]
        public string SubmitterDiscordId { get; set; }

        [JsonProperty("toRate")]
        public bool ToRate { get; set; }

        [JsonProperty("rerateReason")]
        public string RerateReason { get; set; }

        [JsonProperty("rerateNumber")]
        public string RerateNumber { get; set; }

        [JsonProperty("previousDiffId")]
        public int PreviousDiffId { get; set; }

        [JsonProperty("isAnnounced")]
        public bool IsAnnounced { get; set; }

        [JsonProperty("isDeleted")]
        public bool IsDeleted { get; set; }

        [JsonProperty("createdAt")]
        public string CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public string UpdatedAt { get; set; }

        [JsonProperty("isHidden")]
        public bool IsHidden { get; set; }

        [JsonProperty("isVerified")]
        public bool IsVerified { get; set; }

        [JsonProperty("teamId")]
        public int? TeamId { get; set; }
    }
    public class PassesListInfoElementJudgementsJson
    {
        [JsonProperty("id")]
        public int ID { get; set; }
        [JsonProperty("earlyDouble")]
        public int EarlyDouble { get; set; }
        [JsonProperty("earlySingle")]
        public int EarlySingle { get; set; }
        [JsonProperty("ePerfect")]
        public int EPerfect { get; set; }
        [JsonProperty("perfect")]
        public int Perfect { get; set; }
        [JsonProperty("lPerfect")]
        public int LPerfect { get; set; }
        [JsonProperty("lateSingle")]
        public int LateSingle { get; set; }
        [JsonProperty("lateDouble")]
        public int LateDouble { get; set; }
        [JsonProperty("createdAt")]
        public string CreatedAt { get; set; }
        [JsonProperty("updatedAt")]
        public string UpdatedAt { get; set; }
    }
}
 
