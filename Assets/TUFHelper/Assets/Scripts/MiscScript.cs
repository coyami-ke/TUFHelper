using TUFHelper;
using UnityEngine;
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
        UIScript.SwipeToBlack(() =>
        {
            Main.isInTUFHelper = false;
            GCS.sceneToLoad = "";
            SceneManager.LoadScene("scnLevelSelect");
        });
    }

    public void OpenURL(string url)
    {
        if (new System.Random().Next(1, 100) == 1)
        {
            Application.OpenURL("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
        }
        else
        {
            Application.OpenURL(url);
        }
    }


}
