using System;
using System.Threading;
using System.Threading.Tasks;
using DirectLevel;
using Newtonsoft.Json;
using TMPro;
using Together.Utils;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class MiscScript : MonoBehaviour
{

    public static MiscScript instance;

    public GameObject errorObject;

    public void Awake()
    {
        instance = this;

        errorObject.SetActive(true);
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

        UIScript.SwipeToBlack(() =>
        {
            if (SceneManager.GetActiveScene().name.Equals("TUFLevelInfo"))
            {
                Main.isInTUFHelper = false;
                GCS.sceneToLoad = "";
                SceneManager.LoadScene("Assets/TUFHelper/Scenes/TUFLevelSelect.unity");
            }
            else
            {
                Main.isInTUFHelper = false;
                GCS.sceneToLoad = "";
                SceneManager.LoadScene("scnLevelSelect");
            }
        });
    }

    public void OpenURL(string url)
    {
        //if (DownloadPopupScript.IsDownloading) return;

        if (new System.Random().Next(1, 100) == 1)
        {
            Application.OpenURL("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
        }
        else
        {
            Application.OpenURL(url);
        }
    }

    private CancellationTokenSource requestCancelToken;
    public async void UpdateOfflineLevels(TextMeshProUGUI textInfo)
    {
        requestCancelToken?.Cancel();
        requestCancelToken = new CancellationTokenSource();

        CancellationToken token = requestCancelToken.Token;

        int count = Main.Setting.DownloadedLevels.Count;
        int i = 0;
        foreach (var level in Main.Setting.DownloadedLevels)
        {
            if (level.LevelInfo == null) continue;
            try
            {
                string url = $"https://api.tuforums.com/v2/database/levels/byId/{level.LevelInfo.ID}";

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
                level.LevelInfo = newLevel;

                textInfo.text = $"UPDATE INFO ({i + 1}/{count})...";

                i++;
            }
            catch (Exception ex)
            {
            }
        }
        Main.Setting.Save(Main.ModEntry);
        LevelListScript.instance.ClearLevels();
        LevelListScript.instance.UpdateLevelList();
        textInfo.text = "UPDATE INFO";
    }
}
