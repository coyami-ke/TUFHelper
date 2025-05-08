using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace TUFHelper.ModScripts.Json
{
    public class LevelListInfoJson
    {
        [JsonProperty("hasMore")]
        public bool HasMore { get; set; } 
        [JsonProperty("results")]
        public List<LevelListInfoElementJson> Results { get; set; }

        #nullable enable
        public static LevelListInfoJson? Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<LevelListInfoJson>(json);
        }
    }
}
 

