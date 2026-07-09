using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TUFHelper.ModScripts.Json
{
    public class PackListElementJson
    {
        [JsonProperty("id")]
        public string ID { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("iconUrl")]
        public string? IconURL { get; set; }
        [JsonProperty("isPinned")]
        public bool IsPinned { get; set; }
        [JsonProperty("favoritesCount")]
        public int FavoritesCount { get; set; }
        [JsonProperty("levelCount")]
        public int LevelCount { get; set; }
        [JsonProperty("totalLevelCount")]
        public int TotalLevelCount { get; set; }
        [JsonProperty("packOwner")]
        public UserJson PackOwner { get; set; }
        [JsonProperty("packItems")]
        public PackReferenceLevelJson[] PackItems { get; set; }
    }
    public class ReferenceLevelJson
    {
        [JsonProperty("id")]
        public int ID { get; set; }
        [JsonProperty("artist")]
        public string Artist { get; set; }
        [JsonProperty("song")]
        public string Song { get; set; }
        [JsonProperty("diffId")]
        public int DiffID { get; set; }
    }
    public class PackReferenceLevelJson
    {
        [JsonProperty("levelId")]
        public int LevelID { get; set; }
        [JsonProperty("referencedLevel")]
        public ReferenceLevelJson ReferencedLevel { get; set; }
    }
}
