using Newtonsoft.Json;

namespace TUFHelper.Utils
{
    public class LevelInfo
    {

        public int id;
        public float diff;
        public string song, artist, creator, charter, vfxer, team, pguDiff, vidLink, dlLink, workshopLink;

        [JsonConstructor]
        public LevelInfo() {
            
        }

        public LevelInfo(int id, float diff, string song, string artist, string creator, string charter, string vfxer, string team, string pgu_diff, string vidLink, string dlLink, string workshopLink)
        {
            this.id = id;
            this.diff = diff;
            this.song = song;
            this.charter = charter;
            this.vfxer = vfxer;
            this.team = team;
            this.artist = artist;
            this.creator = creator;
            this.pguDiff = pgu_diff;
            this.vidLink = vidLink;
            this.dlLink = dlLink;
            this.workshopLink = workshopLink;
        }
    }

}