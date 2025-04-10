using DirectLevel;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections;
using TMPro;
using Together.Utils;
using TUFHelper;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using TUFHelper.ModScripts.Json;

public class LevelInfoSceneScript : MonoBehaviour
{

    public static LevelListInfoElementJson currentLevelInfo;
    public static string vType, thumUrl;

    public GameObject videoLoadingText;
    public Image difficultyIcon, video, playIcon;
    public Button downloadButton, playButton;

    public TextMeshProUGUI
        songText,
        creatorText,
        downloadButtonText,
        playButtonText,
        debug;

    public void Awake()
    {
        void ClickSfx()
        {
            scrSfx.instance?.PlaySfx(SfxSound.MobileButton);
        }

        downloadButton.onClick.AddListener(ClickSfx);
        playButton.onClick.AddListener(ClickSfx);

        difficultyIcon.sprite = Helper.getDiffSprite(currentLevelInfo.DiffId);
        songText.text = currentLevelInfo.Artist + " - " + currentLevelInfo.Song;
        creatorText.text = "Level By: " + currentLevelInfo.Creator;

        if (currentLevelInfo.DlLink.Equals(""))
        {
            downloadButton.interactable = false;
            downloadButtonText.color = new Color(150 / 255f, 150 / 255f, 150 / 255f);
        }

        string vId = GetVideoId(currentLevelInfo.VideoLink);
        if (vType.Equals("YOUTUBE"))
        {
            StartCoroutine(RequestYoutubeVideo(GetVideoId(currentLevelInfo.VideoLink)));
        } else if(vType.Equals("BILIBILI")) 
        {
            StartCoroutine(RequestBilibiliVideo(GetVideoId(currentLevelInfo.VideoLink)));
        }


        if (!currentLevelInfo.DlLink.Contains("drive.google") && !currentLevelInfo.DlLink.Contains("discord") &&
            !currentLevelInfo.DlLink.Contains("hyonsu"))
        {
            playButton.interactable = false;
            playButtonText.color = new Color(150 / 255f, 150 / 255f, 150 / 255f);
        }

    }

