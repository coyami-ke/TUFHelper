using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SpriteLoader : MonoBehaviour
{
    public static SpriteLoader instance;

    public void Awake()
    {
        if (instance == null) instance = this;
    }
    public void Start()
    {
        this.gameObject.SetActive(false);
    }
    public Image image;
    public void FromFile(string path)
    {
        StartCoroutine(FromFileCoroutine(path));
    }

    private IEnumerator FromFileCoroutine(string path)
    {
        string filePath = "file:///" + path.Replace("\\", "/");

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(filePath))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to load image: " + uwr.error);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                Sprite sprite = Sprite.Create(texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));
                image.sprite = sprite;
            }
        }
    }
}