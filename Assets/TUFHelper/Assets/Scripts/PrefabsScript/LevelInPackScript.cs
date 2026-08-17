using DG.Tweening;
using DirectLevel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Security.Policy;
using TMPro;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using TUFHelper.ModScripts.Web;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelInPackScript : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image diffIconImage, backgroundImage;
    public TextMeshProUGUI levelNameText, artistText, creatorText, idText, clearsText, likesText;

    public string PackID { get; private set; }

    public LevelListInfoElementJson LevelInfo { get; private set; }

    public void SetLevelInfo(PackItemNode node, string packId)
    {
        if (!node.IsLevel) return;

        LevelInfo = node.ReferencedLevel;

        levelNameText.text = node.ReferencedLevel.Song;
        artistText.text = node.ReferencedLevel.Artist;
        creatorText.text = node.ReferencedLevel.Creator;
        diffIconImage.sprite = Main.GetSpriteFromAssets(DiffSpriteHelper.GetSpriteFromId(node.ReferencedLevel.DiffId));
        idText.text = "#" + node.ReferencedLevel.ID;
        clearsText.text = "Clears: " + node.ReferencedLevel.Clears;
        likesText.text = node.ReferencedLevel.Likes.ToString();

        PackID = packId;
    }

    public async void OnPointerClick(PointerEventData eventData)
    {
        Main.Logger.Log("OnPointerClick");

        ErrorScript.instance.gameObject.SetActive(false);

        string url = $"https://api.tuforums.com/v2/database/levels/{LevelInfo.ID}";
        using var response = await Main.Client.GetAsync(url);

        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();

        var settings = new JsonSerializerSettings
        {
            Error = (sender, args) =>
            {
                Main.Logger.Log($"[JSON Error] {args.ErrorContext.Error.Message} at {args.ErrorContext.Path}");
                args.ErrorContext.Handled = true; 
            }
        };

        var deserializedLevel = JsonConvert.DeserializeObject<LevelListElementId>(json, settings);
        if (deserializedLevel == null) Main.Logger.Log("The Level ID is null");
        var level = deserializedLevel.Level;

        if (deserializedLevel.Level == null) Main.Logger.Log("The Level is null");

        lastLevel = level;

        Main.Logger.Log($"LLL: Base Score : {level.BaseScore}, Diff Base Score : {level.Difficulty.BaseScore}");
        File.WriteAllText(Path.Combine(Main.ModEntry.Path, "level.json"), json);

        try
        {

            LevelDownloader levelDownloder = new(level)
            {
                ErrorHandler = (ex) =>
                {
                    DirectLevel.Utils.RunAtMainThread(() => ExceptionCatch(ex));
                }
            };

            DownloadPanel.instance.DownloadLevel(levelDownloder);

            levelDownloder.DownloadComplete += OnCompleteDownload;

        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            ExceptionCatch(ex);
        }
    }
    private void ExceptionCatch(Exception ex)
    {
        ErrorScript.ShowError(ex.Message);
        Main.Logger.Error(ex.StackTrace);
    }

    private LevelListInfoElementJson lastLevel;
    private async void OnCompleteDownload(object sender, DownloadCompleteEventArgs args)
    {
        switch (args.Levels.Count)
        {
            case 0:
                throw new Exception("adofai file was not found");
            case 1:
                UIScript.SwipeToBlack(() => ADOFAIGameplayHandler.OpenLevel(args.Levels[0], lastLevel, PackID));
                break;
            default:
                LevelSelector.instance.LevelInfo = lastLevel;
                StartCoroutine(LevelSelector.instance.LoadLevelsCo(args.Levels, lastLevel, PackID));
                break;
        }

        ADOFAIGameplayHandler.IsFromTUFHelper = true;
        ADOFAIGameplayHandler.EditorPlayPatch.CurrentLevelInfo = lastLevel;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        backgroundImage.DOColor(new(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, 30 / 255f), 0.5f).SetEase(Ease.OutExpo);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        backgroundImage.DOColor(new(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, 10 / 255f), 0.5f).SetEase(Ease.OutExpo);
    }
}
