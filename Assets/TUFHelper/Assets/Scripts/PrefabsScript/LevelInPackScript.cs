using DG.Tweening;
using DirectLevel;
using System;
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

    public LevelListInfoElementJson LevelInfo { get; private set; }

    public void SetLevelInfo(PackItemNode node)
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
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ErrorScript.instance.gameObject.SetActive(false);

        try
        {

            LevelDownloader levelDownloder = new(LevelInfo)
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

    private void OnCompleteDownload(object sender, DownloadCompleteEventArgs args)
    {
        switch (args.Levels.Count)
        {
            case 0:
                throw new Exception("adofai file was not found");
            case 1:
                UIScript.SwipeToBlack(() => ADOFAIGameplayHandler.OpenLevel(args.Levels[0], LevelInfo));
                break;
            default:
                LevelSelector.instance.LevelInfo = LevelInfo;
                StartCoroutine(LevelSelector.instance.LoadLevelsCo(args.Levels, LevelInfo));
                break;
        }

        ADOFAIGameplayHandler.IsFromTUFHelper = true;
        ADOFAIGameplayHandler.EditorPlayPatch.CurrentLevelInfo = LevelInfo;
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
