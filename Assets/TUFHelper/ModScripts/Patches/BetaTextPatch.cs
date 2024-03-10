using HarmonyLib;
using UnityEngine.SceneManagement;

namespace TUFHelper
{
    [HarmonyPatch]
    public class BetaTextPatch
    {
        internal static bool cacheBetaTextEnabled;

        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(scrEnableIfBeta), "Awake")]
        private static void FixBetaTextBug(scrEnableIfBeta __instance)
        {
            __instance.gameObject.SetActive(cacheBetaTextEnabled);
        }
    }
}