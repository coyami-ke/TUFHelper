using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ADOFAIModdingHelper.Utilities
{
    public static class ProjectWindowPathHelper
    {
        private static readonly MethodInfo getActiveFolderPathMethod;
        private static readonly MethodInfo tryGetActiveFolderPathMethod;

        static ProjectWindowPathHelper()
        {
            var type = typeof(ProjectWindowUtil);

            getActiveFolderPathMethod = type.GetMethod(
                "GetActiveFolderPath",
                BindingFlags.Static | BindingFlags.NonPublic
            );

            tryGetActiveFolderPathMethod = type.GetMethod(
                "TryGetActiveFolderPath",
                BindingFlags.Static | BindingFlags.NonPublic
            );
        }

        /// <summary>
        /// Gets the current folder in the Project window (like when you right-click → Create).
        /// Falls back to "Assets" if nothing valid is found.
        /// </summary>
        public static string GetActiveFolderPath()
        {
            if (tryGetActiveFolderPathMethod != null)
            {
                object[] args = { null };
                bool success = (bool)tryGetActiveFolderPathMethod.Invoke(null, args);
                if (success && args[0] is string path && !string.IsNullOrEmpty(path))
                    return path;
            }

            if (getActiveFolderPathMethod != null)
            {
                try
                {
                    string path = (string)getActiveFolderPathMethod.Invoke(null, null);
                    if (!string.IsNullOrEmpty(path))
                        return path;
                }
                catch {}
            }

            return "Assets";
        }

        public static string GetCurrentProjectWindowPath()
        {
            if (Selection.activeObject != null)
            {
                string selPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (AssetDatabase.IsValidFolder(selPath))
                    return selPath;
                else
                    return Path.GetDirectoryName(selPath).Replace("\\", "/");
            }
            return GetActiveFolderPath();
        }

        /// <summary>
        /// Open specified folder in Project Browser
        /// </summary>
        public static void OpenFolder(string folderPath)
        {
            var folder = AssetDatabase.LoadAssetAtPath<Object>(folderPath);
            if (folder == null)
            {
                Debug.LogWarning($"Folder not found: {folderPath}");
                return;
            }

            int folderInstanceId = folder.GetInstanceID();

            var projectBrowserType = typeof(Editor).Assembly.GetType("UnityEditor.ProjectBrowser");
            var windows = Resources.FindObjectsOfTypeAll(projectBrowserType);
            if (windows.Length == 0)
            {
                EditorApplication.ExecuteMenuItem("Window/General/Project");
                windows = Resources.FindObjectsOfTypeAll(projectBrowserType);
            }

            if (windows.Length > 0)
            {
                var browser = windows[0];

                var showFolderContents = projectBrowserType.GetMethod(
                    "ShowFolderContents",
                    BindingFlags.Instance | BindingFlags.NonPublic
                );

                if (showFolderContents != null)
                {
                    showFolderContents.Invoke(browser, new object[] { folderInstanceId, true });
                }
                else
                {
                    Debug.LogError("Could not find ShowFolderContents method");
                }
            }
        }
    }
}