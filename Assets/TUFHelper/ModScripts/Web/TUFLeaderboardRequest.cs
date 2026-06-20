using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Together.Utils;
using TUFHelper.Utils;
using UnityEngine.Networking;

namespace TUFHelper.ModScripts.Web
{
    public class TUFLeaderboardRequest
    {
        public string Answer { get; private set; }
        public string Query { get; set; } = "";
        public int Offset { get; set; } = 0;
        public int Limit { get; set; } = 30;

        public string DEFAULT_URL = "https://api.tuforums.com/v2/database/leaderboard";

        public async Task GetAnswerAsync(CancellationToken token)
        {
            string encodedQuery = Uri.EscapeDataString(global::SearchScript.NormalizeSearchText(Query));
            string url = $"{DEFAULT_URL}?query={encodedQuery}&sortBy=rankedScore&order=desc&offset={Offset}&limit={Limit}&showBanned=hide";

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
                Main.Logger.Error($"[TUFAPIRequest] Request failed: {ex.Message}");
                throw;
            }
        }
    }
}
