using Newtonsoft.Json;

namespace TUFHelper.Utils
{
    public class PlayerInfo
    {

        public string name, country;
        public bool isBanned;

        [JsonConstructor]
        public PlayerInfo() {
            
        }

    }

}