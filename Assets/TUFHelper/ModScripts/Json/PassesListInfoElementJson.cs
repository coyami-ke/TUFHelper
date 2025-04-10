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
        public int ID { get; set; }
        public int LevelID { get; set; }
        public int Speed { get; set; }
        public int PlayerID { get; set; }
        public string FeelingRating { get; set; }
        public string VidTitle { get; set; }
        public string VideoLink { get; set; }
        public bool Is12K { get; set; }
        public bool Is16K { get; set; }
        public bool IsNoHoldTap { get; set; }
        public bool IsWorldsFirst { get; set; }
        public float Accuracy { get; set; }
        public float ScoreV2 { get; set; }
        public bool IsHidden { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsAnnounced { get; set; }
        public bool IsDuplicate { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
    }
    public class PassesListInfoElementPlayerJson
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        public string Country { get; set; }
        public bool IsBanned { get; set; }
    }
    public class PassesListInfoElementLevelJson
    {
        [JsonProperty("id")]
        public int ID { get; set; }
        public string Song { get; set; }
        public string Artist { get; set; }
        public string Creator { get; set; }
        public string Charter { get; set; }
        public string Vfxer { get; set; }
        public string Team { get; set; }
        public int DiffId { get; set; }
        public float BaseScord { get; set; }
        public bool IsCleared { get; set; }
        public int Clears { get; set; }
        public string VideoLink { get; set; }
        public string DlLink { get; set; }
        public string WorkshopLink { get; set; }
        public string PublicComments { get; set; }
        public string SubmitterDiscordId { get; set; }
        public bool ToRate { get; set; }
        public string RerateReason { get; set; }
        public string RerateNumber { get; set; }
        public int PreviousDiffId { get; set; }
        public bool IsAnnounced { get; set; }
        public bool IsDeleted { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public bool IsHidden { get; set; }
        public bool IsVerified { get; set; }
        public int TeamId { get; set; }
    }
}
 
