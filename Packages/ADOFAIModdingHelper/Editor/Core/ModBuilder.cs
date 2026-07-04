//Original Code from https://github.com/ADOFAI-gg/ADOFAI-Modding-Toolkit
using ADOFAIModdingHelper.ScriptableObjects;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditorInternal;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ADOFAIModdingHelper.Core
{
    public class ModBuilder
    {
        public bool SkipAssetBundleBuild;
        public bool DevelopmentBuild;
        public bool GenerateDebugSymbols;
        public bool SplitBuild;

        public List<AssemblyDefinitionAsset> AssemblyDefinitions;
        public List<string> PrecompAssemblies;
        public List<string> AssetBundles;

        /// <summary>
        /// Used when SplitBuild is FALSE.
        /// </summary>
        public Dictionary<string, List<string>> RawFileCopies = new Dictionary<string, List<string>>();

        /// <summary>
        /// Used when SplitBuild is TRUE to separate verbatim file targets.
        /// </summary>
        public Dictionary<BuildTarget, Dictionary<string, List<string>>> PlatformRawFileCopies = new();

        private List<string> _defines;
        private string _baseBuildPath;

        public bool IsBuilding { get; private set; }

        public async Task<string> Build([CanBeNull] string copyDestination, bool allPlatforms, [CanBeNull] HashSet<BuildTarget> buildTargets)
        {
            try
            {
                IsBuilding = true;
                _defines = new List<string>();
                AssetDatabase.SaveAssets();
                var now = DateTime.Now - DateTime.UnixEpoch;
                _baseBuildPath = Path.Combine("Builds", $"{Math.Round(now.TotalMilliseconds)}");
                Directory.CreateDirectory(_baseBuildPath);

                if (DevelopmentBuild) _defines.Add("DEBUG");

                Debug.Log($"extra defines: {string.Join(", ", _defines)}");

                if (SplitBuild)
                {
                    foreach (var target in buildTargets)
                    {
                        string platformPath = Path.Combine(_baseBuildPath, GetPlatformFolderCode(target));
                        Directory.CreateDirectory(platformPath);

                        ModInfo.Info.WriteToFile(Path.Combine(platformPath, "Info.json"));
                        await BuildAssembliesToPath(platformPath);
                    }
                }
                else
                {
                    ModInfo.Info.WriteToFile(Path.Combine(_baseBuildPath, "Info.json"));
                    await BuildAssembliesToPath(_baseBuildPath);
                }

                foreach (var target in buildTargets)
                {
                    BuildAssetBundlesForPlatform(target);
                }

                CopyRawFiles(buildTargets);

                bool createZipConfig = ModToolsConfig.Config.createZip;
                string modId = string.IsNullOrWhiteSpace(ModInfo.Info.Id) ? "Null" : ModInfo.Info.Id;
                string modVersion = string.IsNullOrWhiteSpace(ModInfo.Info.Version) ? "Null" : ModInfo.Info.Version;

                if (createZipConfig)
                {
                    if (SplitBuild)
                    {
                        foreach (var target in buildTargets)
                        {
                            string code = GetPlatformFolderCode(target);
                            string sourceDir = Path.Combine(_baseBuildPath, code);
                            string zipPath = Path.Combine("Builds", $"{modId}_{modVersion}_{code}.zip");

                            CreateZipFromDirectory(sourceDir, zipPath, modId);
                        }
                    }
                    else
                    {
                        string zipPath = Path.Combine("Builds", $"{modId}_{modVersion}.zip");
                        CreateZipFromDirectory(_baseBuildPath, zipPath, modId);
                    }
                }

                if (copyDestination != null)
                {
                    if (Directory.Exists(copyDestination))
                        Directory.Delete(copyDestination, true);

                    if (SplitBuild)
                    {
                        BuildTarget devPlatform = PlatformToBuildTarget(Application.platform);

                        // Default to Windows if the platform is unidentified/NoTarget
                        if (devPlatform == BuildTarget.NoTarget)
                        {
                            devPlatform = BuildTarget.StandaloneWindows64;
                        }

                        string devFolderCode = GetPlatformFolderCode(devPlatform);
                        string platformSpecificSource = Path.Combine(_baseBuildPath, devFolderCode);

                        if (!Directory.Exists(platformSpecificSource))
                        {
                            string fallbackCode = buildTargets.Count > 0
                                ? GetPlatformFolderCode(buildTargets.First())
                                : "win";
                            platformSpecificSource = Path.Combine(_baseBuildPath, fallbackCode);
                        }

                        if (Directory.Exists(platformSpecificSource))
                        {
                            Directory.CreateDirectory(copyDestination);
                            CopyDirectory(platformSpecificSource, copyDestination);
                            Debug.Log($"[SplitBuild Dev Copy] Copied local developer platform ({devFolderCode}) files directly to game path.");
                        }
                    }
                    else
                    {
                        FileUtil.CopyFileOrDirectory(_baseBuildPath, copyDestination);
                    }
                }

                return _baseBuildPath;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
            finally
            {
                IsBuilding = false;
            }
        }

        public static void Copy(string sourceDirectory, string targetDirectory)
        {
            var diSource = new DirectoryInfo(sourceDirectory);
            var diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            foreach (var fi in source.GetFiles())
            {
                if (fi.Extension.Equals(".meta", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            foreach (var diSourceSubDir in source.GetDirectories())
            {
                var nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        private async Task BuildAssembliesToPath(string targetPath)
        {
            var cleanPrecompNames = PrecompAssemblies.Select(Path.GetFileNameWithoutExtension).ToList();
            var names = cleanPrecompNames.Concat(AssemblyDefinitions.Select(x => x.name)).ToList();
            var namesSuffixed = names.Select(x => x + ".dll").ToList();

            var assemblies = CompilationPipeline.GetAssemblies(AssembliesType.PlayerWithoutTestAssemblies)
                .Where(x => names.Contains(x.name)).ToArray();

            var prebuilts = CompilationPipeline.GetPrecompiledAssemblyNames()
                .Where(x => namesSuffixed.Contains(x))
                .Select(CompilationPipeline.GetPrecompiledAssemblyPathFromAssemblyName)
                .ToList();

            var copiedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var prebuilt in prebuilts)
            {
                var name = Path.GetFileName(prebuilt);
                File.Copy(prebuilt, Path.Combine(targetPath, name), overwrite: true);
                copiedFiles.Add(name);
            }

            string internalRefPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "UnityReferenceAssemblies");

            foreach (var targetDllName in namesSuffixed)
            {
                if (copiedFiles.Contains(targetDllName)) continue;

                if (Directory.Exists(internalRefPath))
                {
                    var foundInternal = Directory.GetFiles(internalRefPath, targetDllName, SearchOption.AllDirectories);
                    if (foundInternal.Length > 0)
                    {
                        File.Copy(foundInternal[0], Path.Combine(targetPath, targetDllName), overwrite: true);
                        copiedFiles.Add(targetDllName);
                        continue;
                    }
                }

                var foundLocal = Directory.GetFiles(Application.dataPath, targetDllName, SearchOption.AllDirectories);
                if (foundLocal.Length > 0)
                {
                    File.Copy(foundLocal[0], Path.Combine(targetPath, targetDllName), overwrite: true);
                    copiedFiles.Add(targetDllName);
                }
            }

            try
            {
                EditorUtility.DisplayProgressBar("building assemblies", $"Building {assemblies.Length} assemblies", 1);
                foreach (var assembly in assemblies)
                {
                    await BuildAssembly(assembly, targetPath);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private Task BuildAssembly(Assembly assembly, string targetPath)
        {
            return Task.Run(() =>
            {
                var trees = new List<SyntaxTree>();
                var defines = assembly.defines.Concat(_defines).ToList();
                var parseOptions = new CSharpParseOptions(preprocessorSymbols: defines);

                foreach (var scriptPath in assembly.sourceFiles)
                {
                    var txt = File.ReadAllText(scriptPath);
                    var tree = CSharpSyntaxTree.ParseText(txt, parseOptions, scriptPath, Encoding.UTF8);
                    trees.Add(tree);
                }

                var references = assembly.allReferences.Select(location => MetadataReference.CreateFromFile(location)).ToList();
                var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release)
                    .WithAllowUnsafe(assembly.compilerOptions.AllowUnsafeCode)
                    .WithPlatform(Platform.AnyCpu);

                var compilation = CSharpCompilation.Create(assembly.name, trees, references, options);

                using var dllStream = File.Create(Path.Combine(targetPath, assembly.name + ".dll"));
                var pdbPath = Path.Combine(targetPath, assembly.name + ".pdb");
                using var pdbStream = GenerateDebugSymbols ? File.Create(pdbPath) : null;

                var result = compilation.Emit(dllStream, pdbStream: pdbStream,
                    options: new EmitOptions(pdbFilePath: GenerateDebugSymbols ? pdbPath + '\0' : null,
                        debugInformationFormat: DebugInformationFormat.PortablePdb));

                if (!result.Success)
                {
                    foreach (var diag in result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
                    {
                        Debug.LogError(diag.ToString());
                    }
                    throw new Exception($"Compilation failed for assembly: {assembly.name}");
                }
            });
        }
        private void CreateZipFromDirectory(string sourceDir, string zipDestinationPath, string archiveInternalRoot)
        {
            if (!Directory.Exists(sourceDir)) return;

            using var stream = new FileStream(zipDestinationPath, FileMode.Create);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Create);

            var files = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                // Ensure zip path names don't compress the parent zip path context accidentally
                string relativePath = Path.GetRelativePath(sourceDir, file);
                string entryName = Path.Combine(archiveInternalRoot, relativePath).Replace(Path.DirectorySeparatorChar, '/');
                archive.CreateEntryFromFile(file, entryName);
            }
        }

        private void CopyRawFiles(HashSet<BuildTarget> platforms)
        {
            if (SplitBuild)
            {
                foreach (var target in platforms)
                {
                    if (!PlatformRawFileCopies.TryGetValue(target, out var rawCopies) || rawCopies.Count == 0) continue;
                    string platformRoot = Path.Combine(_baseBuildPath, GetPlatformFolderCode(target));
                    ExecuteFilesCopy(platformRoot, rawCopies);
                }
            }
            else
            {
                ExecuteFilesCopy(_baseBuildPath, RawFileCopies);
            }
        }

        private void ExecuteFilesCopy(string destinationRoot, Dictionary<string, List<string>> fileCopies)
        {
            if (fileCopies == null || fileCopies.Count == 0) return;

            foreach (var (destSubPath, filePaths) in fileCopies)
            {
                var destDir = string.IsNullOrWhiteSpace(destSubPath)
                    ? destinationRoot
                    : Path.Combine(destinationRoot, destSubPath.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar));

                if (!Directory.Exists(destDir))
                    Directory.CreateDirectory(destDir);

                foreach (var srcPath in filePaths)
                {
                    if (Directory.Exists(srcPath))
                    {
                        var folderName = new DirectoryInfo(srcPath).Name;
                        var targetFolderDestination = Path.Combine(destDir, folderName);
                        CopyDirectory(srcPath, targetFolderDestination);
                        continue;
                    }

                    if (!File.Exists(srcPath)) continue;

                    var destFilePath = Path.Combine(destDir, Path.GetFileName(srcPath));
                    File.Copy(srcPath, destFilePath, overwrite: true);
                }
            }
        }

        public static void CopyDirectory(string sourceDirectory, string targetDirectory)
        {
            Directory.CreateDirectory(targetDirectory);
            var diSource = new DirectoryInfo(sourceDirectory);

            foreach (var fi in diSource.GetFiles())
            {
                if (fi.Extension.Equals(".meta", StringComparison.OrdinalIgnoreCase)) continue;
                fi.CopyTo(Path.Combine(targetDirectory, fi.Name), true);
            }

            foreach (var diSourceSubDir in diSource.GetDirectories())
            {
                CopyDirectory(diSourceSubDir.FullName, Path.Combine(targetDirectory, diSourceSubDir.Name));
            }
        }

        private void BuildAssetBundlesForPlatform(BuildTarget target)
        {
            string ns = GetPlatformFolderCode(target);
            var workingBuildPath = Path.Combine("Temp", "Build", "AssetBundles", ns);

            // Destination drops assets directly in root if split build is active
            string targetDestinationDir = SplitBuild
                ? Path.Combine(_baseBuildPath, ns, ns)
                : Path.Combine(_baseBuildPath, ns);

            if (!Directory.Exists(workingBuildPath))
                Directory.CreateDirectory(workingBuildPath);

            if (!Directory.Exists(targetDestinationDir))
                Directory.CreateDirectory(targetDestinationDir);

            if (!SkipAssetBundleBuild || !Directory.Exists(targetDestinationDir))
                BuildPipeline.BuildAssetBundles(workingBuildPath, BuildAssetBundleOptions.None, target);

            foreach (var file in AssetBundles)
            {
                var source = Path.Combine(workingBuildPath, file);
                if (!File.Exists(source)) continue;

                var destination = Path.Combine(targetDestinationDir, file);
                File.Copy(source, destination, true);
            }
        }
        public static string GetPlatformFolderCode(BuildTarget target) => target switch
        {
            BuildTarget.StandaloneWindows64 => "win",
            BuildTarget.StandaloneLinux64 => "linux",
            BuildTarget.StandaloneOSX => "mac",
            _ => "universal"
        };

        public static BuildTarget PlatformToBuildTarget(RuntimePlatform runtimePlatform) =>
        runtimePlatform switch
        {
            RuntimePlatform.WindowsEditor => BuildTarget.StandaloneWindows64,
            RuntimePlatform.OSXEditor => BuildTarget.StandaloneOSX,
            RuntimePlatform.LinuxEditor => BuildTarget.StandaloneLinux64,
            _ => BuildTarget.NoTarget
        };
    }
}