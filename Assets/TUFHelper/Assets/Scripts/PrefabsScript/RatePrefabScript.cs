using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DirectLevel;
using Newtonsoft.Json;
using TMPro;
using Together.Utils;
using TUFHelper;
using TUFHelper.AccountSystem;
using TUFHelper.ModScripts.Json;
using TUFHelper.ModScripts.Web;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RatePrefabScript : MonoBehaviour
{
    public TextMeshProUGUI artistSong, yourRating, managerRating, communityRating, supposedRate, rerateMessage, levelID;

    public Image background;

    public RatingElementJson RatingInfo { get; private set; }

    public void SetRateInfo(RatingElementJson info)
    {
        artistSong.text = $"{info.Level.Artist} - {info.Level.Song}";
        if (info.CurrentDifficulty != null) managerRating.text = info.CurrentDifficulty.Name;
        else managerRating.text = "?";
        if (info.CommunityDifficulty != null) communityRating.text = info.CommunityDifficulty.Name;
        else communityRating.text = "?";
        if (info.AverageDifficulty != null) yourRating.text = info.AverageDifficulty.Name;

        levelID.text = "#" + info.LevelID.ToString();

        rerateMessage.text = info.Level.RerateReason;
        //if (info.RequestedDiffID != null) supposedRate.tag = DiffSpriteHelper.DiffIDRegister[info.RequestedDiffID.Value];

        if (info?.RequestedDiffID < 20) background.color = new(0.25f, 1, 0.25f, 50 / 255f);
        else if (info.Details.Count >= 4) background.color = new(1, 0.25f, 0.25f, 50 / 255f);

        RatingInfo = info;
    }

    private CancellationTokenSource requestCancelToken;
    public async void DownloadLevel()
    {
        if (DownloadPanel.instance.IsDownloading) return;

        Main.Logger.Log("yay");

        string url = $"https://api.tuforums.com/v2/database/levels/byId/{RatingInfo.Level.ID}";

        try
        {
            using var request = UnityWebRequest.Get(url);
            request.certificateHandler = new CertificateWhore();
            request.disposeCertificateHandlerOnDispose = true;

            var op = request.SendWebRequest();
            while (!op.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                LevelListInfoElementJson level = JsonConvert.DeserializeObject<LevelListInfoElementJson>(request.downloadHandler.text);

                LevelDownloader levelDownloder = new(level.DlLink)
                {
                    ErrorHandler = (ex) =>
                    {
                        DirectLevel.Utils.RunAtMainThread(() => ExceptionCatch(ex));
                    }
                };

                DownloadPanel.instance.DownloadLevel(levelDownloder);

                lastLevel = level;
                levelDownloder.DownloadComplete += OnCompleteDownload;
            }
        }
        catch (Exception ex)
        {
            Main.Logger.LogException(ex);
        }
    }

    private LevelListInfoElementJson lastLevel;
    private void OnCompleteDownload(object sender, DownloadCompleteEventArgs args)
    {
        switch (args.Levels.Count)
        {
            case 0:
                throw new Exception("adofai file was not found");
            case 1:
                LevelPrefabScript.SaveLevelToSettings(lastLevel, Path.GetDirectoryName(args.Levels[0]), args.Levels[0]);
                UIScript.SwipeToBlack(() => LevelPrefabScript.TryToLoadLevel(lastLevel, args.Levels[0]));
                break;
            default:
                LevelPrefabScript.SaveLevelToSettings(lastLevel, Path.GetDirectoryName(args.Levels[0]), args.Levels[0]);
                StartCoroutine(LevelSelector.instance.LoadLevelsCo(args.Levels));
                break;
        }
    }
    private void ExceptionCatch(Exception ex)
    {
        ErrorScript.ShowError(ex.Message);
        Main.Logger.Error(ex.Message + ex.StackTrace);
    }
}
