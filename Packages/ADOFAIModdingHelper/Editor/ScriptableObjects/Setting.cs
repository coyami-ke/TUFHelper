using ADOFAIModdingHelper.Common;
using ADOFAIModdingHelper.Core;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ADOFAIModdingHelper.ScriptableObjects
{
    public class Setting : ScriptableObject
    {
        private static Setting _config;

        public static Setting Config
        {
            get
            {
                if (_config)
                    return _config;
                _config = AssetDatabase.LoadAssetAtPath<Setting>(Constants.settingsFolder + "/AMHSettings.asset");
                if (_config) return _config;

                _config = CreateInstance<Setting>();
                if (!Directory.Exists(Constants.settingsFolder))
                {
                    Directory.CreateDirectory(Constants.settingsFolder);
                }
                AssetDatabase.CreateAsset(_config, Constants.settingsFolder + "/AMHSettings.asset");

                return _config;
            }
        }

        public GameImporter Importer = new GameImporter();
        public string ADOFAIPath;
        public string DecompilerPath;
        public bool SeperateBuildTabs = true;
        public int CurrentPage = 0;

        public void RunAppWithoutConfig() => Process.Start(Config.ADOFAIPath);
    }
}
