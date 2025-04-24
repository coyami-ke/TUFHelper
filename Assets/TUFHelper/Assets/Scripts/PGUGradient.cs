using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PGUGradient : MonoBehaviour
{
    public RawImage targetImage;
    public int width = 512;

    void Start()
    {
        Texture2D gradientTex = new Texture2D(width, 1, TextureFormat.RGB24, false);
        gradientTex.wrapMode = TextureWrapMode.Clamp;

        // Define key gradient colors
        Color[] colors = new Color[]
        {
            new Color32(0, 153, 255, 255),     // Cyan Blue
            new Color32(0, 255, 136, 255),     // Bright Green
            new Color32(242, 167, 0, 255),     // Orange
            new Color32(225, 79, 79, 255),     // Red
            new Color32(210, 0, 151, 255),     // Magenta/Purple
            new Color32(45, 29, 65, 255),      // Deep Purple
            new Color32(0, 0, 0, 255)          // Black
        };

        for (int x = 0; x < width; x++)
        {
            float t = (float)x / (width - 1);
            Color color = SampleGradient(colors, t);
            gradientTex.SetPixel(x, 0, color);
        }

        gradientTex.Apply();

        if (targetImage != null)
            targetImage.texture = gradientTex;
    }

    Color SampleGradient(Color[] colorStops, float t)
    {
        float step = 1f / (colorStops.Length - 1);
        int index = Mathf.Min(Mathf.FloorToInt(t / step), colorStops.Length - 2);
        float localT = (t - (index * step)) / step;
        return Color.Lerp(colorStops[index], colorStops[index + 1], localT);
    }
}
