using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace TUFHelper.AccountSystem
{
    public class FullInfoAboutMyAccount
    {
        [JsonProperty("user")]
        public InfoAboutMyAccount User { get; set; }
    }
    public class InfoAboutMyAccount
    {
        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; } = "unknown";

        [JsonProperty("nickname")]
        public string Nickname { get; set; } = "unknown";

        [JsonProperty("avatarUrl")]
        public string AvatarUrl { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("isRater")]
        public bool IsRater { get; set; }

        [JsonProperty("isSuperAdmin")]
        public bool IsSuperAdmin { get; set; }

        [JsonProperty("isRatingBanned")]
        public bool IsRatingBanned { get; set; }

        [JsonProperty("isEmailVerified")]
        public bool IsEmailVerified { get; set; }

        [JsonProperty("playerId")]
        public int PlayerID { get; set; }

        [JsonProperty("password")]
        public bool Password { get; set; }
    }

    public class TUFTokenRequest
    {
        public const string DEFAULT_LOGIN_URL = "https://api.tuforums.com/v2/auth/login";
        public const string DEFAULT_ME_URL = "https://api.tuforums.com/v2/auth/profile/me";
        public const string DEFAULT_DISCORD_LOGIN_URL = "https://api.tuforums.com/v2/auth/oauth/discord";
        public const string DEFAULT_SEND_RATING_URL = "https://api.tuforums.com/v2/admin/rating";

        public int LastResponseCode { get; private set; } = 0;

        public string Token { get; set; }

        public async Task TrySendRating(int levelID, string comment, bool isCommunityRating, string rating)
        {
            string url = $"https://api.tuforums.com/v2/admin/rating/{levelID}";

            // Construct the JSON payload
            var data = new
            {
                comment,
                isCommunityRating,
                rating
            };

            string jsonData = JsonConvert.SerializeObject(data);

            using UnityWebRequest www = UnityWebRequest.Put(url, jsonData);
            www.method = UnityWebRequest.kHttpVerbPUT;
            www.SetRequestHeader("Content-Type", "application/json");

            if (!string.IsNullOrEmpty(Token))
            {
                www.SetRequestHeader("Authorization", $"Bearer {Token}");
            }

            var operation = www.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            LastResponseCode = (int)www.responseCode;

            // Main.Logger.Log($"Level ID: {levelID} comment: {comment} rating: {rating} isCommunityRating: {isCommunityRating}");
            // Main.Logger.Log($"Body: {jsonData}");

            if (www.result != UnityWebRequest.Result.Success)
            {
                Main.Logger.Error($"Failed to send rating: {www.error}");
            }
            else
            {
                Main.Logger.Log("Rating successfully sent.");
                // Main.Logger.Log(www.downloadHandler.text);
            }
        }

        public async Task<byte[]> GetPfpFromURL(string url)
        {
            using UnityWebRequest www = UnityWebRequest.Get(url);
            var operation = www.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Main.Logger.Error($"Failed to download profile picture: {www.error}");
                return null;
            }

            return www.downloadHandler.data;
        }
        public async Task<FullInfoAboutMyAccount> GetInfoAboutMe()
        {
            using UnityWebRequest request = new(DEFAULT_ME_URL, UnityWebRequest.kHttpVerbGET);
            request.SetRequestHeader("Authorization", "Bearer " + Token);
            request.downloadHandler = new DownloadHandlerBuffer();

            try
            {
                await SendWebRequestAsync(request);

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var responseString = request.downloadHandler.text;
                    var jsonResponse = JsonConvert.DeserializeObject<FullInfoAboutMyAccount>(responseString);

                    LastResponseCode = (int)request.responseCode;

                    return jsonResponse;
                }
                else
                {
                    Main.Logger.Error($"The mod could not get information about your accoount.");

                    LastResponseCode = (int)request.responseCode;

                    return null;
                }
            }
            catch (Exception ex)
            {
                Main.Logger.Error(ex.Message);
                return null;
            }
        }

        public async Task TryGetToken(string email, string password)
        {
            var payload = new
            {
                emailOrUsername = email,
                password,
                remember = true
            };

            string json = JsonConvert.SerializeObject(payload);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

            using UnityWebRequest request = new(DEFAULT_LOGIN_URL, UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(jsonBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            try
            {
                await SendWebRequestAsync(request);

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var responseString = request.downloadHandler.text;
                    var jsonResponse = JObject.Parse(responseString);

                    if (jsonResponse["token"] != null)
                    {
                        Token = jsonResponse["token"].ToString();
                        Main.Logger.Log("Token retrieved successfully.");
                    }
                    else
                    {
                        Main.Logger.Error("Token not found in response.");
                    }
                }
                else if (request.responseCode == 401)
                {
                    Main.Logger.Error("Invalid credentials.");
                }
                else
                {
                    Main.Logger.Error($"Login failed: {(HttpStatusCode)request.responseCode}\nResponse: {request.downloadHandler.text}");
                }
            }
            catch (Exception ex)
            {
                Main.Logger.Error($"Exception during token retrieval: {ex.Message}");
            }

            LastResponseCode = (int)request.responseCode;
        }

        private static Task SendWebRequestAsync(UnityWebRequest request)
        {
            var tcs = new TaskCompletionSource<object>();

            var operation = request.SendWebRequest();
            operation.completed += _ =>
            {
                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    tcs.SetException(new Exception(request.error));
                }
                else
                {
                    tcs.SetResult(null);
                }
            };

            return tcs.Task;
        }
    }
}
