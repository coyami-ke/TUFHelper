using System;
using System.Threading;
using System.Threading.Tasks;
using Together.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace TUFHelper.ModScripts.Web
{
    public class TUFAPIRequest_Packs
    {
        public const string DEFAULT_URL = "https://api.tuforums.com/v2/database/levels/packs";

        public int Offset { get; set; } = 0;
        public int Limit { get; } = 30;
        public string Query { get; set; } = "";
        public string SortBy = "RECENT";
        public AscendingOrDescending SortAsc = AscendingOrDescending.Descending;

        public string Answer { get; private set; }

        public TUFAPIRequest_Packs(int limit)
        {
            Limit = limit;
        }

        public async Task GetAnswerAsync(CancellationToken token)
        {
            string order = SortAsc == AscendingOrDescending.Ascending ? "ASC" : "DESC";
            string sort = $"{SortBy}_{order}";

            string encodedQuery = Uri.EscapeDataString(global::SearchScript.NormalizeSearchText(Query));
            string url = $"{DEFAULT_URL}?limit={Limit}&offset={Offset}&query={encodedQuery}&sort={sort}";

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
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TUFAPIRequest] Request failed: {ex.Message}");
                throw;
            }
        }

    }
}
