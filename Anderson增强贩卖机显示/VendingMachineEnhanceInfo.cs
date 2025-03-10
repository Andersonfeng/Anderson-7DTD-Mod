﻿using System.Reflection;
using HarmonyLib;
using HarmonyLib.Tools;

namespace VendingMachineEnhanceInfo
{
    public class VendingMachineEnhanceInfo : IModApi
    {
        public void InitMod(Mod modInstance)
        {
            Log.Out("Anderson-VendingMachineEnhanceInfo: InitMod");

            var harmony = new Harmony(GetType().Name);
            harmony.PatchAll(Assembly.GetExecutingAssembly());


            HarmonyFileLog.Enabled = true;
        }
    }

    [HarmonyPatch(typeof(XUiC_TraderWindow))]
    public class XUiC_TraderWindowPatch
    {
        public static string nextAutoBuy_date;
        public static string autoBuyThreshold;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(XUiC_TraderWindow), nameof(XUiC_TraderWindow.GetBindingValue))]
        public static bool Prefix(
            string bindingName,
            ref string value,
            ref bool __result,
            XUiC_QuestTrackerWindow __instance)
        {
            __result = true;
            switch (bindingName)
            {
                case "RentedVendorNextUpdate":
                    if (!string.IsNullOrEmpty(nextAutoBuy_date))
                    {
                        value = nextAutoBuy_date;
                    }

                    return false;
                case "NextBuyChance":
                    if (!string.IsNullOrEmpty(autoBuyThreshold))
                    {
                        value = autoBuyThreshold;
                    }

                    return false;

                default:
                    return true;
            }
        }
    }

    [HarmonyPatch(typeof(TileEntityVendingMachine))]
    public class TileEntityVendingMachinePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(TileEntityVendingMachine.TryAutoBuy))]
        public static bool Prefix(ref TileEntityVendingMachine __instance,
            ref bool __result,
            ref ulong ___nextAutoBuy,
            ref float ___autoBuyThreshold,
            ref float ___autoBuyThresholdStep,
            ref int ___minimumAutoBuyCount,
            bool isInitial = true)
        {
            var nextAutoBuy_date = GameUtils.WorldTimeToString(___nextAutoBuy);
            // Log.Out("VendingMachineEnhanceInfo: TileEntityVendingMachinePatch nextAutoBuy_date:"+nextAutoBuy_date);
            XUiC_TraderWindowPatch.nextAutoBuy_date = nextAutoBuy_date;
            XUiC_TraderWindowPatch.autoBuyThreshold = ___autoBuyThreshold.ToString();
            Log.Out("VendingMachineEnhanceInfo: TileEntityVendingMachinePatch ___autoBuyThreshold:"+___autoBuyThreshold);
            Log.Out("VendingMachineEnhanceInfo: TileEntityVendingMachinePatch autoBuyThreshold:"+XUiC_TraderWindowPatch.autoBuyThreshold);
            return true;
        }
    }
}