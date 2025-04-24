using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace TUFHelper.ModScripts.Json
{
    public enum TUFLevelsPostRequest_ClearedFilter
    {
        hide,
        show,
        only
    }
    public class TUFLevelsPostRequest
    {
        [JsonProperty("pguRange")]
        public PGURangeJson PGURange { get; set; } = new() { From = "P1", To = "U20" };
        [JsonProperty("specialDifficulties")]
        public string[] SpecialDifficulties { get; set; }
        // query = name,
        //         limit = 30,
        //         offset = 0,
        //         sort = "RECENT_DESC",
        //         deletedFilter = "hide",
        //         clearedFilter = "show",
        //         pguRange = new { from = "G11", to = "G11" },
        //         specialDifficulties = new string[] { }
    }
    public class PGURangeJson
    {
        [JsonProperty("from")]
        public string From { get; set; }
        [JsonProperty("to")]
        public string To { get; set; }
    }
}
 
