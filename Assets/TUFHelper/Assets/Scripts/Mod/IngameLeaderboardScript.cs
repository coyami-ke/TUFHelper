using System.Collections;
using System.Collections.Generic;
using TUFHelper.ModScripts.Json;
using UnityEngine;

public class IngameLeaderboardScript : MonoBehaviour
{
    public GameObject parentList, prefab;
    
    public static IngameLeaderboardScript instance { get; private set; }

    public void Awake()
    {
        instance = this;
    }
    public void LoadLeaderboard(PassesListInfoElementJson[] passes)
    {
        foreach (Transform child in parentList.transform)
            Destroy(child.gameObject);

        int rank = 1;
        foreach (var pass in passes)
        {
            GameObject obj = Instantiate(prefab, parentList.transform);
            RectTransform rect = obj.GetComponent<RectTransform>();

            var script = obj.GetComponent<IngamerankPrefabScript>();
            script.LoadPass(pass, rank);
            script.UpdateVisual();

            // rect.localScale = Vector3.one;
            // rect.sizeDelta = new Vector2(0, 60);
            // rect.anchoredPosition = new Vector2(0, (rank - 1) * -50);
            rank++;
        }
    }
}
