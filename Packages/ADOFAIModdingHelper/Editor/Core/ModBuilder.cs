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

        public List<AssemblyDefinitionAsset> AssemblyDefinitions;
        public List<string> PrecompAssemblies;
        public List<string> AssetBundles;

        /// <summary>
        /// Resolved (full disk path) files to copy verbatim.
        /// Key = destination sub-folder relative to mod root (empty = root).
        /// Value = list of absolute source file paths.
        /// </summary>
        public Dictionary<string, List<string>> RawFileCopies = new();

        private List<string> _defines;
        private string _buildPath;

        public bool IsBuilding { get; private set; }

        public async Task<string> Build([CanBeNull] string copyDestination, bool allPlatforms, [CanBeNull] HashSet<BuildTarget> buildTargets)
        {
            try
            {
                IsBuilding = true;
                _defines = new List<string>();
                AssetDatabase.SaveAssets();
                var now = DateTime.Now - DateTime.UnixEpoch;
                _buildPath = Path.Combine("Builds", $"{Math.Round(now.TotalMilliseconds)}");
                Directory.CreateDirectory(_buildPath);

                if (DevelopmentBuild) _defines.Add("DEBUG");

                Debug.Log($"extra defines: {string.Join(", ", _defines)}");

                WriteModInfo();
                await BuildAssemblies();
                if (allPlatforms)
                {
                    BuildAllAssetBundles();
                }
                else if (buildTargets == null || buildTargets.Count == 0)
                {
                    BuildAssetBundlesForCurrentPlatform();
                }
                else
                {
                    BuildAssetBundles(buildTargets);
                }

                CopyRawFiles();

                if (copyDestination != null)
                {
                    if (Directory.Exists(copyDestination))
                        Directory.Delete(copyDestination, true);

                    FileUtil.CopyFileOrDirectory(_buildPath, copyDestination);
                }

                return _buildPath;
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

        private async Task BuildAssemblies()
        {
            var cleanPrecompNames = PrecompAssemblies
                .Select(x => Path.GetFileNameWithoutExtension(x))
                .ToList();

            var names = cleanPrecompNames
                .Concat(AssemblyDefinitions.Select(x => x.name))
                .ToList();

            var namesSuffixed = names.Select(x => x + ".dll").ToList();

            var assemblies = CompilationPipeline.GetAssemblies(AssembliesType.PlayerWithoutTestAssemblies)
                .Where(x => names.Contains(x.name)).ToArray();

            // Find prebuilts recognized natively by Unity's pipeline
            var prebuilts = CompilationPipeline.GetPrecompiledAssemblyNames()
                .Where(x => namesSuffixed.Contains(x))
                .Select(CompilationPipeline.GetPrecompiledAssemblyPathFromAssemblyName)
                .ToList();

            // Track files we successfully resolve so we don't look for duplicates
            var copiedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var prebuilt in prebuilts)
            {
                var name = Path.GetFileName(prebuilt);
                File.Copy(prebuilt, Path.Combine(_buildPath, name), overwrite: true);
                copiedFiles.Add(name);
            }

            // Fallback scanner for stubborn system facade DLLs (like System.Memory.dll) 
            // that live in Unity's internal Reference Directories rather than local Assets.
            string internalReferenceAssembliesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "UnityReferenceAssemblies");

            foreach (var targetDllName in namesSuffixed)
            {
                if (copiedFiles.Contains(targetDllName)) continue;

                // 1. Try to search Unity's system reference directory first
                if (Directory.Exists(internalReferenceAssembliesPath))
                {
                    var foundInternalPaths = Directory.GetFiles(internalReferenceAssembliesPath, targetDllName, SearchOption.AllDirectories);
                    if (foundInternalPaths.Length > 0)
                    {
                        File.Copy(foundInternalPaths[0], Path.Combine(_buildPath, targetDllName), overwrite: true);
                        copiedFiles.Add(targetDllName);
                        continue;
                    }
                }

                // 2. Try to search your project asset directory if it wasn't a framework facade assembly
                var foundLocalPaths = Directory.GetFiles(Application.dataPath, targetDllName, SearchOption.AllDirectories);
                if (foundLocalPaths.Length > 0)
                {
                    File.Copy(foundLocalPaths[0], Path.Combine(_buildPath, targetDllName), overwrite: true);
                    copiedFiles.Add(targetDllName);
                }
            }

            try
            {
                EditorUtility.DisplayProgressBar("building assemblies", $"Building {assemblies.Length} assemblies", 1);

                foreach (var assembly in assemblies)
                {
                    await BuildAssembly(assembly);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
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

            // Copy each file into the new directory.
            foreach (var fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (var diSourceSubDir in source.GetDirectories())
            {
                var nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        private Task BuildAssembly(Assembly assembly)
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

                var references = assembly.allReferences.Select(location => MetadataReference.CreateFromFile(location))
                    .ToList();

                var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                        optimizationLevel: OptimizationLevel.Release)
                    .WithAllowUnsafe(assembly.compilerOptions.AllowUnsafeCode)
                    .WithPlatform(Platform.AnyCpu);

                var compilation = CSharpCompilation.Create(assembly.name, trees, references, options);
                var resources = GetEmbeddedResources(assembly.name);

                if (!Directory.Exists(_buildPath)) Directory.CreateDirectory(_buildPath);

                using var dllStream = File.Create(Path.Combine(_buildPath, assembly.name + ".dll"));

                var pdbPath = Path.Combine(_buildPath, assembly.name + ".pdb");
                using var pdbStream = GenerateDebugSymbols ? File.Create(pdbPath) : null;

                var result = compilation.Emit(dllStream, pdbStream: pdbStream,
                    manifestResources: resources,
                    options: new EmitOptions(pdbFilePath: GenerateDebugSymbols ? pdbPath + '\0' : null,
                        debugInformationFormat: DebugInformationFormat.PortablePdb));

                foreach (var resultDiagnostic in result.Diagnostics)
                {
                    switch (resultDiagnostic.Severity)
                    {
                        case DiagnosticSeverity.Error:
                            Debug.LogError(resultDiagnostic.ToString());
                            break;
                        case DiagnosticSeverity.Hidden:
                        case DiagnosticSeverity.Info:
                        default:
                            break;
                    }
                }

                if (!result.Success)
                {
                    throw new Exception("compilation failed");
                }
            });
        }

        private static IEnumerable<ResourceDescription> GetEmbeddedResources(string assemblyName)
        {
            var embeddedDirectory = Path.Combine(Application.dataPath, assemblyName, "Assets", "Embedded");
            if (!Directory.Exists(embeddedDirectory))
            {
                return Enumerable.Empty<ResourceDescription>();
            }

            return Directory.GetFiles(embeddedDirectory)
                .Where(path => !path.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                .Select(path =>
                {
                    var resourceName = $"{assemblyName}.{Path.GetFileName(path)}";
                    return new ResourceDescription(resourceName, () => File.OpenRead(path), isPublic: true);
                })
                .ToArray();
        }

        /// <summary>
        /// Copies raw files verbatim into the build output, respecting their designated sub-folders.
        /// Operates on pre-resolved disk paths so it is safe to call from any thread.
        /// </summary>
        private void CopyRawFiles()
        {
            if (RawFileCopies == null || RawFileCopies.Count == 0) return;

            foreach (var (destSubPath, filePaths) in RawFileCopies)
            {
                // Resolve destination directory: empty key means mod root.
                var destDir = string.IsNullOrWhiteSpace(destSubPath)
                    ? _buildPath
                    : Path.Combine(
                        _buildPath,
                        destSubPath
                            .Replace('/', Path.DirectorySeparatorChar)
                            .TrimStart(Path.DirectorySeparatorChar));

                if (!Directory.Exists(destDir))
                    Directory.CreateDirectory(destDir);

                foreach (var srcPath in filePaths)
                {
                    if (!File.Exists(srcPath))
                    {
                        Debug.LogWarning($"[RawFileCopy] Source not found, skipping: {srcPath}");
                        continue;
                    }

                    var destFilePath = Path.Combine(destDir, Path.GetFileName(srcPath));
                    File.Copy(srcPath, destFilePath, overwrite: true);
                    Debug.Log($"[RawFileCopy] {srcPath} → {destFilePath}");
                }
            }
        }

        private void WriteModInfo() => ModInfo.Info.WriteToFile(Path.Combine(_buildPath, "Info.json"));

        private void BuildAssetBundlesForCurrentPlatform()
        {
            BuildAssetBundlesForPlatform(PlatformToBuildTarget(Application.platform));
        }

        private void BuildAssetBundles(HashSet<BuildTarget> buildTargets)
        {
            foreach (var target in buildTargets)
                BuildAssetBundlesForPlatform(target);
        }

        private void BuildAllAssetBundles()
        {
            BuildAssetBundlesForPlatform(BuildTarget.StandaloneLinux64);
            BuildAssetBundlesForPlatform(BuildTarget.StandaloneWindows64);
            BuildAssetBundlesForPlatform(BuildTarget.StandaloneOSX);
        }

        private void BuildAssetBundlesForPlatform(BuildTarget target)
        {
            var ns = target switch
            {
                BuildTarget.StandaloneWindows64 => "win",
                BuildTarget.StandaloneLinux64 => "linux",
                BuildTarget.StandaloneOSX => "mac",
                _ => throw new ArgumentOutOfRangeException(nameof(target))
            };
            var buildPath = Path.Combine("Temp", "Build", "AssetBundles", ns);
            var destDir = Path.Combine(_buildPath, ns);
            if (!Directory.Exists(buildPath))
                Directory.CreateDirectory(buildPath);
            var directoryExists = Directory.Exists(destDir);
            if (!directoryExists) Directory.CreateDirectory(destDir);

            if (!SkipAssetBundleBuild || !directoryExists)
                BuildPipeline.BuildAssetBundles(buildPath, BuildAssetBundleOptions.ForceRebuildAssetBundle, target);

            //var files = new[] { $"{modInfo.Id}_assets.bundle", $"{modInfo.Id}_scenes.bundle" };

            foreach (var file in AssetBundles)
            {
                var source = Path.Combine(buildPath, file);
                var destination = Path.Combine(destDir, file);

                File.Copy(source, destination, true);
            }
        }

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
