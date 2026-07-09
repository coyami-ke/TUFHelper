using ADOFAIModdingHelper.Common;
using ADOFAIModdingHelper.ScriptableObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace ADOFAIModdingHelper.Windows
{
    public class ModConfigWindow : EditorWindow
    {
        // -----------------------------------------------------------------------
        // Serialized UXML Assets
        // -----------------------------------------------------------------------

        [SerializeField] private VisualTreeAsset MainPanel;
        [SerializeField] private VisualTreeAsset SubPanel;
        [SerializeField] private VisualTreeAsset ListElement;
        [SerializeField] private VisualTreeAsset ButtonTemplate;

        // -----------------------------------------------------------------------
        // UI Element Name Constants
        // -----------------------------------------------------------------------

        private static class Names
        {
            // Root
            public const string OptionsList = "OptionsList";
            public const string ModConfigContainer = "ModConfigContainer";

            // Settings panel
            public const string PathTextField = "ADOFAIPathTextField";
            public const string PathBrowseButton = "ADOFAIPathBrowseButton";
            public const string ImportGameButton = "ImportGameButton";
            public const string SeperateTabsToggle = "SeperateTabsToggle";
            public const string DecompilerPathTextField = "DecompilerPathTextField";
            public const string DecompilerPathBrowseButton = "DecompilerPathBrowseButton";
            public const string OpenDecompilerButton = "OpenDecompilerButton";

            // Mod Info
            public const string ModEntry = "ModEntry";
            public const string NoModInfo = "NoModInfo";
            public const string ModInfoLabelButton = "ModEntryLabelButton";
            public const string ModInfoArrowButton = "ModEntryArrowButton";
            public const string ModInfoDataContainer = "ModInfoDataContainer";

            // Builder
            public const string AssetBundleList = "AssetBundleListView";
            public const string PrecompList = "PreCompListView";
            public const string RawFileCopiesList = "RawFileCopiesListView";
            public const string Platforms = "Platforms";
            public const string AllPlatformToggle = "AllPlatformABToggle";
            public const string WindowsButton = "windowsButton";
            public const string MacOSButton = "macOSButton";
            public const string LinuxButton = "linuxButton";
            public const string DebugPresetButton = "debugBuildButton";
            public const string ReleasePresetButton = "relaseBuildButton";
            public const string ClearPresetButton = "clearBuildOptionButton";
            public const string BuildButton = "BuildButton";

            // Build Management
            public const string CachedBuildLabel = "CachedBuildLabel";
            public const string OpenBuildDirectory = "OpenBuildDirectory";
            public const string DeleteAllBuilds = "DeleteAllBuilds";

            // Shortcuts
            public const string SceneNav = "SceneNa";
            public const string AsmDefNav = "AsmDefNav";
            public const string RepoButton = "RepositoryButton";
            public const string IssuesButton = "IssuesButton";
            public const string PRButton = "PullRequestsButton";
            public const string GithubConfigLabelButton = "GithubConfigLabelButton";
            public const string GithubConfigArrowButton = "GithubConfigArrowButton";
            public const string GithubConfigContainer = "GithubConfigContainer";
            public const string ScenePathTextField = "ScenesDirectoryTextField";
            public const string ScenePathBrowsBtn = "SceneDirBrowseButton";
            public const string NoScenes = "NoScene";
            public const string NoAsmdefs = "NoAsmdef";

            // Panel names
            public const string PanelSettings = "SettingsProperties";
            public const string PanelModInfo = "ModInfoProperties";
            public const string PanelBuilder = "BuilderProperties";
            public const string PanelBuildManagement = "BuildManagementProperties";
            public const string PanelShortcut = "ShortcutProperties";

            public const string BarSpacer = "BarSpacer";
        }

        //// -----------------------------------------------------------------------
        //// Nav Mapping: option name, panel name
        //// -----------------------------------------------------------------------

        private static readonly IReadOnlyDictionary<string, string> NavMap =
            new Dictionary<string, string>
            {
                { "Settings" , Names.PanelSettings },
                { "Mod Info",          Names.PanelModInfo },
                { "Builder",          Names.PanelBuilder },
                { "Build Management",  Names.PanelBuildManagement },
                { "Shortcuts",         Names.PanelShortcut },
            };

        // -----------------------------------------------------------------------
        // State
        // -----------------------------------------------------------------------

        private readonly string[] _settingsOptionsSep = { "Settings", "Mod Info", "Builder", "Build Management", "Shortcuts" };
        private readonly string[] _settingsOptionsNoSep = { "Settings", "Build", "Shortcuts" };

        private ListView _optionsList;
        private VisualElement _modConfigContainer;
        private VisualElement _subPanel;

        private static bool _showModInfo = true;
        private static bool _showGithubConfig = true;

        private readonly HashSet<string> _scenes = new();
        private readonly HashSet<string> _asmDefs = new();
        private readonly Dictionary<string, string> _asmDefRelativePaths = new();

        private FileSystemWatcher _buildDirectoryWatcher;
        private FileSystemWatcher _assemblyDefinitionsWatcher;
        private FileSystemWatcher _scenesWatcher;

        // -----------------------------------------------------------------------
        // Menu Item
        // -----------------------------------------------------------------------

        [MenuItem(Constants.ADOFAIModdingHelperMenuRoot + "Mods Configs", false,
            priority: Constants.ADOFAIModdingHelperMenuPriority - 2)]
        public static void OpenInfo()
        {
            var window = GetWindow<ModConfigWindow>("Mod Config");
            window._optionsList?.SetSelection(1);
        }

        [MenuItem(Constants.ADOFAIModdingHelperMenuRoot + "Builds Configs", false,
            priority: Constants.ADOFAIModdingHelperMenuPriority - 2)]
        public static void OpenBuild()
        {
            var window = GetWindow<ModConfigWindow>("Mod Config");
            window._optionsList?.SetSelection(Setting.Config.SeperateBuildTabs ? 2 : 1);
        }

        [MenuItem(Constants.ADOFAIModdingHelperMenuRoot + "Settings", false,
            priority: Constants.ADOFAIModdingHelperMenuPriority - 2)]
        public static void ShowSetting()
        {
            GetWindow<ModConfigWindow>("Mod Config");
        }

        // -----------------------------------------------------------------------
        // Lifecycle
        // -----------------------------------------------------------------------

        private void CreateGUI()
        {
            MainPanel.CloneTree(rootVisualElement);

            _optionsList = rootVisualElement.Q<ListView>(Names.OptionsList);
            _modConfigContainer = rootVisualElement.Q<VisualElement>(Names.ModConfigContainer);

            var chosenOptions = new List<string>(Setting.Config.SeperateBuildTabs ? _settingsOptionsSep : _settingsOptionsNoSep);
            _optionsList.itemsSource = chosenOptions;
            _optionsList.makeItem = () => ListElement.CloneTree().ElementAt(0);
            _optionsList.bindItem = (element, i) =>
            {
                var source = _optionsList.itemsSource as List<string>;
                if (source != null && i < source.Count)
                {
                    element.Q<Label>().text = source[i];
                }
            };

            _optionsList.selectionChanged += items =>
            {
                _modConfigContainer.Clear();
                if (items.FirstOrDefault() is not string selectedItem) return;
                OnSettingSelected(selectedItem);
                Setting.Config.CurrentPage = _optionsList.selectedIndex;
            };

            _optionsList.SetSelection(Setting.Config.CurrentPage);
        }

        private void OnDestroy()
        {
            _buildDirectoryWatcher = DisposeWatcher(_buildDirectoryWatcher);
            _assemblyDefinitionsWatcher = DisposeWatcher(_assemblyDefinitionsWatcher);
            _scenesWatcher = DisposeWatcher(_scenesWatcher);
        }

        // -----------------------------------------------------------------------
        // Panel Loader
        // -----------------------------------------------------------------------

        private void OnSettingSelected(string item)
        {
            _modConfigContainer.Clear();

            _subPanel = SubPanel.CloneTree();
            _modConfigContainer.Add(_subPanel);

            _subPanel.TrackPropertyValue(new SerializedObject(Setting.Config).FindProperty("SeperateBuildTabs"), (prop) =>
            {
                UpdateSeparateTabs();
            });

            var chosenOptions = Setting.Config.SeperateBuildTabs ? _settingsOptionsSep : _settingsOptionsNoSep;
            switch (item)
            {
                case "Settings":
                    SetupSettings();
                    SetNav(chosenOptions[0]);
                    break;
                case "Build":
                    SetupModInfo();
                    SetupBuilder();
                    SetupBuildManagement();
                    SetupShortcutsElements();
                    SetupShortcuts();
                    SetNav(chosenOptions[1]);
                    break;
                case "Mod Info":
                    SetupModInfo();
                    SetNav(chosenOptions[1]);
                    break;
                case "Builder":
                    SetupBuilder();
                    SetNav(chosenOptions[2]);
                    break;
                case "Build Management":
                    SetupBuildManagement();
                    SetNav(chosenOptions[3]);
                    break;
                case "Shortcuts":
                    SetupShortcutsElements();
                    SetupShortcuts();
                    SetNav(chosenOptions[Setting.Config.SeperateBuildTabs ? 4 : 2]);
                    break;
            }
        }

        private void SetNav(string nav)
        {
            foreach (var (navBtn, navPanel) in NavMap)
            {
                _subPanel.Q<VisualElement>(navPanel).style.display =
                    navBtn == nav ? DisplayStyle.Flex : DisplayStyle.None;
            }

            bool isCombinedBuild = nav == "Build";
            _subPanel.Query<VisualElement>(Names.BarSpacer).ForEach(e =>
                e.style.display = isCombinedBuild ? DisplayStyle.Flex : DisplayStyle.None);

            if (isCombinedBuild)
            {
                _subPanel.Q<VisualElement>(Names.PanelModInfo).style.display = DisplayStyle.Flex;
                _subPanel.Q<VisualElement>(Names.PanelBuilder).style.display = DisplayStyle.Flex;
                _subPanel.Q<VisualElement>(Names.PanelBuildManagement).style.display = DisplayStyle.Flex;
            }
        }

        // -----------------------------------------------------------------------
        // Settings Panel
        // -----------------------------------------------------------------------

        private void SetupSettings()
        {
            var settingsProperties = _subPanel.Q<VisualElement>(Names.PanelSettings);
            settingsProperties.Bind(new SerializedObject(Setting.Config));

            SetupPathBrowse(settingsProperties);
            SetupDecompilerPathBrowser(settingsProperties);
            SetupImportGame(settingsProperties);
            SetupOpenDecompiler(settingsProperties);
            SetupSeparateTabsToggle();
        }

        private void SetupPathBrowse(VisualElement panel)
        {
            var pathField = panel.Q<TextField>(Names.PathTextField);
            var browseButton = panel.Q<Button>(Names.PathBrowseButton);

            browseButton.clicked += () =>
            {
                // Logic from from https://github.com/ADOFAI-gg/ADOFAI-Modding-Toolkit
                string initialDirectory;
                string extension;

#if UNITY_EDITOR_WIN
                initialDirectory = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\A Dance of Fire and Ice";
                extension = "exe";
#elif UNITY_EDITOR_OSX
                initialDirectory = "~/Library/Application Support/Steam/steamapps/common/ADanceOfFireAndIce";
                extension = "app";
#elif UNITY_EDITOR_LINUX
                initialDirectory = "~/.local/share/Steam/steamapps/common/A Dance of Fire and Ice";
                extension = string.Empty;
#else
                initialDirectory = Application.dataPath;
                extension = string.Empty;
#endif
                string path = EditorUtility.OpenFilePanel("Select ADOFAI Path", initialDirectory, extension);
                if (!string.IsNullOrEmpty(path))
                {
                    pathField.value = path;
                    pathField.Blur();
                }
            };
        }
        private static void SetupImportGame(VisualElement panel)
        {
            var importButton = panel.Q<Button>(Names.ImportGameButton);

            importButton.clicked += () =>
            {
                // Logic from from https://github.com/ADOFAI-gg/ADOFAI-Modding-Toolkit
                bool continueImporting = EditorUtility.DisplayDialog(
                    "Are you sure?",
                    "Importing the game assembly may take a while, and you will likely be asked to restart the Unity Editor.",
                    "Yes, continue",
                    "No");

                if (!continueImporting) return;

                var config = Setting.Config;

                if (string.IsNullOrEmpty(config.ADOFAIPath))
                {
                    EditorUtility.DisplayDialog("Error", "Please set the ADOFAI Executable Path first.", "OK");
                    return;
                }

                config.Importer.SetGamePath(config.ADOFAIPath);
                config.Importer.Import();
            };
        }

        private void SetupDecompilerPathBrowser(VisualElement panel)
        {
            var pathField = panel.Q<TextField>(Names.DecompilerPathTextField);
            var browseButton = panel.Q<Button>(Names.DecompilerPathBrowseButton);

            browseButton.clicked += () =>
            {
                // Logic from from https://github.com/ADOFAI-gg/ADOFAI-Modding-Toolkit
                string initialDirectory = Application.dataPath;
                string extension;

#if UNITY_EDITOR_WIN
                extension = "exe";
#elif UNITY_EDITOR_OSX
                extension = "app";
#elif UNITY_EDITOR_LINUX
                extension = string.Empty;
#else
                extension = string.Empty;
#endif

                string path = EditorUtility.OpenFilePanel("Select Decompiler Path", initialDirectory, extension);
                if (!string.IsNullOrEmpty(path))
                {
                    pathField.value = path;
                    pathField.Blur();
                }
            };
        }

        private static void SetupOpenDecompiler(VisualElement panel)
        {
            var importButton = panel.Q<Button>(Names.OpenDecompilerButton);

            importButton.clicked += () =>
            {
                var config = Setting.Config;

                if (string.IsNullOrEmpty(config.DecompilerPath) || !File.Exists(config.DecompilerPath))
                {
                    EditorUtility.DisplayDialog("Error", "Please set the Decompiler Executable Path first.", "OK");
                    return;
                }
                string gameDirectory = Path.GetDirectoryName(config.DecompilerPath);

                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = config.DecompilerPath,
                    WorkingDirectory = gameDirectory,
                    UseShellExecute = true
                };

                System.Diagnostics.Process.Start(startInfo);
            };
        }

        private void SetupSeparateTabsToggle()
        {
            var toggle = _subPanel.Q<Toggle>(Names.SeperateTabsToggle);

            toggle.RegisterValueChangedCallback(evt =>
            {
                UpdateSeparateTabs();
            });
        }

        public void UpdateSeparateTabs()
        {
            var chosenOptions = new List<string>(Setting.Config.SeperateBuildTabs ? _settingsOptionsSep : _settingsOptionsNoSep);
            _optionsList.itemsSource = chosenOptions;
            _optionsList.bindItem = (element, i) =>
            {
                if (_optionsList.itemsSource is List<string> source && i < source.Count)
                {
                    element.Q<Label>().text = source[i];
                }
            };
            _optionsList.RefreshItems();

            SetNav(chosenOptions[0]);
            _optionsList.SetSelection(0);
        }

        // -----------------------------------------------------------------------
        // Mod Info Section
        // -----------------------------------------------------------------------

        private void SetupModInfo()
        {
            _subPanel.Q<VisualElement>(Names.PanelModInfo).Bind(new SerializedObject(ModInfo.Info));
            _subPanel.Q<VisualElement>(Names.ModEntry).style.display = DisplayStyle.Flex;

            _subPanel.Q<Button>(Names.ModInfoLabelButton).clicked += () =>
                ShowContainer(!_showModInfo, ref _showModInfo,
                    _subPanel.Q<VisualElement>(Names.ModInfoDataContainer),
                    _subPanel.Q<Button>(Names.ModInfoArrowButton));

            _subPanel.Q<Button>(Names.ModInfoArrowButton).clicked += () =>
                ShowContainer(!_showModInfo, ref _showModInfo,
                    _subPanel.Q<VisualElement>(Names.ModInfoDataContainer),
                    _subPanel.Q<Button>(Names.ModInfoArrowButton));

            ShowContainer(_showModInfo, ref _showModInfo,
                                _subPanel.Q<VisualElement>(Names.ModInfoDataContainer),
                                _subPanel.Q<Button>(Names.ModInfoArrowButton));
        }

        // -----------------------------------------------------------------------
        // Builder Section
        // -----------------------------------------------------------------------

        private void SetupBuilder()
        {
            var config = ModToolsConfig.Config;
            var configSO = new SerializedObject(ModToolsConfig.Config);
            var platformContainer = _subPanel.Q<VisualElement>(Names.Platforms);
            var allPlatformToggle = _subPanel.Q<Toggle>(Names.AllPlatformToggle);

            var boundPanel = _subPanel.Q<VisualElement>(Names.PanelBuilder);
            boundPanel.Bind(configSO);

            var assetBundlesProp = configSO.FindProperty("AssetBundles");
            List<string> projectBundles = AssetDatabase.GetAllAssetBundleNames().ToList();

            SetupCustomList(_subPanel.Q<ListView>(Names.AssetBundleList), assetBundlesProp, "BundleDropdown", projectBundles, "(No Bundles Found)");

            var precompProp = configSO.FindProperty("PrecompAssemblies");

            List<string> projectDlls = AssetDatabase.FindAssets("t:DefaultAsset")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith(".dll"))
                .Select(Path.GetFileName)
                .ToList();

            string editorDataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "UnityReferenceAssemblies");

            if (Directory.Exists(editorDataDir))
            {
                var internalDlls = Directory.GetFiles(editorDataDir, "*.dll", SearchOption.AllDirectories)
                    .Select(Path.GetFileName);

                projectDlls.AddRange(internalDlls);
            }

            List<string> allAvailableDlls = projectDlls
                .Distinct()
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            SetupCustomList(_subPanel.Q<ListView>(Names.PrecompList), precompProp, "DllDropdown", allAvailableDlls, "(No DLLs Found)");

            SetupRawFileCopiesList();

            void UpdatePlatformVisibility(bool buildAll) =>
                platformContainer.style.display = buildAll ? DisplayStyle.None : DisplayStyle.Flex;

            UpdatePlatformVisibility(ModToolsConfig.Config.buildEveryPlatform);
            allPlatformToggle.RegisterValueChangedCallback(evt => UpdatePlatformVisibility(evt.newValue));

            SetupPlatformToggleButton(_subPanel, Names.WindowsButton, BuildTarget.StandaloneWindows64, config);
            SetupPlatformToggleButton(_subPanel, Names.MacOSButton, BuildTarget.StandaloneOSX, config);
            SetupPlatformToggleButton(_subPanel, Names.LinuxButton, BuildTarget.StandaloneLinux64, config);

            void ApplyPreset(string preset, bool buildAll)
            {
                config.ApplyPreset(preset);
                configSO.Update();
                UpdatePlatformVisibility(buildAll);
                allPlatformToggle.value = buildAll;
            }

            _subPanel.Q<Button>(Names.DebugPresetButton).clicked += () => ApplyPreset("Debug", false);
            _subPanel.Q<Button>(Names.ReleasePresetButton).clicked += () => ApplyPreset("Release", true);
            _subPanel.Q<Button>(Names.ClearPresetButton).clicked += () => ApplyPreset("Clear", false);

            _subPanel.Q<Button>(Names.BuildButton).clicked += () =>
            {
                string modId = ModInfo.Info.Id;
                if (!string.IsNullOrWhiteSpace(modId))
                {
                    string folderName = !string.IsNullOrWhiteSpace(modId)
                    ? modId
                    : $"{config.name}_{config.GetInstanceID()}";

                    string dest = config.copyToDirectory
                        ? Path.Combine(Path.GetDirectoryName(Setting.Config.ADOFAIPath)!, "Mods", folderName)
                        : null;

                    config.BuildMod(dest);
                }
                else
                {
                    EditorUtility.DisplayDialog("Mod ID is Empty!",
                        "The Mod ID is empty, which will create a folder of type \"config.name_instance\". Cannot proceed.", "Ok");
                }
            };
        }

        // -----------------------------------------------------------------------
        // Raw File Copies List
        // -----------------------------------------------------------------------

        private void SetupRawFileCopiesList()
        {
            var config = ModToolsConfig.Config;
            config.RawFileCopies ??= new List<RawFileCopyEntry>();

            var builderPanel = _subPanel.Q<VisualElement>(Names.PanelBuilder);

            // ------------------------------------------------------------------
            // Inner helper: rebuild the ObjectField + EnumField rows for one entry
            // ------------------------------------------------------------------
            void RebuildFileSlots(VisualElement slotsContainer, int entryIndex)
            {
                slotsContainer.Clear();
                if (entryIndex < 0 || entryIndex >= config.RawFileCopies.Count) return;

                var entry = config.RawFileCopies[entryIndex];
                entry.Files ??= new List<RawFileItem>();

                for (int f = 0; f < entry.Files.Count; f++)
                {
                    int fileIndex = f; // captured per-row

                    var row = new VisualElement
                    {
                        style = { flexDirection = FlexDirection.Row, marginTop = 2 }
                    };

                    // Asset selection field
                    var objField = new ObjectField
                    {
                        objectType = typeof(UnityEngine.Object),
                        allowSceneObjects = false,
                        style = { flexGrow = 1, marginRight = 4 }
                    };
                    objField.SetValueWithoutNotify(entry.Files[fileIndex].File);
                    objField.RegisterValueChangedCallback(evt =>
                    {
                        if (slotsContainer.userData is not int idx || idx >= config.RawFileCopies.Count) return;
                        config.RawFileCopies[idx].Files[fileIndex].File = evt.newValue;
                        EditorUtility.SetDirty(config);
                    });

                    // Per-file platform restriction dropdown
                    var fileTargetField = new EnumField(entry.Files[fileIndex].Target)
                    {
                        style = { width = 100, marginRight = 2 }
                    };
                    fileTargetField.RegisterValueChangedCallback(evt =>
                    {
                        if (slotsContainer.userData is not int idx || idx >= config.RawFileCopies.Count) return;
                        config.RawFileCopies[idx].Files[fileIndex].Target = (BuildTarget)evt.newValue;
                        EditorUtility.SetDirty(config);
                    });

                    // Remove item button
                    var removeBtn = new Button
                    {
                        text = "−",
                        style = { width = 20, paddingLeft = 0, paddingRight = 0 }
                    };
                    removeBtn.clicked += () =>
                    {
                        if (slotsContainer.userData is not int idx || idx >= config.RawFileCopies.Count) return;
                        config.RawFileCopies[idx].Files.RemoveAt(fileIndex);
                        EditorUtility.SetDirty(config);
                        RebuildFileSlots(slotsContainer, idx);
                    };

                    row.Add(objField);
                    row.Add(fileTargetField);
                    row.Add(removeBtn);
                    slotsContainer.Add(row);
                }
            }

            // ------------------------------------------------------------------
            // Outer ListView – one item per entry path
            // ------------------------------------------------------------------
            var outerList = new ListView(config.RawFileCopies)
            {
                name = Names.RawFileCopiesList,
                reorderable = true,
                showAddRemoveFooter = true,
                showBorder = true,
                showFoldoutHeader = true,
                headerTitle = "Raw File Copies",
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                style = { marginTop = 6, marginBottom = 4 }
            };

            outerList.makeItem = () =>
            {
                var root = new VisualElement
                {
                    style = { paddingLeft = 4, paddingRight = 4, paddingTop = 4, paddingBottom = 4 }
                };

                // --- Entry global Target Platform dropdown ---
                var platformRow = new VisualElement
                {
                    style = { flexDirection = FlexDirection.Row, marginBottom = 3 }
                };
                var platformLabel = new Label("Global Target")
                {
                    style = { minWidth = 110, unityTextAlign = TextAnchor.MiddleLeft, fontSize = 11 }
                };
                var platformField = new EnumField(BuildTarget.NoTarget)
                {
                    name = "EntryTargetPlatformField",
                    style = { flexGrow = 1 }
                };
                platformField.RegisterValueChangedCallback(evt =>
                {
                    if (root.userData is not int idx || idx >= config.RawFileCopies.Count) return;
                    config.RawFileCopies[idx].Target = (BuildTarget)evt.newValue;
                    EditorUtility.SetDirty(config);
                });
                platformRow.Add(platformLabel);
                platformRow.Add(platformField);
                root.Add(platformRow);

                // --- Destination path row ---
                var pathRow = new VisualElement
                {
                    style = { flexDirection = FlexDirection.Row, marginBottom = 3 }
                };
                var pathLabel = new Label("Destination Path")
                {
                    style = { minWidth = 110, unityTextAlign = TextAnchor.MiddleLeft, fontSize = 11 }
                };
                var pathField = new TextField
                {
                    name = "RawCopyPathField",
                    style = { flexGrow = 1 }
                };
                pathField.RegisterValueChangedCallback(evt =>
                {
                    if (root.userData is not int idx || idx >= config.RawFileCopies.Count) return;
                    config.RawFileCopies[idx].DestinationPath = evt.newValue;
                    EditorUtility.SetDirty(config);
                });
                pathRow.Add(pathLabel);
                pathRow.Add(pathField);
                root.Add(pathRow);

                // --- Files header row ---
                var filesHeader = new VisualElement
                {
                    style = { flexDirection = FlexDirection.Row, marginBottom = 2 }
                };
                var filesLabel = new Label("Files (Object | Platform)")
                {
                    style = { minWidth = 110, unityTextAlign = TextAnchor.MiddleLeft, fontSize = 11 }
                };
                var addFileBtn = new Button
                {
                    text = "+ Add File",
                    style = { fontSize = 10, paddingTop = 1, paddingBottom = 1 }
                };
                addFileBtn.clicked += () =>
                {
                    if (root.userData is not int idx || idx >= config.RawFileCopies.Count) return;
                    config.RawFileCopies[idx].Files ??= new List<RawFileItem>();
                    config.RawFileCopies[idx].Files.Add(new RawFileItem());
                    EditorUtility.SetDirty(config);

                    var slots = root.Q<VisualElement>("RawFileSlotsContainer");
                    slots.userData = idx;
                    RebuildFileSlots(slots, idx);
                };
                filesHeader.Add(filesLabel);
                filesHeader.Add(addFileBtn);
                root.Add(filesHeader);

                // --- File slots container ---
                var fileSlots = new VisualElement { name = "RawFileSlotsContainer" };
                root.Add(fileSlots);

                return root;
            };

            outerList.bindItem = (element, i) =>
            {
                element.userData = i;
                var entry = config.RawFileCopies[i];

                element.Q<EnumField>("EntryTargetPlatformField").SetValueWithoutNotify(entry.Target);
                element.Q<TextField>("RawCopyPathField").SetValueWithoutNotify(entry.DestinationPath ?? string.Empty);

                var slotsContainer = element.Q<VisualElement>("RawFileSlotsContainer");
                slotsContainer.userData = i;
                RebuildFileSlots(slotsContainer, i);
            };

            outerList.unbindItem = (element, _) =>
            {
                element.userData = null;
                var slotsContainer = element.Q<VisualElement>("RawFileSlotsContainer");
                if (slotsContainer != null) slotsContainer.userData = null;
            };

            outerList.itemsAdded += indices =>
            {
                for (int i = 0; i < config.RawFileCopies.Count; i++)
                {
                    if (config.RawFileCopies[i] == null)
                    {
                        config.RawFileCopies[i] = new RawFileCopyEntry();
                    }
                }
                EditorUtility.SetDirty(config);
                outerList.schedule.Execute(outerList.RefreshItems);
            };

            outerList.itemsRemoved += _ => EditorUtility.SetDirty(config);
            builderPanel.Add(outerList);
        }

        private static void SetupPlatformToggleButton(VisualElement panel, string buttonName,
            BuildTarget target, ModToolsConfig config)
        {
            var btn = panel.Q<Button>(buttonName);

            void Refresh()
            {
                bool active = config.BuildPlatforms.Contains(target);
                btn.style.backgroundColor = active
                    ? new StyleColor(new Color(0, 0.52f, 0.92f))
                    : new StyleColor(StyleKeyword.Null);
                btn.style.unityFontStyleAndWeight = active ? FontStyle.Bold : FontStyle.Normal;
            }

            Refresh();
            btn.clicked += () =>
            {
                var platforms = config.BuildPlatforms;
                if (!platforms.Add(target)) platforms.Remove(target);
                config.BuildPlatforms = platforms;
                EditorUtility.SetDirty(config);
                Refresh();
            };
        }

        private void SetupCustomList(ListView listView, SerializedProperty prop, string dropdownName,
            List<string> choices, string emptyText = "No Items Found")
        {
            listView.BindProperty(prop);

            var originalChoices = new List<string>(choices);
            bool hasChoices = originalChoices.Count > 0;

            List<string> BuildChoicesFor(int index)
            {
                if (!hasChoices) return new List<string>() { emptyText };
                if (index < 0 || index >= prop.arraySize) return new List<string> { "None" };

                var currentValue = prop.GetArrayElementAtIndex(index).stringValue;
                var usedElsewhere = new HashSet<string>();
                for (int j = 0; j < prop.arraySize; j++)
                {
                    if (j == index) continue;
                    var v = prop.GetArrayElementAtIndex(j).stringValue;
                    if (!string.IsNullOrEmpty(v)) usedElsewhere.Add(v);
                }

                var available = new List<string> { "None" };
                foreach (var choice in originalChoices)
                {
                    if (!usedElsewhere.Contains(choice) || choice == currentValue)
                        available.Add(choice);
                }
                return available;
            }

            listView.makeItem = () =>
            {
                var container = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                var label = new Label("Element")
                {
                    name = "IndexLabel",
                    style = { minWidth = 95, unityTextAlign = TextAnchor.MiddleLeft }
                };
                var dropdown = new DropdownField
                {
                    name = dropdownName,
                    style = { flexGrow = 1, paddingBottom = 2, paddingTop = 2, minHeight = 24 }
                };

                var textElement = dropdown.Q<TextElement>();
                if (textElement != null)
                {
                    textElement.style.textOverflow = TextOverflow.Ellipsis;
                    textElement.style.overflow = Overflow.Hidden;
                    textElement.style.whiteSpace = WhiteSpace.NoWrap;
                }

                dropdown.RegisterValueChangedCallback(evt =>
                {
                    if (container.userData is not int idx) return;
                    if (idx >= prop.arraySize) return;

                    var property = prop.GetArrayElementAtIndex(idx);
                    if (string.IsNullOrEmpty(evt.newValue) || evt.newValue == "None" || evt.newValue == emptyText)
                    {
                        property.stringValue = string.Empty;
                        dropdown.SetValueWithoutNotify("None");
                    }
                    else
                    {
                        property.stringValue = evt.newValue;
                    }
                    property.serializedObject.ApplyModifiedProperties();
                    listView.schedule.Execute(listView.Rebuild);
                });

                container.Add(label);
                container.Add(dropdown);
                return container;
            };

            listView.bindItem = (element, i) =>
            {
                element.userData = i;
                element.Q<Label>("IndexLabel").text = $"Element {i}";
                var dropdown = element.Q<DropdownField>(dropdownName);
                var property = prop.GetArrayElementAtIndex(i);
                dropdown.choices = BuildChoicesFor(i);
                var display = string.IsNullOrEmpty(property.stringValue) ? "None" : property.stringValue;
                dropdown.SetValueWithoutNotify(display);
            };

            listView.unbindItem = (element, _) => element.userData = null;

            listView.itemsAdded += indices =>
            {
                prop.serializedObject.Update();
                foreach (var index in indices)
                    prop.GetArrayElementAtIndex(index).stringValue = string.Empty;
                prop.serializedObject.ApplyModifiedProperties();
                listView.schedule.Execute(listView.Rebuild);
            };
        }

        // -----------------------------------------------------------------------
        // Build Management Section
        // -----------------------------------------------------------------------

        private void SetupBuildManagement()
        {
            var config = ModToolsConfig.Config;
            string buildPath = Path.Combine(Directory.GetCurrentDirectory(), "Builds");
            var cachedBuildLabel = _subPanel.Q<Label>(Names.CachedBuildLabel);

            _subPanel.Q<VisualElement>(Names.PanelBuildManagement).Bind(new SerializedObject(config));

            _subPanel.Q<Button>(Names.OpenBuildDirectory).clicked += () =>
            {
                if (!Directory.Exists(buildPath)) Directory.CreateDirectory(buildPath);
                EditorUtility.RevealInFinder(buildPath);
            };

            _subPanel.Q<Button>(Names.DeleteAllBuilds).clicked += () =>
            {
                config.DeleteBuilds(config.deleteBuildsExceptLastN);
                UpdateBuildStats(cachedBuildLabel);
            };

            UpdateBuildStats(cachedBuildLabel);
            ChangeWatchedFolder(buildPath, cachedBuildLabel);
        }

        // -----------------------------------------------------------------------
        // Shortcuts Section
        // -----------------------------------------------------------------------

        private void SetupShortcutsElements()
        {
            var config = ModToolsConfig.Config;
            _subPanel.Q<Button>(Names.GithubConfigLabelButton).clicked += () =>
                ShowContainer(!_showGithubConfig, ref _showGithubConfig,
                    _subPanel.Q<VisualElement>(Names.GithubConfigContainer),
                    _subPanel.Q<Button>(Names.GithubConfigArrowButton));

            _subPanel.Q<Button>(Names.GithubConfigArrowButton).clicked += () =>
                ShowContainer(!_showGithubConfig, ref _showGithubConfig,
                    _subPanel.Q<VisualElement>(Names.GithubConfigContainer),
                    _subPanel.Q<Button>(Names.GithubConfigArrowButton));

            _subPanel.Q<Button>(Names.RepoButton).clicked += () => Application.OpenURL(config.RepositoryLink);
            _subPanel.Q<Button>(Names.IssuesButton).clicked += () => Application.OpenURL($"{config.RepositoryLink}/issues");
            _subPanel.Q<Button>(Names.PRButton).clicked += () => Application.OpenURL($"{config.RepositoryLink}/pulls");

            var scenePathTextField = _subPanel.Q<TextField>(Names.ScenePathTextField);
            var scenePathBrowseButton = _subPanel.Q<Button>(Names.ScenePathBrowsBtn);
            var scenesContainer = _subPanel.Q<VisualElement>(Names.SceneNav);

            scenePathTextField.RegisterCallback<BlurEvent>(_ =>
            {
                _scenesWatcher = DisposeWatcher(_scenesWatcher);
                _scenesWatcher = CreateFileWatcher(config.ScenesPath, "*.unity", () =>
                {
                    EditorApplication.delayCall += () =>
                    {
                        _scenes.Clear();
                        ReloadSceneCache(config.ScenesPath);
                        RefreshButtonList(scenesContainer, ButtonTemplate, _scenes, SetupSceneButton);
                        CheckIfButtonListEmptyAndShowEmptyText(scenesContainer, _subPanel.Q<VisualElement>(Names.NoScenes));
                    };
                });
                _scenes.Clear();
                ReloadSceneCache(config.ScenesPath);
                RefreshButtonList(scenesContainer, ButtonTemplate, _scenes, SetupSceneButton);
                CheckIfButtonListEmptyAndShowEmptyText(scenesContainer, _subPanel.Q<VisualElement>(Names.NoScenes));
            });

            scenePathBrowseButton.clicked += () =>
            {
                string path = EditorUtility.OpenFolderPanel("Select Scenes Directory",
                    Path.GetDirectoryName(AssetDatabase.GetAssetPath(config)), "Scenes");
                if (!string.IsNullOrEmpty(path))
                {
                    scenePathTextField.value = path;

                    EditorApplication.delayCall += () =>
                    {
                        _scenes.Clear();
                        ReloadSceneCache(config.ScenesPath);
                        RefreshButtonList(scenesContainer, ButtonTemplate, _scenes, SetupSceneButton);
                        CheckIfButtonListEmptyAndShowEmptyText(scenesContainer, _subPanel.Q<VisualElement>(Names.NoScenes));
                    };
                }
            };

            ShowContainer(_showGithubConfig, ref _showGithubConfig,
                    _subPanel.Q<VisualElement>(Names.GithubConfigContainer),
                    _subPanel.Q<Button>(Names.GithubConfigArrowButton));
        }

        private void SetupShortcuts()
        {
            var config = ModToolsConfig.Config;
            _subPanel.Q<VisualElement>(Names.PanelShortcut).Bind(new SerializedObject(config));
            var scenesContainer = _subPanel.Q<VisualElement>(Names.SceneNav);
            var asmDefContainer = _subPanel.Q<VisualElement>(Names.AsmDefNav);

            _scenesWatcher = DisposeWatcher(_scenesWatcher);
            _scenesWatcher = CreateFileWatcher(config.ScenesPath, "*.unity", () =>
            {
                EditorApplication.delayCall += () =>
                {
                    _scenes.Clear();
                    ReloadSceneCache(config.ScenesPath);
                    RefreshButtonList(scenesContainer, ButtonTemplate, _scenes, SetupSceneButton);
                    CheckIfButtonListEmptyAndShowEmptyText(scenesContainer, _subPanel.Q<VisualElement>(Names.NoScenes));
                };
            });

            _assemblyDefinitionsWatcher = DisposeWatcher(_assemblyDefinitionsWatcher);
            _assemblyDefinitionsWatcher = CreateFileWatcher(Application.dataPath, "*.asmdef", () =>
            {
                EditorApplication.delayCall += () =>
                {
                    _asmDefs.Clear();
                    _asmDefRelativePaths.Clear();
                    ReloadAsmDefCache();
                    RefreshButtonList(asmDefContainer, ButtonTemplate, _asmDefs, SetupAsmDefButton);
                    CheckIfButtonListEmptyAndShowEmptyText(asmDefContainer, _subPanel.Q<VisualElement>(Names.NoAsmdefs));
                };
            });

            _scenes.Clear();
            _asmDefs.Clear();
            _asmDefRelativePaths.Clear();

            ReloadSceneCache(config.ScenesPath);
            ReloadAsmDefCache();
            RefreshButtonList(scenesContainer, ButtonTemplate, _scenes, SetupSceneButton);
            RefreshButtonList(asmDefContainer, ButtonTemplate, _asmDefs, SetupAsmDefButton);
            CheckIfButtonListEmptyAndShowEmptyText(scenesContainer, _subPanel.Q<VisualElement>(Names.NoScenes));
            CheckIfButtonListEmptyAndShowEmptyText(asmDefContainer, _subPanel.Q<VisualElement>(Names.NoAsmdefs));
        }

        // -----------------------------------------------------------------------
        // File Cache Helpers
        // -----------------------------------------------------------------------

        private void ReloadSceneCache(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            foreach (var file in Directory.GetFiles(path, "*.unity", SearchOption.AllDirectories))
                _scenes.Add(Path.GetFullPath(file));
        }

        private void ReloadAsmDefCache()
        {
            foreach (var file in Directory.GetFiles(Application.dataPath, "*.asmdef", SearchOption.AllDirectories))
            {
                string full = Path.GetFullPath(file);
                if (_asmDefs.Add(full))
                    _asmDefRelativePaths[full] = ToRelativePath(full);
            }
        }

        private static string ToRelativePath(string fullPath)
        {
            string dataPath = Path.GetFullPath(Application.dataPath);
            return fullPath.StartsWith(dataPath)
                ? "Assets" + fullPath[dataPath.Length..].Replace('\\', '/')
                : null;
        }

        // -----------------------------------------------------------------------
        // Button List Builder
        // -----------------------------------------------------------------------

        private static void CheckIfButtonListEmptyAndShowEmptyText(VisualElement container, VisualElement text)
        {
            text.style.display = container.childCount != 0 ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private static void RefreshButtonList(VisualElement container, VisualTreeAsset template,
            HashSet<string> files, Action<Button, string> setup)
        {
            foreach (var child in container.Children().Where(c => c.name != "TemplateNamePlaceholder").ToList())
                child.RemoveFromHierarchy();

            foreach (var file in files)
            {
                var btn = (Button)template.CloneTree().ElementAt(0);
                btn.text = Path.GetFileNameWithoutExtension(file);
                setup(btn, file);
                container.Add(btn);
            }
        }

        private static void SetupSceneButton(Button btn, string file)
        {
            btn.clicked += () =>
            {
                if (!TryPingAsset(file)) return;
                SwitchToScene(Path.GetFullPath(file));
            };
        }

        private static void SetupAsmDefButton(Button btn, string file)
        {
            btn.clicked += () => TryPingAsset(file);
        }

        private static bool TryPingAsset(string file)
        {
            string relativePath = ToRelativePath(Path.GetFullPath(file));
            if (relativePath == null)
            {
                Debug.LogWarning($"File is outside the Assets folder and cannot be loaded: {file}");
                return false;
            }

            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(relativePath);
            if (asset == null)
            {
                Debug.LogWarning($"Could not load asset at: {relativePath}");
                return false;
            }

            EditorGUIUtility.PingObject(asset);
            Selection.activeObject = asset;
            return true;
        }

        // -----------------------------------------------------------------------
        // File System Watchers
        // -----------------------------------------------------------------------

        private static FileSystemWatcher CreateFileWatcher(string path, string filter, System.Action onChange)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            var watcher = new FileSystemWatcher(path)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName
                             | NotifyFilters.LastWrite | NotifyFilters.Size,
                Filter = filter,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
            };

            FileSystemEventHandler handler = (_, _) => onChange();
            watcher.Created += handler;
            watcher.Deleted += handler;
            watcher.Changed += handler;
            watcher.Renamed += (_, _) => onChange();
            return watcher;
        }

        public void ChangeWatchedFolder(string path, Label label)
        {
            _buildDirectoryWatcher = DisposeWatcher(_buildDirectoryWatcher);
            _buildDirectoryWatcher = CreateFileWatcher(path, "*",
                () => EditorApplication.delayCall += () => UpdateBuildStats(label));
        }

        private static FileSystemWatcher DisposeWatcher(FileSystemWatcher watcher)
        {
            if (watcher == null) return null;
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            return null;
        }

        // -----------------------------------------------------------------------
        // Build Stats
        // -----------------------------------------------------------------------

        private static void UpdateBuildStats(Label label)
        {
            var config = ModToolsConfig.Config;
            if (label == null) return;

            string path = Path.Combine(Directory.GetCurrentDirectory(), "Builds");

            if (!Directory.Exists(path))
            {
                label.text = "Cached Builds: 0 (0 B)";
                return;
            }

            try
            {
                var di = new DirectoryInfo(path);
                long totalBytes = di.EnumerateFiles("*", SearchOption.AllDirectories).Sum(f => f.Length);
                label.text = $"Cached Builds: {di.GetDirectories().Length} ({FormatBytes(totalBytes)})";
            }
            catch (IOException) { }
        }

        private static string FormatBytes(long bytes) => bytes switch
        {
            < 1024 => $"{bytes} B",
            < 1024 * 1024 => $"{bytes / 1024.0:0.##} KB",
            < 1024 * 1024 * 1024L => $"{bytes / (1024.0 * 1024.0):0.##} MB",
            _ => $"{bytes / (1024.0 * 1024.0 * 1024.0):0.##} GB",
        };

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private static void SwitchToScene(string path)
        {
            if (Application.isPlaying)
            {
                string name = path.Contains('/') ? path.Split('/').Last() : path;
                SceneManager.LoadScene(name);
            }
            else if (!SceneManager.GetActiveScene().name.Contains(path))
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                EditorSceneManager.OpenScene(path);
            }
        }

        private void ShowContainer(bool show, ref bool valueHolder, VisualElement container, Button arrow)
        {
            valueHolder = show;
            arrow.style.rotate = show
                ? new Rotate(new Angle(0, AngleUnit.Degree))
                : new Rotate(new Angle(-90, AngleUnit.Degree));
            container.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}