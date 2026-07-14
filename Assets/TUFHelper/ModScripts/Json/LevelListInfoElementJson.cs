using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace TUFHelper.ModScripts.Json
{
    public class LevelListInfoElementJson
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

        [JsonProperty("ppBaseScore")]
        public float? PPBaseScore { get; set; }
        [JsonProperty("baseScore")]
        public float? BaseScore { get; set; }
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

        [JsonProperty("rerateNum")]
        public string RerateNum { get; set; }

        [JsonProperty("previousDiffId")]
        public int? PreviousDiffId { get; set; }

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
        [JsonProperty("likes")]
        public int Likes { get; set; }
        [JsonProperty("passes")]
        public List<PassesListInfoElementJson> Passes { get; set; }
        [JsonProperty("curation")]
        public LevelListInfoElementCurationJson Curation { get; set; }
        [JsonProperty("tags")]
        public List<LevelListInfoElementTagJson> Tags { get; set; } = new();
        [JsonProperty("difficulty")]
        public LevelListInfoElementDifficultyJson Difficulty { get; set; } = new();
        [JsonProperty("bpm")]
        public float BPM { get; set; }
        [JsonProperty("tilecount")]
        public int TileCount { get; set; }
        [JsonProperty("levelLengthInMs")]
        public float LevelLengthInMs { get; set; }
    }
    public class LevelListInfoElementDifficultyJson
    {
        [JsonProperty("ID")]
        public int ID { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("icon")]
        public string Icon { get; set; }
        [JsonProperty("emoji")]
        public string Emoji { get; set; }
        [JsonProperty("color")]
        public string Color { get; set; }
        [JsonProperty("createdAt")]
        public string CreatedAt { get; set; }
        [JsonProperty("updatedAt")]
        public string UpdatedAt { get; set; }
        [JsonProperty("baseScore")]
        public double BaseScore { get; set; }
        [JsonProperty("sortOrder")]
        public int SortOrder { get; set; }
        [JsonProperty("legacy")]
        public string Legacy { get; set; }
        [JsonProperty("legacyIcon")]
        public string LegacyIcon { get; set; }
        [JsonProperty("legacyEmoji")]
        public string LegacyEmoji { get; set; }
    }
    public class ListListInfoElementPassJson 
    {
        [JsonProperty("id")]
        public int ID { get; set; }
    }
    public class LevelListInfoElementCurationJson
    {
        [JsonProperty("id")]
        public int ID { get; set; }
        [JsonProperty("levelId")]
        public int LevelID { get; set; }
        [JsonProperty("typeId")]
        public int? TypeID { get; set; }
        [JsonProperty("type")]
        public LevelListInfoElementCurationTypeJson Type { get; set; }
    }
    public class LevelListInfoElementCurationTypeJson
    {
        [JsonProperty("id")]
        public int ID { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("icon")]
        public string Icon { get; set; }
    }
    public class LevelListInfoElementTagJson
    {
        [JsonProperty("id")]
        public int ID { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
    public class LevelListInfoElementDiffJson
    {
        [JsonProperty("id")]
        public int ID { get; set; }
        [JsonProperty("baseScore")]
        public float BaseScore { get; set; } = 0;
    }
}
 
