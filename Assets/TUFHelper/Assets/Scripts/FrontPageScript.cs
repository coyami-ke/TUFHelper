using TUFHelper;
using TUFHelper.Utils;
using UnityEngine;

public class FrontPageScript : MonoBehaviour
{
    public GameObject frontPageObject;
    public GameObject playCanvasObject;
    public GameObject packsCanvasObject;
    public GameObject ratingPageObject;

    public static bool isFirstRun = true;

    public bool IsRatingPageActive { get; private set; } = false;
    public static bool IsPackListActive { get; set; } = false;
    public static string LastOpenedPackId { get; set; } = string.Empty;
    public static int LastOpenedPackLevelId { get; set; } = -1;

    public static FrontPageScript instance { get; private set; }

    public async void Awake()
    {
        instance = this;

        if (isFirstRun)
        {
            isFirstRun = false;
            return;
        }

        if (Main.isInTUFHelper)
        {
            frontPageObject.SetActive(false);

            if (!IsPackListActive)
            {
                playCanvasObject.SetActive(true);
            }
            else
            {
                packsCanvasObject.SetActive(true);
                await PackListScript.Instance.ShowPackView(LastOpenedPackId, LastOpenedPackLevelId);
            }
        }
    }

    private void OnDestroy()
    {
        if (ratingPageObject != null)
        {
            IsRatingPageActive = ratingPageObject.activeSelf;
        }
    }
}