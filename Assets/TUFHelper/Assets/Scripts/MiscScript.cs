using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DirectLevel;
using Newtonsoft.Json;
using TMPro;
using Together.Utils;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using TUFHelper.ModScripts.Web;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityModManagerNet;

public class MiscScript : MonoBehaviour
{

    public static MiscScript instance;

    public GameObject errorObject;
    public GameObject frontMenuCanvas;
    public GameObject playCanvas;
    private CancellationTokenSource requestCancelToken;

    public void Awake()
    {
        instance = this;

        errorObject.SetActive(true);

        var keyviewer = UnityModManager.FindMod("KeyViewer");
        if (keyviewer != null && !Main.Setting.IsShowedKeyviewerError)
        {
            var keyviewerAssembly = keyviewer.Assembly;

            if (keyviewerAssembly == null) return;

            ErrorScript.instance.errorContentText.text = "TUFHelper found the mod KeyViewer! The current version of TUFHelper doesn't have support of KeyViewer and it might cause bugs.";
            ErrorScript.instance.gameObject.SetActive(true);

            Main.Setting.IsShowedKeyviewerError = true;
            Main.Setting.Save(Main.ModEntry);
        }

        var fmod = UnityModManager.FindMod("FMod");
        if (fmod != null && !Main.Setting.IsShowedFmodError)
        {
            var fmodAssembly = fmod.Assembly;

            if (fmodAssembly == null) return;

            ErrorScript.instance.errorContentText.text = "TUFHelper found the mod FMod! The current version of TUFHelper doesn't have support of FMod and it might cause performance issues.";
            ErrorScript.instance.gameObject.SetActive(true);

            Main.Setting.IsShowedFmodError = true;
            Main.Setting.Save(Main.ModEntry);
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitButtonClick();
        }
    }

    public void ExitButtonClick()
    {
        //if (DownloadPopupScript.IsDownloading) return;

        //if (WindowsManager.instance != null && !WindowsManager.instance.FolderListActive && LevelListScript.instance.GroupByFolder)
        //{
        //    WindowsManager.instance.MoveToFolderList();
        //    return;
        //}

        //UIScript.SwipeToBlack(() =>
        //{
        //    Main.isInTUFHelper = false;
        //    ADOFAIGameplayHandler.IsFromTUFHelper = false;
        //    ADOFAIGameplayHandler.EditorPlayPatch.CurrentLevelInfo = null;
        //    GCS.sceneToLoad = "";
        //    SceneManager.LoadScene("scnLevelSelect");
        //});

        if (FrontPageScript.instance.frontPageObject.activeSelf)
        {
            UIScript.SwipeToBlack(() =>
            {
                Main.isInTUFHelper = false;
                ADOFAIGameplayHandler.IsFromTUFHelper = false;
                FrontPageScript.isFirstRun = true;
                ADOFAIGameplayHandler.EditorPlayPatch.CurrentLevelInfo = null;
                GCS.sceneToLoad = "";
                SceneManager.LoadScene("scnLevelSelect");
            });
            return;
        }

        var frontPageButtons = frontMenuCanvas.GetComponentsInChildren<FrontPageButton>(includeInactive: true);
        foreach (var button in frontPageButtons)
        {
            if (button.showableCanvas != null) button.showableCanvas.SetActive(false);
        }

        FrontPageScript.instance.frontPageObject.SetActive(true);

        CustomMusicPlayer.instance.StopPlay();
    }

    public void OpenURL(string url)
    {
        // if (new System.Random().Next(1, 100) == 1)
        // {
        //     Application.OpenURL("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
        // }
        // else
        // {

        // }
        Application.OpenURL(url);
    }
    public void ShowInfoAboutHotkeys()
    {
        
    }
    
