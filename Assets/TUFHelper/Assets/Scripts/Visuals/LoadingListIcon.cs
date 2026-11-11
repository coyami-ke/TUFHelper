using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingListIcon : MonoBehaviour
{
    public RectTransform icon1, icon2, cross;
    public TextMeshProUGUI text;
    public float progress = 0;
    public float radius = 100;

    private float speed = 1f;
    private float spedProgress = 0;

    public void Start()
    {
        
    }

    public void Update()
    {
        progress += Time.deltaTime;

        speed = 1 + 0.5f + Mathf.Cos(progress * 2 + Mathf.PI) / 2;

        spedProgress += Time.deltaTime * speed * 1.5f;

        icon1.AnchorPosX(Mathf.Sin(spedProgress) * radius);
        icon1.AnchorPosY(Mathf.Cos(spedProgress) * (radius / 2));
        icon1.rotation = Quaternion.Euler(icon1.rotation.x, icon1.rotation.y, spedProgress * 16);

        icon2.AnchorPosX(Mathf.Sin(spedProgress + Mathf.PI) * radius);
        icon2.AnchorPosY(Mathf.Cos(spedProgress + Mathf.PI) * (radius / 2));
        icon2.rotation = Quaternion.Euler(icon2.rotation.x, icon2.rotation.y, spedProgress * 16 + 135f);

        cross.rotation = Quaternion.Euler(cross.rotation.x, cross.rotation.y, spedProgress * 16 - 25f);

        float normalizedPosY_1 = 1 - (radius / 2 + icon1.anchoredPosition.y) / radius;
        float normalizedPosY_2 = 1 - (radius / 2 + icon2.anchoredPosition.y) / radius;

        icon1.ScaleXY(normalizedPosY_1 / 2 + 0.5f);
        icon2.ScaleXY(normalizedPosY_2 / 2 + 0.5f);

        int countPoints = Mathf.FloorToInt(progress * 3) % 4;
        List<char> chars = new();
        for (int i = 0; i < countPoints; i++)
        {
            chars.Add('.');
        }
        
        text.text = "LOADING" + new string(chars.ToArray());
    }
}
