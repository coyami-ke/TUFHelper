using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using TUFHelper;
using UnityEngine;
using UnityEngine.UI;

public class LanguageSettingsRuntime : MonoBehaviour
{
    private readonly List<OptionView> options = new();
    private GameObject optionMenu;
    private TextMeshProUGUI selectedLabel;
    private TextMeshProUGUI arrowLabel;
    private Image selectBackground;
    private bool menuOpen;

    public static void Install(ModSettings settings)
    {
        if (settings == null || settings.tabs == null || settings.tabs.Any(t => t != null && t.nameTab == "Language"))
        {
            return;
        }

        SettingTabPrefabScript sourceTab = settings.tabs.LastOrDefault(t => t != null);
        if (sourceTab == null)
        {
            return;
        }

        GameObject panel = CreatePanel(sourceTab.settingsObject);
        LanguageSettingsRuntime runtime = panel.AddComponent<LanguageSettingsRuntime>();
        runtime.BuildPanel();

        GameObject tabObject = Instantiate(sourceTab.gameObject, sourceTab.transform.parent);
        tabObject.name = "LanguageTab";
        SettingTabPrefabScript languageTab = tabObject.GetComponent<SettingTabPrefabScript>();
        languageTab.nameTab = "Language";
        languageTab.settingsObject = panel;

        SetTabLabel(tabObject, "Language");
        SetTabIcon(tabObject);

        int sourceIndex = sourceTab.transform.GetSiblingIndex();
        tabObject.transform.SetSiblingIndex(sourceIndex);

        List<SettingTabPrefabScript> tabs = settings.tabs.ToList();
        tabs.Insert(tabs.Count - 1, languageTab);
        settings.tabs = tabs.ToArray();
        LayoutTabs(settings.tabs);

        panel.SetActive(false);
        BundleFontFixer.FixFontsIn(tabObject);
        BundleFontFixer.FixFontsIn(panel);
        LanguageManager.ApplyTo(settings.gameObject);
    }

    private static GameObject CreatePanel(GameObject source)
    {
        Transform parent = source != null ? source.transform.parent : ModSettings.instance.transform;
        GameObject panel = new("LanguageSettings", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        if (source != null && source.TryGetComponent(out RectTransform sourceRect))
        {
            rect.anchorMin = sourceRect.anchorMin;
            rect.anchorMax = sourceRect.anchorMax;
            rect.anchoredPosition = sourceRect.anchoredPosition;
            rect.sizeDelta = sourceRect.sizeDelta;
            rect.pivot = sourceRect.pivot;
        }
        else
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        Image image = panel.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.011764706f);
        image.raycastTarget = true;

        return panel;
    }

    private static void SetTabLabel(GameObject tabObject, string text)
    {
        TextMeshProUGUI label = tabObject.GetComponentsInChildren<TextMeshProUGUI>(true)
            .FirstOrDefault(t => t.gameObject.name == "Setting")
            ?? tabObject.GetComponentInChildren<TextMeshProUGUI>(true);

        if (label != null)
        {
            LanguageManager.RememberOriginal(label, text);
        }
    }

