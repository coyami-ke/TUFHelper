using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TUFHelper.ModScripts.Json
{
    public class LevelListInfoJson
    {
        [JsonProperty("hasMore")]
        public bool HasMore { get; set; }
        [JsonProperty("results")]
        public List<LevelListInfoElementJson> Results { get; set; }
    }
}
