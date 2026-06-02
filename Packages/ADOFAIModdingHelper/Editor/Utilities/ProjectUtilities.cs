using ADOFAIModdingHelper.Core;
using UnityEditor;
using UnityEngine;

namespace ADOFAIModdingHelper.Utilities
{
    public static class ProjectUtilities
    {
        //[MenuItem(Constants.ADOFAIRunnerMenuRoot + "Open Console %#c", false, priority: Constants.ADOFAIRunnerMenuPriority)]
        public static void OpenConsoleWindow()
        {
            var assembly = typeof(EditorWindow).Assembly;
            var consoleWindowType = assembly.GetType("UnityEditor.ConsoleWindow");

            if (consoleWindowType != null)
            {
                EditorWindow.GetWindow(consoleWindowType);
            }
            else
            {
                Debug.LogError("Could not find UnityEditor.ConsoleWindow type.");
            }
        }

        public static float DynamicaWidth(string text, float extraAmount)
        {
            GUIStyle popupStyle = EditorStyles.popup;
            Vector2 size = popupStyle.CalcSize(new GUIContent(text));
            return size.x + extraAmount;
        }
        public static void CreateFolderInAssets(string folderName)
        {
            string path = "Assets/" + folderName;

            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets", folderName);
                AssetDatabase.Refresh();
                Debug.Log($"Created folder at: {path}");
            }
            else
            {
                Debug.Log($"Folder already exists: {path}");
            }
        }
    }
}