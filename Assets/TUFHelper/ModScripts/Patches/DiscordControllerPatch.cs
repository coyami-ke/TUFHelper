using System;
using System.Collections;
using System.Collections.Generic;
using Discord;
using HarmonyLib;
using TUFHelper.Utils;
using UnityEngine;

namespace TUFHelper
{
    [HarmonyPatch(typeof(DiscordController), "UpdatePresence")]
    public static class TUFCustomDiscordPresencePatch
    {
        public static bool Prefix(DiscordController __instance)
        {
            var discordField = AccessTools.Field(typeof(DiscordController), "discord");
            var discordValue = (Discord.Discord)discordField.GetValue(__instance);

            if (__instance == null || discordValue == null)
                return false;

            if (Main.isInTUFHelper)
            {
                string detail = "TUFHelper by coyami-ke";
                string state = "Official v2.3.0";

                Activity activity = default(Activity);
                activity.Name = "A Dance of Fire and Ice";
                activity.State = state;
                activity.Details = detail;
                activity.Assets.LargeImage = "planets_icon_stars";
                activity.Assets.LargeText = "Cooking MilkyWay :fire:";

                try
                {
                    discordValue.GetActivityManager().UpdateActivity(activity, delegate (Result result) { });
                }
                catch (Exception)
                {
                }

                DiscordController.shouldUpdatePresence = false;

                return false;
            }

            return true;
        }
    }

}
