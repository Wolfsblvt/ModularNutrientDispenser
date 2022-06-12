using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;

namespace Wolfsblvt.ModularNutrientDispenser.Patches
{
    [UsedImplicitly]
    [HarmonyPatch(typeof(Building_NutrientPasteDispenser))]
    public static class Building_NutrientPasteDispenser_Patch
    {
        /// <summary>
        ///     Applying Prefix to <see cref="Building_NutrientPasteDispenser" />.<see cref="Building_NutrientPasteDispenser.CanDispenseNow" />.
        ///     <para />
        ///     We remove the vanilla logic here that the dispenser has to have power to dispense. Only important value is if there is enough feed.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Building_NutrientPasteDispenser.CanDispenseNow), MethodType.Getter)]
        public static bool Prefix_CanDispenseNow_Rework(ref Building_NutrientPasteDispenser __instance, ref bool __result)
        {
            __result = __instance.HasEnoughFeedstockInHoppers();
            return false;
        }
    }
}