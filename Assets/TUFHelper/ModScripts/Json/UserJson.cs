using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TUFHelper.ModScripts.Json
{
    public class UserJson
    {
        [JsonProperty("id")]
        public string ID { get; set; }
        [JsonProperty("nickname")]
        public string Nickname { get; set; }
        [JsonProperty("username")]
        public string Username { get; set; }
        [JsonProperty("avatarUrl")]
        public string AvatarURL { get; set; }
    }
}
