using Newtonsoft.Json;

namespace TUFHelper.Utils
{
    public class PassInfo
    {

        public int id, levelId;
        public string player, speed, vidLink;
        public int[] judgements;

        [JsonConstructor]
        public PassInfo() {
            
        }

    }

}