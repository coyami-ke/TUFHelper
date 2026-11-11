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
        public FacetGroup Tags { get; set; }

        [JsonProperty("curationTypes")]
        public FacetGroup CurationTypes { get; set; }
    }

    public class FacetGroup
    {
        [JsonProperty("mode")]
        public string Mode { get; set; } = "simple";

        [JsonProperty("op")]
        public string Op { get; set; } = "or";

        [JsonProperty("ids")]
        public List<int> Ids { get; set; }
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

            string encodedQuery = Uri.EscapeDataString(global::SearchScript.NormalizeSearchText(Query));
            string url = $"{DEFAULT_URL}?limit={Limit}&offset={Offset}&query={encodedQuery}&pguRange={minDiff},{maxDiff}&sort={sort}&deletedFilter=hide";
            if (SpecialDifficulties.Count > 0 || QDifficulties.Count > 0)
            {
                //var cleaned = SpecialDifficulties
                //    .Concat(QDifficulties)
                //    .Where(x => !string.IsNullOrWhiteSpace(x))
                //    .ToList();

                var cleaned = SpecialDifficulties;
                if (EnableQRange) cleaned = cleaned.Concat(QDifficulties).ToList();

                if (cleaned.Count > 0)
                {
                    string joined = string.Join(",", cleaned);
                    url += "&specialDifficulties=" + UnityWebRequest.EscapeURL(joined);
                }
            }
            var cleanedTags = TagsFilter
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct()
                .ToList();

            if (cleanedTags.Count > 0)
            {
                string joined = string.Join(",", cleanedTags);
                string encoded = Uri.EscapeDataString(joined); // fully percent-encodes spaces as %20
                url += "&tagsFilter=" + encoded;
            }

            string answer = "";
            HttpResponseMessage response = await Main.Client.GetAsync(url, token);

            response.EnsureSuccessStatusCode();

            //throw new HttpRequestException("test");

            answer = await response.Content.ReadAsStringAsync();

            Answer = answer;
        }
    }
}
