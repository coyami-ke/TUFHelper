using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public class TUFAPIRequest_Levels
    {
        public const string DEFAULT_URL = "https://api.tuforums.com/v2/database/levels";

        public int Offset { get; set; } = 0;
        public int Limit { get; } = 50;
        public string Query { get; set; } = "";
        public int MinDiffPGU { get; set; } = 1;
        public int MaxDiffPGU { get; set; } = 60;
        public List<string> SpecialDifficulties { get; set; } = new();
        public List<string> QDifficulties { get; set; } = new();
        public List<string> TagsFilter { get; set; } = new();
        public string SortBy = "RECENT";
        public AscendingOrDescending SortAsc = AscendingOrDescending.Descending;

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

            string url = $"{DEFAULT_URL}?limit={Limit}&offset={Offset}&query={Query}&pguRange={minDiff},{maxDiff}&sort={sort}&deletedFilter=hide";
            if (SpecialDifficulties.Count > 0 || QDifficulties.Count > 0)
            {
                // Combine both lists
                var allDifficulties = SpecialDifficulties.Concat(QDifficulties);
                url += "&specialDifficulties=" + string.Join(",", allDifficulties);
            }
            if (TagsFilter.Count > 0)
            {
                url += "&tagsFilter=" + string.Join(",", TagsFilter);
            }

            using var request = UnityWebRequest.Get(url);
            request.certificateHandler = new CertificateWhore();
            request.disposeCertificateHandlerOnDispose = true;

            try
            {
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    if (token.IsCancellationRequested)
                    {
                        request.Abort(); // explicitly abort
                        token.ThrowIfCancellationRequested();
                    }
                    await Task.Yield();
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Answer = request.downloadHandler.text;
                }
                else
                {
                    throw new Exception($"Request error: {request.error}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TUFAPIRequest] Request failed: {ex.Message}");
                throw;
            }
        }

    }
}