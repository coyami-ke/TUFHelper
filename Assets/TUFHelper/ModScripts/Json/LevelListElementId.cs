using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TUFHelper.ModScripts.Json
{
    public class LevelListElementId
    {
        [JsonProperty("level")]
        public LevelListInfoElementJson Level { get; set; }
    }
}
