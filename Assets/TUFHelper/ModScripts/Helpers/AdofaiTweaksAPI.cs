using System.Reflection;
using HarmonyLib;
using UnityModManagerNet;

namespace TUFHelper.ModScripts.Helpers
{
    public class AdofaiTweaksAPI
    {
        private static UnityModManager.ModEntry _adofaiTweaksMod;
        private static PropertyInfo _hideUISetting;

        public static void UpdateUI()
        {
            if (_adofaiTweaksMod == null) return;
            if (!_adofaiTweaksMod.Enabled) return;
            
            var instance = _hideUISetting.GetValue(null);
            instance.GetType()?.GetMethod("ShowOrHideElements", AccessTools.all)?.Invoke(instance, null);
        }
        
        public static void Init()
        {
            _adofaiTweaksMod = UnityModManager.FindMod("AdofaiTweaks");
            if (_adofaiTweaksMod != null)
            {
                var adofaiTweaksAssembly = _adofaiTweaksMod.Assembly;
                
                if (adofaiTweaksAssembly == null) return;
                
                var t = adofaiTweaksAssembly.GetType("AdofaiTweaks.Tweaks.HideUiElements.HideUiElementsPatches");
                _hideUISetting = t.GetProperty("Settings", AccessTools.all);
            }
        }
    }
}