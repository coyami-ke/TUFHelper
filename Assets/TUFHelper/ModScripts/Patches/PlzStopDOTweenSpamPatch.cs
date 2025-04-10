using DG.Tweening.Core;
using HarmonyLib;

namespace TUFHelper
{
    [HarmonyPatch]
    public class PlzStopDOTweenSpamPatch
    {
        
        [HarmonyPatch(typeof(Debugger), "LogSafeModeCapturedError")]
        [HarmonyPrefix]
        public static bool DisableLog()
        {
            return false;
        }
        
        [HarmonyPatch(typeof(Debugger), "Log")]
        [HarmonyPrefix]
        public static bool DisableLog2()
        {
            return false;
        }
        
    }
}