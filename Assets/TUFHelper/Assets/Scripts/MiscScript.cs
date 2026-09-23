using System;
using System.Collections.Generic;
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

public class MiscScript : MonoBehaviour
{
    public static MiscScript instance;

    public GameObject errorObject;
    public GameObject frontMenuCanvas;
    public GameObject playCanvas;

    private CancellationTokenSource _requestCancelToken;
    private LevelListInfoElementJson _lastLevel;

    private void Awake()
    {
        instance = this;

        if (errorObject != null)
            errorObject.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitButtonClick();
        }
    }

    private void OnDestroy()
    {
        _requestCancelToken?.Cancel();
        _requestCancelToken?.Dispose();
    }

    public void ExitButtonClick()
    {
        if (FrontPageScript.instance != null && FrontPageScript.instance.frontPageObject.activeSelf)
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

        if (frontMenuCanvas != null)
        {
            var frontPageButtons = frontMenuCanvas.GetComponentsInChildren<FrontPageButton>(includeInactive: true);
            foreach (var button in frontPageButtons)
            {
                if (button.showableCanvas != null && button.showableCanvas.activeSelf)
                    UITransition.Hide(button.showableCanvas, 0.16f);
            }
        }

        if (FrontPageScript.instance != null)
        {
            UITransition.Show(FrontPageScript.instance.frontPageObject, 0.24f, 0.05f);
            UITransition.AnimateLobbyIn(FrontPageScript.instance.frontPageObject);
        }

        if (CustomMusicPlayer.instance != null)
            CustomMusicPlayer.instance.StopPlay();
    }

    public void OpenURL(string url)
    {
        if (string.IsNullOrEmpty(url)) return;
        Application.OpenURL(url);
    }

    public void ShowInfoAboutHotkeys()
    {
        // Reserved for hotkey info UI implementation
    }

    public async void UpdateOfflineLevels(TextMeshProUGUI textInfo)
    {
        CancellationToken token = CancelAndCreateNewToken();

        var levelsArray = Main.DownloadedLevels?.Levels?.ToArray();
        if (levelsArray == null || levelsArray.Length == 0) return;

        int count = levelsArray.Length;
        int processedCount = 0;

        foreach (var level in levelsArray)
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
                    continue;

                string json = request.downloadHandler.text;
                var newLevel = JsonConvert.DeserializeObject<LevelListInfoElementJson>(json);

                processedCount++;
                if (textInfo != null)
                {
                    textInfo.text = $"{LanguageManager.Translate("UPDATE INFO")} ({processedCount}/{count})...";
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                Main.Logger.Error($"Error updating offline level ID {level.ID}: {ex.Message}");
            }
        }

        Main.Setting.Save(Main.ModEntry);

        if (LevelListScript.instance != null)
        {
            LevelListScript.instance.ClearLevels();
            await LevelListScript.instance.UpdateLevelListAsync();
        }

        if (textInfo != null)
        {
            LanguageManager.RememberOriginal(textInfo, "UPDATE INFO");
        }
    }

    public async void ImFuckingLucky()
    {
        if (FrontPageScript.instance != null && FrontPageScript.instance.frontPageObject.activeSelf)
            UITransition.Hide(FrontPageScript.instance.frontPageObject, 0.16f);

        if (playCanvas != null)
            UITransition.Show(playCanvas, 0.24f, 0.04f);

        CancellationToken token = CancelAndCreateNewToken();
        LevelListInfoElementJson selectedLevel = null;

        try
        {
            if (Main.Setting.ShowOnlyDownloaded)
            {
                var downloaded = Main.DownloadedLevels?.Levels;
                if (downloaded == null || downloaded.Count == 0)
                {
                    ExceptionCatch(new Exception("No downloaded levels available."));
                    return;
                }

                var filteredLevels = downloaded.Where(level =>
                {
                    if (level == null) return false;

                    if (DiffSpriteHelper.IsSpecialDiff(level.DiffId) || DiffSpriteHelper.IsQuantumDiff(level.DiffId))
                    {
                        if (!LevelListScript.DefaultRequest.SpecialDifficulties.Contains(DiffSpriteHelper.DiffIDRegister[level.DiffId]) ||
                            LevelListScript.DefaultRequest.QDifficulties.Contains(DiffSpriteHelper.DiffIDRegister[level.DiffId]))
                            return false;
                    }
                    else
                    {
                        if (level.DiffId < LevelListScript.DefaultRequest.MinDiffPGU || level.DiffId > LevelListScript.DefaultRequest.MaxDiffPGU)
                            return false;
                    }

                    return true;
                }).ToList();

                if (filteredLevels.Count == 0)
                {
                    ExceptionCatch(new Exception("No downloaded levels match current difficulty filters."));
                    return;
                }

                selectedLevel = filteredLevels[UnityEngine.Random.Range(0, filteredLevels.Count)];
            }
            else
            {
                TUFAPIRequest_Levels request = new(1)
                {
                    MinDiffPGU = LevelListScript.DefaultRequest.MinDiffPGU,
                    MaxDiffPGU = LevelListScript.DefaultRequest.MaxDiffPGU,
                    Query = "",
                    Offset = 0,
                    SortBy = "RANDOM",
                    SpecialDifficulties = new List<string>(LevelListScript.DefaultRequest.SpecialDifficulties),
                    QDifficulties = new List<string>(LevelListScript.DefaultRequest.QDifficulties)
                };

                await request.GetAnswerAsync(token);

                var json = JsonConvert.DeserializeObject<LevelListInfoJson>(request.Answer);
                if (json != null && json.Results != null && json.Results.Count > 0)
                {
                    if (DownloadPanel.instance != null && DownloadPanel.instance.IsDownloading)
                        return;

                    selectedLevel = json.Results[0];
                }
            }

            if (ErrorScript.instance != null)
                UITransition.Hide(ErrorScript.instance.gameObject, 0.12f);

            if (selectedLevel == null)
            {
                ExceptionCatch(new Exception("Failed to pick a random level."));
                return;
            }

            if (string.IsNullOrEmpty(selectedLevel.DlLink) || selectedLevel.DlLink.Length < 10 || !selectedLevel.DlLink.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                ExceptionCatch(new Exception($"This level has an invalid or missing download link ('{selectedLevel.DlLink}')."));
                return;
            }

            LevelDownloader levelDownloader = new(selectedLevel)
            {
                ErrorHandler = (ex) =>
                {
                    DirectLevel.Utils.RunAtMainThread(() => ExceptionCatch(ex));
                }
            };

            _lastLevel = selectedLevel;
            levelDownloader.DownloadComplete += OnCompleteDownload;

            if (DownloadPanel.instance != null)
            {
                DownloadPanel.instance.DownloadLevel(levelDownloader);
            }
        }
        catch (OperationCanceledException)
        {
            // Ignored when canceled via CancellationToken
        }
        catch (Exception ex)
        {
            ExceptionCatch(ex);
        }
    }

    private void OnCompleteDownload(object sender, DownloadCompleteEventArgs args)
    {
        if (sender is LevelDownloader downloader)
        {
            downloader.DownloadComplete -= OnCompleteDownload;
        }

        switch (args.Levels.Count)
        {
            case 0:
                ExceptionCatch(new Exception("ADOFAI level file was not found in downloaded package."));
                break;
            case 1:
                UIScript.SwipeToBlack(() => ADOFAIGameplayHandler.LaunchLevel(args.Levels[0], _lastLevel));
                break;
            default:
                if (LevelSelector.instance != null)
                {
                    LevelSelector.instance.LevelInfo = _lastLevel;
                    StartCoroutine(LevelSelector.instance.LoadLevelsCo(args.Levels, _lastLevel));
                }
                break;
        }
    }

    private CancellationToken CancelAndCreateNewToken()
    {
        _requestCancelToken?.Cancel();
        _requestCancelToken?.Dispose();
        _requestCancelToken = new CancellationTokenSource();
        return _requestCancelToken.Token;
    }

    private void ExceptionCatch(Exception ex)
    {
        if (ErrorScript.instance != null)
        {
            ErrorScript.ShowError(ex.Message);
        }
        Main.Logger.Error(ex.Message + "\n" + ex.StackTrace);
    }
}
