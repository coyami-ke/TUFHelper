using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Together.Utils;
using TUFHelper.ModScripts.Json;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace TUFHelper.ModScripts.Web
{
    public enum AscendingOrDescending
    {
        Ascending,
        Descending
    }
    public class FacetFilter
    {
        [JsonProperty("v")]
        public int Version { get; set; } = 1;

        [JsonProperty("combine")]
        public string Combine { get; set; } = "and";

        [JsonProperty("tags")]
        public FacetGroup Tags { get; set; } = new();

        [JsonProperty("curationTypes")]
        public FacetGroup CurationTypes { get; set; } = new();
    }

    public class FacetGroup
    {
        [JsonProperty("mode")]
        public string Mode { get; set; } = "simple";

        [JsonProperty("op")]
        public string Op { get; set; } = "or";

        [JsonProperty("ids")]
        public List<int> Ids { get; set; } = new();
    }
    public class TUFAPIRequest_Levels
    {
        public const string DEFAULT_URL = "https://api.tuforums.com/v2/database/levels";

        public int Offset { get; set; } = 0;
        public int Limit { get; } = 50;
        public string Query { get; set; } = "";
        public int MinDiffPGU { get; set; } = 1;
        public int MaxDiffPGU { get; set; } = 60;
        public List<string> SpecialDifficulties { get; set; } = new();
        public List<string> CurationTypes { get; set; } = new();
        public List<string> QDifficulties { get; set; } = new();
        public List<string> TagsFilter { get; set; } = new();
        public string SortBy = "RECENT";
        public AscendingOrDescending SortAsc = AscendingOrDescending.Descending;
        public bool EnableQRange { get; set; } = true;

        public string Answer { get; private set; }

        public TUFAPIRequest_Levels(int limit)
        {
            Limit = limit;
        }

        public async Task GetAnswerAsync(CancellationToken token)
        {
            string minDiff = DiffSpriteHelper.DiffIDRegister[MinDiffPGU];
            string maxDiff = DiffSpriteHelper.DiffIDRegister[MaxDiffPGU];
            string order = SortAsc == AscendingOrDescending.Ascending ? "ASC" : "DESC";
            string sort = $"{SortBy}_{order}";

            string encodedQuery = Uri.EscapeDataString(Query);
            string url = $"{DEFAULT_URL}?limit={Limit}&offset={Offset}&query={encodedQuery}&pguRange={minDiff},{maxDiff}&sort={sort}&deletedFilter=hide";
            if (SpecialDifficulties.Count > 0 || QDifficulties.Count > 0)
            {
                var cleaned = SpecialDifficulties;
                if (EnableQRange) cleaned = cleaned.Concat(QDifficulties).ToList();

                if (cleaned.Count > 0)
                {
                    string joined = string.Join(",", cleaned);
                    url += "&specialDifficulties=" + UnityWebRequest.EscapeURL(joined);
                }
            }

            FacetFilter facetQuery = new();
            foreach (var tag in TagsFilter)
            {
                if (TagSpriteHelper.ReversedSpriteRegister.TryGetValue(tag, out var value))
                {
                    facetQuery.Tags.Ids.Add(value);
                }
                else continue;
            }

            var registry = CurationHelper.GetReversedIDRegister();
            foreach (var curation in CurationTypes)
            {
                if (registry.TryGetValue(curation, out var value))
                {
                    facetQuery.CurationTypes.Ids.Add(value);
                }
            }

            url += "&facetQuery=" + JsonConvert.SerializeObject(facetQuery);
            
            string answer = "";

            HttpResponseMessage response = await Main.Client.GetAsync(url, token);

            response.EnsureSuccessStatusCode();

            answer = await response.Content.ReadAsStringAsync();

            Answer = answer;
        }
    }
}
