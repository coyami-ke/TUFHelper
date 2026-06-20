using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using TUFHelper;
using UnityEngine;
using UnityEngine.EventSystems;

public class SearchScript : MonoBehaviour
{

    public static SearchScript instance;
    public static string searchText = "";
    public TMP_InputField searchField;

    public void Awake()
    {
        instance = this;
        RestoreSearchField();
    }

    public void OnEnable()
    {
        RestoreSearchField();
        StartCoroutine(RestoreSearchFieldNextFrame());
    }

    private System.Collections.IEnumerator RestoreSearchFieldNextFrame()
    {
        yield return null;
        RestoreSearchField();
    }

    private void RestoreSearchField()
    {
        if (searchField == null) return;

        searchField.interactable = true;
        searchField.readOnly = false;
        searchField.text = searchText;
        searchField.ForceLabelUpdate();
    }

    public static string NormalizeSearchText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "";

        string normalized = text.Normalize(NormalizationForm.FormKC);
        normalized = Regex.Replace(normalized, @"\s+", " ");
        return normalized.Trim();
    }

    public async void OnEndEdit(string text)
    {
        string normalized = NormalizeSearchText(text);
        if (searchText == normalized) return;

        searchText = normalized;
        if (searchField != null && searchField.text != normalized)
        {
            searchField.SetTextWithoutNotify(normalized);
            searchField.ForceLabelUpdate();
        }

        LevelListScript.DefaultRequest.Query = normalized;
        LevelListScript.DefaultRequest.Offset = 0;
        LevelListScript.instance.ClearLevels();
        await LevelListScript.instance.UpdateLevelListAsync();
    }
    public void Update()
    {
        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(searchField.gameObject);
                }
                searchField.Select();
                searchField.ActivateInputField();
                searchField.MoveTextEnd(false);
            }
        }       
    }
}
