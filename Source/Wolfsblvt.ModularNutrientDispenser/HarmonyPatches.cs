using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Wolfsblvt.ModularNutrientDispenser
{
    public static class HarmonyPatches
    {
        [UsedImplicitly]
        [HarmonyPatch(typeof(ThingListGroupHelper), nameof(ThingListGroupHelper.Includes))]
        public static class ThingListGroupHelper_Includes_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix_FoodSourceDispenserFix(ref bool __result, ThingRequestGroup group, ThingDef def)
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
}