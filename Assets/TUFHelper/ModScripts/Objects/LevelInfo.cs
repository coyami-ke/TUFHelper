using Newtonsoft.Json;

namespace TUFHelper.Utils
{
    public class LevelInfo
    {

        public int id, clears, newDiff;
        public float legacyDiff, realDiff;
        public string song, artist, creator, charter, vfxer, team, vidLink, dlLink, workshopLink;

        [JsonConstructor]
        public LevelInfo() {
            clears = 0;
        }

    }

}