using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Together.Utils;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace TUFHelper.ModScripts.Web
{
    public class TUFRatingRequest
    {
        public const string DEFAULT_URL = "https://api.tuforums.com/v2/admin/rating";

        public string Answer { get; private set; }

        public async Task GetAnswerAsync(CancellationToken token)
        {
            using var request = UnityWebRequest.Get(DEFAULT_URL);
            var account = AccountSaver.GetAccount();
            request.SetRequestHeader("Authorization", "Bearer " + account.Token);

            request.certificateHandler = new CertificateWhore();
            request.disposeCertificateHandlerOnDispose = true;

            try
            {
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    if (token.IsCancellationRequested)
                    {
                        request.Abort();
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
