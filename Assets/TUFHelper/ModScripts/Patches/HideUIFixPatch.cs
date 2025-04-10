using HarmonyLib;
using TUFHelper.ModScripts.Helpers;

namespace TUFHelper
{
    [HarmonyPatch]
    public class HideUIFixPatch
    {
        public static bool RecentDirectLevelOpend;
        
        
        [HarmonyPatch(typeof(scnEditor), "Start")]
        [HarmonyPostfix]
        public static void AutoPathLock(scnEditor __instance)
        {

            if (RecentDirectLevelOpend)
            {
                AdofaiTweaksAPI.UpdateUI();

                __instance.DeselectFloors();
                __instance.LockPathEditing(true);
                
                RecentDirectLevelOpend = false;
            }
        }
        
        
    }
}