    private static void SetTabIcon(GameObject tabObject)
    {
        Image icon = tabObject.GetComponentsInChildren<Image>(true)
            .FirstOrDefault(i => i.gameObject.name == "Icon");

        Sprite sprite = LoadLanguageIcon();
        if (icon != null && sprite != null)
        {
            icon.sprite = sprite;
            icon.color = Color.white;
            icon.preserveAspect = true;

            if (icon.TryGetComponent(out RectTransform rect))
            {
                rect.anchorMin = new Vector2(0f, 0.5f);
                rect.anchorMax = new Vector2(0f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(50.4f, 0f);
                rect.sizeDelta = new Vector2(58f, 58f);
            }
        }
    }

    private static Sprite LoadLanguageIcon()
    {
        string path = Path.Combine(Main.ModEntry.Path, "language.png");
        byte[] bytes = File.Exists(path) ? File.ReadAllBytes(path) : LoadEmbeddedLanguageIcon();
        if (bytes == null || bytes.Length == 0)
        {
            return null;
        }

        Texture2D texture = new(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(bytes))
        {
            return null;
        }

        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    private static byte[] LoadEmbeddedLanguageIcon()
    {
        Assembly assembly = typeof(LanguageSettingsRuntime).Assembly;
        using Stream stream = assembly.GetManifestResourceStream("TUFHelper.language.png");
        if (stream == null)
        {
            return null;
        }

        using MemoryStream memory = new();
        stream.CopyTo(memory);
        return memory.ToArray();
    }

    private static void LayoutTabs(SettingTabPrefabScript[] tabs)
    {
        float startY = -57.2f;
        float spacing = 93.55f;

        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i] == null)
            {
                continue;
            }

            RectTransform rect = tabs[i].GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, startY - spacing * i);
        }
    }

    private void BuildPanel()
    {
        CreateText("Follow the game's language setting.", new Vector2(0, -120), 19, TextAlignmentOptions.Center);

        CreateSelectBox(new Vector2(0, -230));

        Refresh();
    }

    private TextMeshProUGUI CreateText(string english, Vector2 anchoredPosition, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject obj = new(english, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        obj.transform.SetParent(transform, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(720, 54);

        TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        LanguageManager.RememberOriginal(text, english);

        return text;
    }

    private void CreateSelectBox(Vector2 anchoredPosition)
    {
        GameObject obj = new("LanguageSelectOption", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        obj.transform.SetParent(transform, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(560, 62);

        selectBackground = obj.GetComponent<Image>();
        selectBackground.color = new Color(0.12f, 0.14f, 0.2f, 0.88f);

        Button button = obj.GetComponent<Button>();
        button.targetGraphic = selectBackground;
        button.onClick.AddListener(ToggleMenu);

        selectedLabel = CreateChildText(obj.transform, "SelectedLanguage", "Auto (Game Language)", 24, TextAlignmentOptions.Left);
        RectTransform selectedRect = selectedLabel.GetComponent<RectTransform>();
        selectedRect.offsetMin = new Vector2(24, 0);
        selectedRect.offsetMax = new Vector2(-70, 0);

        arrowLabel = CreateChildText(obj.transform, "SelectArrow", "v", 26, TextAlignmentOptions.Center);
        RectTransform arrowRect = arrowLabel.GetComponent<RectTransform>();
        arrowRect.anchorMin = new Vector2(1f, 0f);
        arrowRect.anchorMax = new Vector2(1f, 1f);
        arrowRect.pivot = new Vector2(0.5f, 0.5f);
        arrowRect.anchoredPosition = new Vector2(-34, 0);
        arrowRect.sizeDelta = new Vector2(46, 0);

        optionMenu = new GameObject("LanguageSelectMenu", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        optionMenu.transform.SetParent(transform, false);
        RectTransform menuRect = optionMenu.GetComponent<RectTransform>();
        menuRect.anchorMin = new Vector2(0.5f, 1f);
        menuRect.anchorMax = new Vector2(0.5f, 1f);
        menuRect.anchoredPosition = anchoredPosition + new Vector2(0, -151);
        menuRect.sizeDelta = new Vector2(560, 232);

        Image menuImage = optionMenu.GetComponent<Image>();
        menuImage.color = new Color(0.07f, 0.08f, 0.12f, 0.96f);
        menuImage.raycastTarget = true;

        CreateDropdownOption("Auto (Game Language)", "Auto", new Vector2(0, 87));
        CreateDropdownOption("English", "English", new Vector2(0, 29));
        CreateDropdownOption("Korean", "Korean", new Vector2(0, -29));
        CreateDropdownOption("Chinese", "Chinese", new Vector2(0, -87));
        SetMenuOpen(false);
    }

    private TextMeshProUGUI CreateChildText(Transform parent, string name, string english, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject obj = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        LanguageManager.RememberOriginal(text, english);
        return text;
    }

    private void CreateDropdownOption(string englishLabel, string mode, Vector2 anchoredPosition)
    {
        GameObject obj = new(englishLabel + "Option", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        obj.transform.SetParent(optionMenu.transform, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(540, 50);

        Image image = obj.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0f);

        Button button = obj.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() =>
        {
            LanguageManager.SetLanguageMode(mode);
            SetMenuOpen(false);
            Refresh();
        });

        TextMeshProUGUI label = CreateChildText(obj.transform, "Label", englishLabel, 23, TextAlignmentOptions.Left);
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.offsetMin = new Vector2(24, 0);
        labelRect.offsetMax = new Vector2(-56, 0);

        TextMeshProUGUI check = CreateChildText(obj.transform, "Check", "✓", 23, TextAlignmentOptions.Center);
        RectTransform checkRect = check.GetComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(1f, 0f);
        checkRect.anchorMax = new Vector2(1f, 1f);
        checkRect.pivot = new Vector2(0.5f, 0.5f);
        checkRect.anchoredPosition = new Vector2(-30, 0);
        checkRect.sizeDelta = new Vector2(36, 0);

        options.Add(new OptionView(mode, englishLabel, image, check));
    }

    private void ToggleMenu()
    {
        SetMenuOpen(!menuOpen);
    }

    private void SetMenuOpen(bool open)
    {
        menuOpen = open;
        if (optionMenu != null)
        {
            optionMenu.SetActive(open);
        }

        if (arrowLabel != null)
        {
            LanguageManager.RememberOriginal(arrowLabel, open ? "^" : "v");
        }

        if (selectBackground != null)
        {
            selectBackground.color = open
                ? new Color(0.16f, 0.18f, 0.26f, 0.96f)
                : new Color(0.12f, 0.14f, 0.2f, 0.88f);
        }
    }

    private void Refresh()
    {
        string selected = Main.Setting?.Language ?? "Auto";
        foreach (OptionView option in options)
        {
            bool isSelected = option.Mode == selected;
            option.Background.color = isSelected
                ? new Color(0.35f, 0.47f, 0.72f, 0.42f)
                : new Color(1f, 1f, 1f, 0f);

            option.Check.gameObject.SetActive(isSelected);
            if (isSelected && selectedLabel != null)
            {
                LanguageManager.RememberOriginal(selectedLabel, option.EnglishLabel);
            }
        }
    }

    private readonly struct OptionView
    {
        public readonly string Mode;
        public readonly string EnglishLabel;
        public readonly Image Background;
        public readonly TextMeshProUGUI Check;

        public OptionView(string mode, string englishLabel, Image background, TextMeshProUGUI check)
        {
            Mode = mode;
            EnglishLabel = englishLabel;
            Background = background;
            Check = check;
        }
    }
}
