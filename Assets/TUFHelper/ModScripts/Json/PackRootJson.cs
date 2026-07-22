using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TUFHelper.ModScripts.Json
{
    public class PackRootJson
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("iconUrl")]
        public string IconUrl { get; set; }

        //[JsonProperty("packOwner")]
        //public PackOwnerJson PackOwner { get; set; }
        [JsonProperty("items")]
        public List<PackItemNode> Items { get; set; } = new();
    }
    public class PackItemNode
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("parentId")]
        public int ParentId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("sortOrder")]
        public int SortOrder { get; set; }

        [JsonProperty("isCleared")]
        public bool IsCleared { get; set; }

        [JsonProperty("levelId")]
        public int? LevelId { get; set; }

        [JsonProperty("referencedLevel")]
        public LevelListInfoElementJson ReferencedLevel { get; set; }
        [JsonProperty("children")]
        public List<PackItemNode> Children { get; set; } = new();
        [JsonIgnore]
        public bool IsFolder => Type == "folder";

        [JsonIgnore]
        public bool IsLevel => Type == "level";

        [JsonIgnore]
        public bool IsExpanded = false;
        [JsonIgnore]
        public MonoBehaviour SpawnedUIScript;
    }
}
