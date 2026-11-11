using TMPro;
using UnityEngine;

public class TagGroupPrefabScript : MonoBehaviour
{
    public TextMeshProUGUI text;

    public void SetGroupInfo(string group)
    {
        text.text = group;
    }
}
