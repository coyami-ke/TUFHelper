using System;
using System.Net.Http;
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
        public string SortBy { get; set; } = "RECENT";
        public AscendingOrDescending SortAsc { get; set; } = AscendingOrDescending.Descending;

        public string Answer { get; private set; }

        public TUFAPIRequest_Packs(int limit)
        {
            Limit = limit;
        }

        public async Task GetAnswerAsync(CancellationToken token)
        {
            string order = SortAsc == AscendingOrDescending.Ascending ? "ASC" : "DESC";
            string encodedQuery = Uri.EscapeDataString(global::SearchScript.NormalizeSearchText(Query));

            string url = $"{DEFAULT_URL}?offset={Offset}&limit={Limit}&sort={SortBy}&order={order}&query={encodedQuery}&viewMode=1";

            using var request = UnityWebRequest.Get(url);
            request.certificateHandler = new CertificateWhore();
            request.disposeCertificateHandlerOnDispose = true;

            string answer = "";
            try
            {
                HttpResponseMessage response = await Main.Client.GetAsync(url, token);

                response.EnsureSuccessStatusCode();

                answer = await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                Main.Logger.Error($"[TUFAPIRequest] Network HTTP failure at {url}: {ex.Message}");
                throw;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Main.Logger.Error($"[TUFAPIRequest] Unexpected error: {ex.Message}");
                throw;
            }

            Answer = answer;
        }
    }
}