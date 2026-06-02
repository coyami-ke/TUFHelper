//Original Code from https://github.com/ADOFAI-gg/ADOFAI-Modding-Toolkit
using ADOFAIModdingHelper.Common;
using ADOFAIModdingHelper.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ADOFAIModdingHelper.ScriptableObjects
{
    [Serializable]
    public class RawFileCopyEntry
    {
        /// <summary>
        /// Destination sub-folder relative to the mod root.
        /// Leave empty to copy directly into the root.
        /// Forward slashes are supported: "win/helpers", "mac/libs", etc.
        /// </summary>
        public string DestinationPath = string.Empty;

        /// <summary>
        /// Assets (any type) to copy into <see cref="DestinationPath"/>.
        /// </summary>
        public List<UnityEngine.Object> Files = new();
    }

    public class ModToolsConfig : ScriptableObject
    {
        private static ModToolsConfig _config;

        public static ModToolsConfig Config
        {
            get
            {
                if (_config) return _config;
                _config = AssetDatabase.LoadAssetAtPath<ModToolsConfig>(Constants.settingsFolder + "/ModConfig.asset");
                if (_config) return _config;

                _config = CreateInstance<ModToolsConfig>();
                if (!Directory.Exists(Constants.settingsFolder))
                    Directory.CreateDirectory(Constants.settingsFolder);
                AssetDatabase.CreateAsset(_config, Constants.settingsFolder + "/ModConfig.asset");
                return _config;
            }
        }

        public string BuildDirectory;

        public bool openModInfoFoldout;

        public bool skipAssetBundleBuild;

        public bool createZip;

        public bool developmentBuild = true;
        public bool generateDebugSymbols;

        public bool buildEveryPlatform;
        public BuildTarget[] serializedBuildPlatforms;

        public bool copyToDirectory;

        public bool runApplication;
        public bool runApplicationThroughSteam;

        public int deleteBuildsExceptLastN;
        public bool automaticallyDeleteBuilds;

        public List<AssemblyDefinitionAsset> AssemblyDefinitions;
        public List<string> PrecompAssemblies;
        public List<string> AssetBundles;

        /// <summary>
        /// Files to copy verbatim into the build, organised by destination sub-folder.
        /// </summary>
        public List<RawFileCopyEntry> RawFileCopies = new();

        public string RepositoryLink;
        public string IssuesLink;
        public string PullRequestsLink;

        public string ScenesPath;

        private HashSet<BuildTarget> _buildTargets;
        public HashSet<BuildTarget> BuildPlatforms
        {
            get
            {
                if (_buildTargets == null)
                    return _buildTargets ??= serializedBuildPlatforms?.ToHashSet() ?? new();

                return _buildTargets;
            }
            set
            {
                _buildTargets = value;
                serializedBuildPlatforms = value.ToArray();
            }
        }

        public readonly ModBuilder ModBuilder = new();

        public void BuildMod(string copyDestination)
        {
            ModBuilder.SkipAssetBundleBuild = skipAssetBundleBuild;
            ModBuilder.DevelopmentBuild = developmentBuild;
            ModBuilder.GenerateDebugSymbols = generateDebugSymbols;

            ModBuilder.AssemblyDefinitions = AssemblyDefinitions;
            ModBuilder.AssetBundles = RemoveNoneFromList(AssetBundles);
            ModBuilder.PrecompAssemblies = RemoveNoneFromList(PrecompAssemblies);

            // Resolve raw-file-copy asset paths now, on the main thread, so ModBuilder
            // can do pure File I/O without touching the Unity Asset Database.
            var resolvedFileCopies = new Dictionary<string, List<string>>();
            foreach (var entry in RawFileCopies ?? new List<RawFileCopyEntry>())
            {
                if (entry?.Files == null || entry.Files.Count == 0) continue;
                var paths = entry.Files
                    .Where(f => f != null)
                    .Select(f => System.IO.Path.GetFullPath(AssetDatabase.GetAssetPath(f)))
                    .Where(System.IO.File.Exists)
                    .ToList();
                if (paths.Count > 0)
                    resolvedFileCopies[entry.DestinationPath ?? string.Empty] = paths;
            }
            ModBuilder.RawFileCopies = resolvedFileCopies;

            ModBuilder.Build(copyDestination, buildEveryPlatform, BuildPlatforms)
                .ContinueWith(task =>
                {
                    if (createZip)
                    {
                        using var stream =
                            new FileStream(Path.Combine(Path.GetDirectoryName(task.Result)!, string.IsNullOrWhiteSpace(ModInfo.Info.Id) ? "Null" : ModInfo.Info.Id + ".zip"),
                                FileMode.Create);
                        using var archive = new ZipArchive(stream, ZipArchiveMode.Create);

                        foreach (var file in Directory.GetFiles(task.Result, "*", SearchOption.AllDirectories))
                        {
                            archive.CreateEntryFromFile(file,
                                Path.Combine(ModInfo.Info.Id, Path.GetRelativePath(task.Result, file)));
                        }
                    }

                    if (automaticallyDeleteBuilds)
                        DeleteBuilds(1);

                    RunApp();
                });
        }

        private List<string> RemoveNoneFromList(List<string> list)
        {
            var newlist = new List<string>(list);
            newlist.RemoveAll(item => item == "None");
            return newlist;
        }
        public void RunApp(bool FRun = false)
        {
            if (runApplication || FRun)
            {
                if (runApplicationThroughSteam)
                {
                    Process.Start("steam://rungameid/977950");
                }
                else
                {
                    string exePath = Setting.Config.ADOFAIPath;

                    if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
                    {
                        UnityEngine.Debug.LogError("ADOFAI Path is invalid or executable does not exist!");
                        return;
                    }

                    string gameDirectory = Path.GetDirectoryName(exePath);

                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = exePath,
                        WorkingDirectory = gameDirectory,
                        UseShellExecute = true
                    };

                    Process.Start(startInfo);
                }
            }
        }
        public void DeleteBuilds(int? saveLeast = null)
        {
            var buildDir = Path.Combine(Directory.GetCurrentDirectory(), "Builds");

            if (Directory.Exists(buildDir))
            {
                var except = deleteBuildsExceptLastN;

                if (except == 0)
                {
                    Directory.Delete(buildDir, true);
                    Directory.CreateDirectory(buildDir);

                    var zipPath = Path.Combine(buildDir, string.IsNullOrWhiteSpace(ModInfo.Info.Id) ? "Null" : ModInfo.Info.Id + ".zip");

                    if (File.Exists(zipPath))
                        File.Delete(zipPath);
                }
                else
                {
                    var buildDirectories = new DirectoryInfo(buildDir).GetDirectories()
                        .OrderByDescending(d => d.CreationTimeUtc)
                        .ToList();

                    for (var i = Math.Max(saveLeast ?? 0, Math.Max(0, except)); i < buildDirectories.Count; i++)
                    {
                        buildDirectories[i].Delete(true);
                    }
                }
            }
        }

        public void ApplyPreset(string preset)
        {
            switch (preset)
            {
                case "Debug":
                    copyToDirectory = true;
                    buildEveryPlatform = false;
                    developmentBuild = true;
                    generateDebugSymbols = true;
                    createZip = false;
                    runApplication = true;
                    break;
                case "Release":
                    buildEveryPlatform = true;
                    developmentBuild = false;
                    generateDebugSymbols = false;
                    createZip = true;
                    break;
                case "Clear":
                    skipAssetBundleBuild = false;
                    buildEveryPlatform = false;
                    developmentBuild = false;
                    generateDebugSymbols = false;
                    copyToDirectory = false;
                    createZip = false;
                    runApplication = false;
                    runApplicationThroughSteam = false;
                    break;
            }
        }
    }
}