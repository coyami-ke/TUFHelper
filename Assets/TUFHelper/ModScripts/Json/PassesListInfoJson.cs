using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace TUFHelper.ModScripts.Json
{
    public class PassesListInfoJson
    {
        [JsonProperty("count")]
        public int Count { get; set; }
        [JsonProperty("results")]
        public List<PassesListInfoElementJson> Results { get; set; }

        #nullable enable
        public static PassesListInfoJson? Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<PassesListInfoJson>(json);
        }
    }
}
 