    public IEnumerator RequestYoutubeVideo(string vId)
    {
        UnityWebRequest www = UnityWebRequest.Get("https://www.googleapis.com/youtube/v3/videos?id=" + vId + "&key=AIzaSyA6n-nD5qP51aq6g1gnPSVzLYRkLSKhh_A&part=snippet");
        www.certificateHandler = new CertificateWhore();

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Passes Request Error: " + www.error);
        }
        else
        {
            JObject jo = JsonConvert.DeserializeObject<JObject>(www.downloadHandler.text);
            JObject thumbnails = jo.Value<JArray>("items")[0].Value<JObject>("snippet").Value<JObject>("thumbnails");
            if (thumbnails.ContainsKey("maxres"))
            {
                thumUrl = thumbnails.Value<JObject>("maxres").Value<string>("url");
            } 
            else if (thumbnails.ContainsKey("standard"))
            {
                thumUrl = thumbnails.Value<JObject>("standard").Value<string>("url");
            }
            else if (thumbnails.ContainsKey("high"))
            {
                thumUrl = thumbnails.Value<JObject>("high").Value<string>("url");
            }
            else if (thumbnails.ContainsKey("medium"))
            {
                thumUrl = thumbnails.Value<JObject>("medium").Value<string>("url");
            } else
            {
                thumUrl = thumbnails.Value<JObject>("default").Value<string>("url");
            }

            StartCoroutine(LoadThumbnailCo(thumUrl));
        }
    }

    internal IEnumerator LoadThumbnailCo(string url)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            uwr.certificateHandler = new CertificateWhore();

            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                video.sprite = sprite;
                video.color = Color.white;

                videoLoadingText.SetActive(false);
                playIcon.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError("Thumbnail download failed: " + uwr.error);
            }
        }
    }

    public IEnumerator RequestBilibiliVideo(string vId)
    {
        UnityWebRequest www = UnityWebRequest.Get("https://api.bilibili.com/x/web-interface/view?bvid=" + vId);
        www.certificateHandler = new CertificateWhore();

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Passes Request Error: " + www.error);
        }
        else
        {
            JObject jo = JsonConvert.DeserializeObject<JObject>(www.downloadHandler.text);
            thumUrl = jo.Value<JObject>("data").Value<string>("pic").Replace("http://", "https://");
        }

        StartCoroutine(LoadThumbnailCo(thumUrl));
    }


    public string GetVideoId(string url)
    {
        url = GetCleanUrl(url);
        url = url.Replace("https://www.youtube.com/watch?v=","");
        url = url.Replace("https://youtu.be/", "");
        url = url.Replace("https://www.bilibili.com/video/", "");
        url = url.Replace("https://b23.tv/", "");
        return url;
    }

    public string GetCleanUrl(string url)
    {

        if (url.Contains("youtu.be") || url.Contains("youtube.com"))
        {
            int questionMarkIndex = url.IndexOf("&");

            if (questionMarkIndex >= 0)
            {
                url = url.Substring(0, questionMarkIndex);
            }

            vType = "YOUTUBE";
        }

        if (url.Contains("bilibili.com") || url.Contains("b23.tv"))
        {
            int questionMarkIndex = url.IndexOf("/?");

            if (questionMarkIndex >= 0)
            {
                url = url.Substring(0, questionMarkIndex);
            }

            vType = "BILIBILI";
        }

        return url;
    }

    public void DownloadButtonClick()
    {
        if (DownloadPopupScript.IsDownloading) return;

        Application.OpenURL(currentLevelInfo.DlLink);
    }

    private void ExceptionCatch(Exception ex)
    {

        Debug.LogException(ex);
        ErrorScript.ShowError(ex.Message);

        DownloadPopupScript.IsDownloading = false;
    }

    public void PlayButtonClick()
    {

        if (DownloadPopupScript.IsDownloading) return;

        ErrorScript.instance.gameObject.SetActive(false);

        try
        {
            DownloadPopupScript.IsDownloading = true;

            //Persistence.SetHideCursorWhilePlaying(false);

            DownloadPopupScript.Show();

            var levelDownloder = new LevelDownloader(currentLevelInfo.DlLink);

            levelDownloder.ErrorHandler = (ex) =>
            {
                DirectLevel.Utils.RunAtMainThread(() => ExceptionCatch(ex));
            };

            levelDownloder.OnUpdateProgress = (progress, stateMessage) =>
            {
                DownloadPopupScript.ChangeProgress = progress;
                DownloadPopupScript.ChangeMessage = stateMessage;
            };

            levelDownloder.OnDownloadComplete = levelList =>
            {
                switch (levelList.Count)
                {
                    case 0:
                        throw new Exception("adofai file was not found");
                    case 1:
                        UIScript.SwipeToBlack(() => TryToLoadLevel(levelList[0]));
                        break;
                    default:
                        DirectLevel.Utils.RunAtMainThread(() =>
                        {
                            DownloadPopupScript.Close();
                            ResultLevelScript.ShowList(levelList);
                        });
                        break;
                }
            };

            levelDownloder.OnCalculationCompleteFileSize = size =>
            {
                if (size > 300000000)
                {
                    DirectLevel.Utils.RunAtMainThread(() =>
                    {
                        DownloadPopupScript.ShowFileWarning(size,
                            () => { levelDownloder.DownloadWithTask(Main.Setting.levelSaveFolder, false); },
                            () => { DownloadPopupScript.IsDownloading = false; });
                    });
                    return true;
                }

                return false;


            };

            levelDownloder.DownloadWithTask(Main.Setting.levelSaveFolder, true);

        }
        catch (Exception ex)
        {
            ExceptionCatch(ex);
        }
    }


    public static void TryToLoadLevel(string levelFilePath)
    {
        DownloadPopupScript.IsDownloading = false;
        HideUIFixPatch.RecentDirectLevelOpend = true;

        GCS.sceneToLoad = "scnEditor";
        GCS.worldEntrance = null;
        scnEditor.levelToOpenOnLoad = levelFilePath;

        SceneManager.LoadScene("scnEditor");

        /*
        DirectLevel.Utils.RunAtMainThread(() =>
        {
            SceneManager.LoadScene("scnEditor");
        });*/
    }

}