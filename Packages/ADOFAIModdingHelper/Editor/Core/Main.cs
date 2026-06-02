using ADOFAIModdingHelper.Common;
using ADOFAIModdingHelper.ScriptableObjects;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace ADOFAIModdingHelper.Core
{
    [InitializeOnLoad]
    public static class Main
    {
        static Main()
        {
            EditorApplication.delayCall += SetupEnvironment;
        }

        private static void SetupEnvironment()
        {
            Debug.Log("Setting up ADOFAI Modding Helper environment...");
            if (!Directory.Exists(Constants.settingsFolder))
            {
                Directory.CreateDirectory(Constants.settingsFolder);
                AssetDatabase.Refresh();
            }

            Logger.Init();

            _ = Setting.Config;
            _ = ModToolsConfig.Config;

            AssetDatabase.Refresh();
        }
    }
}
