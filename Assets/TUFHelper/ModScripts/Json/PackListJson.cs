using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TUFHelper.ModScripts.Json
{
    public class PackListJson
    {
        [JsonProperty("packs")]
        public PackListElementJson[] Packs { get; set; }
        [JsonProperty("total")]
        public int Total { get; set; }
        [JsonProperty("page")]
        public int Page { get; set; }
        [JsonProperty("offset")]
        public int Offset { get; set; }
        [JsonProperty("limit")]
        public int Limit { get; set; }
    }
}
