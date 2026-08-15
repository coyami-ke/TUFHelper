using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TUFHelper.ModScripts.Json
{
    public class CDNLevelJson
    {
        [JsonProperty("metadata")]
        public CDNLevelMetadataJson Metadata { get; set; }
    }
    public class CDNLevelMetadataJson
    {
        [JsonProperty("songFiles")]
        public Dictionary<string, CDNSongFileJson> SongFiles { get; set;  }
    }
    public class CDNSongFileJson
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("path")]
        public string Path { get; set; }
        [JsonProperty("size")]
        public int Size { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