    public async void UpdateOfflineLevels(TextMeshProUGUI textInfo)
    {
        requestCancelToken?.Cancel();
        requestCancelToken = new CancellationTokenSource();

        CancellationToken token = requestCancelToken.Token;

        int count = Main.DownloadedLevels.Levels.Count;
        int i = 0;
        foreach (var level in Main.DownloadedLevels.Levels.ToArray())
        {
            if (level == null) continue;
            try
            {
                string url = $"https://api.tuforums.com/v2/database/levels/byId/{level.ID}";

                using var request = UnityWebRequest.Get(url);
                request.certificateHandler = new CertificateWhore();
                request.disposeCertificateHandlerOnDispose = true;

                var op = request.SendWebRequest();
                while (!op.isDone)
                {
                    await Task.Yield();
                    token.ThrowIfCancellationRequested();
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    return;
                }

                var json = request.downloadHandler.text;
                var newLevel = JsonConvert.DeserializeObject<LevelListInfoElementJson>(json);

                textInfo.text = $"{LanguageManager.Translate("UPDATE INFO")} ({i + 1}/{count})...";

                i++;
            }
            catch
            {
            }
        }

        Main.Setting.Save(Main.ModEntry);
        LevelListScript.instance.ClearLevels();
        await LevelListScript.instance.UpdateLevelListAsync();
        LanguageManager.RememberOriginal(textInfo, "UPDATE INFO");
    }
    public async void ImFuckingLucky()
    {
        if (FrontPageScript.instance.frontPageObject.activeSelf) FrontPageScript.instance.frontPageObject.SetActive(false);

        playCanvas.SetActive(true);


        requestCancelToken?.Cancel();
        requestCancelToken = new CancellationTokenSource();

        CancellationToken token = requestCancelToken.Token;

        LevelListInfoElementJson selectedLevel = null;
        if (Main.Setting.ShowOnlyDownloaded)
        {
            UnityEngine.Random.Range(0, Main.DownloadedLevels.Levels.Count - 1);

            var filteredLevels = Main.DownloadedLevels.Levels.Where(level =>
            {
                // Filter by difficulty
                if (DiffSpriteHelper.IsSpecialDiff(level.DiffId) || DiffSpriteHelper.IsQuantumDiff(level.DiffId))
                {
                    if (!LevelListScript.DefaultRequest.SpecialDifficulties.Contains(DiffSpriteHelper.DiffIDRegister[level.DiffId]) ||
                        LevelListScript.DefaultRequest.QDifficulties.Contains(DiffSpriteHelper.DiffIDRegister[level.DiffId]))
                        return false;
                }

                //if (DiffSpriteHelper.IsSpecialDiff(level.DiffId) || DiffSpriteHelper.IsQuantumDiff(level.DiffId))
                //{
                //    if (!DefaultRequest.SpecialDifficulties.Contains(DiffSpriteHelper.DiffIDRegister[level.DiffId]) ||
                //            !DefaultRequest.QDifficulties.Contains(DiffSpriteHelper.DiffIDRegister[level.DiffId]))
                //        return false;
                //}

                else
                {
                    if (level.DiffId < LevelListScript.DefaultRequest.MinDiffPGU || level.DiffId > LevelListScript.DefaultRequest.MaxDiffPGU)
                        return false;
                }

                return true;
            }).ToList();

            selectedLevel = filteredLevels[0];
        }
        else
        {
            TUFAPIRequest_Levels request = new(1);
            request.MinDiffPGU = LevelListScript.DefaultRequest.MinDiffPGU;
            request.MaxDiffPGU = LevelListScript.DefaultRequest.MaxDiffPGU;
            request.Query = "";
            request.Offset = 0;
            request.SortBy = "RANDOM";
            request.SpecialDifficulties = new(LevelListScript.DefaultRequest.SpecialDifficulties);
            request.QDifficulties = new(LevelListScript.DefaultRequest.QDifficulties);

            await request.GetAnswerAsync(token);

            var json = JsonConvert.DeserializeObject<LevelListInfoJson>(request.Answer);

            if (json.Results.Count > 0)
            {
                var level = json.Results[0];

                if (DownloadPanel.instance.IsDownloading) return;

                selectedLevel = level;
            }
        }

        ErrorScript.instance.gameObject.SetActive(false);

        try
        {
            // Inside ImFuckingLucky
            if (selectedLevel == null || string.IsNullOrEmpty(selectedLevel.DlLink) || selectedLevel.DlLink.Length < 10)
            {
                ExceptionCatch(new Exception("This level has an invalid or missing download link ('" + selectedLevel.DlLink + "')."));
                return;
            }

            // Only proceed if it looks like a real URL
            if (!selectedLevel.DlLink.StartsWith("http"))
            {
                ExceptionCatch(new Exception("Malformed URL: " + selectedLevel.DlLink));
                return;
            }

            if (selectedLevel.DlLink == null) Main.Logger.Error("THERES NULL");
            LevelDownloader levelDownloder = new(selectedLevel) 
            {
                ErrorHandler = (ex) =>
                {
                    DirectLevel.Utils.RunAtMainThread(() => ExceptionCatch(ex));
                }
            };

            DownloadPanel.instance.DownloadLevel(levelDownloder);

            lastLevel = selectedLevel;
            levelDownloder.DownloadComplete += OnCompleteDownload;

        }
        catch (Exception ex)
        {
            ExceptionCatch(ex);
        }
    }
    private void ExceptionCatch(Exception ex)
    {
        ErrorScript.ShowError(ex.Message);
        Main.Logger.Error(ex.Message + ex.StackTrace);
    }

    private LevelListInfoElementJson lastLevel;
    private void OnCompleteDownload(object sender, DownloadCompleteEventArgs args)
    {
        switch (args.Levels.Count)
        {
            case 0:
                throw new Exception("adofai file was not found");
            case 1:
                UIScript.SwipeToBlack(() => LevelPrefabScript.TryToLoadLevel(lastLevel, args.Levels[0]));
                break;
            default:
                LevelSelector.instance.LevelInfo = lastLevel;
                StartCoroutine(LevelSelector.instance.LoadLevelsCo(args.Levels));
                break;
        }
    }
}
