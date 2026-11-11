using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace TUFHelper.Utils
{
    public class TUFTag
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("icon")]
        public string Icon { get; set; }
        [JsonProperty("color")]
        public string Color { get; set; }
        [JsonProperty("group")]
        public string Group { get; set; }
    }
    public static class TagSpriteHelper
    {
        public const string PATH_TO_JSON = "Assets/TUFHelper/Assets/Data/tags.json";
        public const string PATH_TO_TAG_SPRITES = "Assets/TUFHelper/Assets/Sprites/TagIcons/";

        private static Dictionary<int, string> _spriteRegister;
        public static Dictionary<int, string> SpriteRegister
        {
            get
            {
                if (_spriteRegister == null)
                {
                    Dictionary<int, string> dictionary = new();
                    var tags = JsonConvert.DeserializeObject<List<TUFTag>>(File.ReadAllText(PATH_TO_JSON));
                    foreach (var tag in tags)
                    {
                        dictionary.Add(tag.Id, tag.Name);
                    }

                    _spriteRegister = dictionary;
                    return dictionary;
                }
                else
                {
                    return _spriteRegister;
                }
            }
        }

        private static Dictionary<string, int> _reversedSpriteRegister;
        public static Dictionary<string, int> ReversedSpriteRegister
        {
            get
            {
                if (_spriteRegister == null)
                {
                    Dictionary<string, int> dictionary = new();
                    var tags = JsonConvert.DeserializeObject<List<TUFTag>>(File.ReadAllText(PATH_TO_JSON));
                    foreach (var tag in tags)
                    {
                        dictionary.Add(tag.Name, tag.Id);
                    }

                    _reversedSpriteRegister = dictionary;
                    return dictionary;
                }
                else
                {
                    return _reversedSpriteRegister;
                }
            }
        }

        public static Sprite GetSpriteFromTag(string fileName)
        {
            if (Main.assets == null)
            {
                Main.Logger.Log("❌ AssetBundle not loaded.");
                return null;
            }

            var sprite = Main.GetSpriteFromAssets(fileName);
            if (sprite != null)
                return sprite;

            Main.Logger.Log($"❌ Tag icon not found in AssetBundle: {fileName}");
            return null;
        }

        public static async Task<TUFTag[]> GetTagsFromAPI()
        {
            var response = await Main.Client.GetAsync("https://api.tuforums.com/v2/database/difficulties/tags");
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TUFTag[]>(json);
        }

        public static async Task UpdateTags()
        {
            Main.Logger.Log("tags");
            var tags = await TagSpriteHelper.GetTagsFromAPI();
            List<string> gotTags = new();
            string dir = Path.Combine(Main.ModEntry.Path, "tags");
            foreach (var tag in tags)
            {
                if (string.IsNullOrEmpty(tag.Icon)) continue;

                string name = tag.Name.Replace('/', ' ');
                var bytes = await Main.Client.GetByteArrayAsync(tag.Icon);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                await File.WriteAllBytesAsync(Path.Combine(dir, name + ".png"), bytes);

                Main.Logger.Log(tag.Name);

                gotTags.Add(name);
            }

            await File.WriteAllTextAsync(Path.Combine(dir, "tags.json"), JsonConvert.SerializeObject(gotTags));
        }
    }
}
