using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuantumGradient : MonoBehaviour
{
    public RawImage targetImage;
    public int width = 512;

    public void Start()
    {
        Texture2D gradientTex = new(width, 1, TextureFormat.RGB24, false)
        {
            wrapMode = TextureWrapMode.Clamp
        };

        Color[] colors = new Color[]
        {
            new Color32(255, 255, 255, 255),
            new Color32(241, 161,   5, 255),
            new Color32(235, 123,  41, 255),
            new Color32(227,  85,  74, 255),
            new Color32(192,  52,  94, 255),
            new Color32(214,  16, 136, 255),
            new Color32(113,  73, 164, 255),
            new Color32( 63,  32, 103, 255),
            new Color32( 47,  43,  54, 255),
            new Color32(126,   0,   0, 255),
            new Color32(255, 254, 254, 255),
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

    private Color SampleGradient(Color[] colorStops, float t)
    {
        float step = 1f / (colorStops.Length - 1);
        int index = Mathf.Min(Mathf.FloorToInt(t / step), colorStops.Length - 2);
        float localT = (t - (index * step)) / step;
        return Color.Lerp(colorStops[index], colorStops[index + 1], localT);
    }
}
