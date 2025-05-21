using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SpriteLoader : MonoBehaviour
{
    public static SpriteLoader instance;

    public void Awake()
    {
        if (instance == null) instance = this;
    }
    public Image image;
    public void FromFile(string path)
    {
        Texture2D texture = new(2, 2);
        texture.LoadImage(File.ReadAllBytes(path));

        Sprite sprite = Sprite.Create(texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f));
        image.sprite = sprite;
    }
}