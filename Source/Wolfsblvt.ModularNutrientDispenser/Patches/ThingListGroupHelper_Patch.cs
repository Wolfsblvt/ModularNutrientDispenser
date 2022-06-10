using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Wolfsblvt.ModularNutrientDispenser.Patches
{
    [UsedImplicitly]
    [HarmonyPatch(typeof(ThingListGroupHelper))]
    public static class ThingListGroupHelper_Patch
    {
        /// <summary>
        ///     Applying Prefix to <see cref="ThingListGroupHelper" />.<see cref="ThingListGroupHelper.Includes" />.
        ///     <para />
        ///     We have to fix the food source group checks to include inheritance for dispensers.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ThingListGroupHelper.Includes))]
        public static bool Prefix_Includes_FoodSourceDispenserFix(ref bool __result, ThingRequestGroup group, [NotNull] ThingDef def)
        {
            // We are copying the vanilla logic for both food source checks in that switch case.
            // The only change is to use IsAssignableFrom to allow inherited types of the nutrient paste dispenser
            // For those checks, we don't run the original method, as we found the result already
            switch (group)
            {
                case ThingRequestGroup.FoodSource:
                    __result = def.IsNutritionGivingIngestible
                               || typeof(Building_NutrientPasteDispenser).IsAssignableFrom(def.thingClass);
                    return false;
                case ThingRequestGroup.FoodSourceNotPlantOrTree:
                    __result = def.IsNutritionGivingIngestible
                               && (def.ingestible.foodType & ~FoodTypeFlags.Plant & ~FoodTypeFlags.Tree) != FoodTypeFlags.None
                               || typeof(Building_NutrientPasteDispenser).IsAssignableFrom(def.thingClass);
                    return false;
                default:
                    return true;
            }
        }
    }
}