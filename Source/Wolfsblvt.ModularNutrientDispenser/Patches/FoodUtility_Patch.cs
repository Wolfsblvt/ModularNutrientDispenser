using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;

namespace Wolfsblvt.ModularNutrientDispenser.Patches
{
    [UsedImplicitly]
    [HarmonyPatch(typeof(FoodUtility))]
    [SuppressMessage("ReSharper", "CommentTypo", Justification = "Doesn't make sense if I want to inline code as comments for documentation purposes")]
    public static class FoodUtility_Patch
    {
        /// <summary>
        ///     Applying Transpiler to <see cref="FoodUtility" />.<see cref="FoodUtility.BestFoodSourceOnMap" />.
        ///     <para />
        ///     This transpiler edits the foodValidator part to be more generic for dispenser in what they check for.
        /// </summary>
        public static IEnumerable<CodeInstruction> Transpiler_BestFoodSourceOnMap_FixNutrientPasteDispenserCheck(IEnumerable<CodeInstruction> instructions)
        {
            // Change vanilla
            //if (!allowDispenserFull || !getterCanManipulate || (int)ThingDefOf.MealNutrientPaste.ingestible.preferability < (int)minPref || (int)ThingDefOf.MealNutrientPaste.ingestible.preferability > (int)maxPref || !eater.WillEat(ThingDefOf.MealNutrientPaste, getter) || (t.Faction != getter.Faction && t.Faction != getter.HostFaction) || (!allowForbidden && t.IsForbidden(getter)) || !building_NutrientPasteDispenser.powerComp.PowerOn || (!allowDispenserEmpty && !building_NutrientPasteDispenser.HasEnoughFeedstockInHoppers()) || !t.InteractionCell.Standable(t.Map) || !IsFoodSourceOnMapSociallyProper(t, getter, eater, allowSociallyImproper) || !getter.Map.reachability.CanReachNonLocal(getter.Position, new TargetInfo(t.InteractionCell, t.Map), PathEndMode.OnCell, TraverseParms.For(getter, Danger.Some)))
            //{
            //    return false;
            //}
            // To this
            //if (!allowDispenserFull || !getterCanManipulate || (int)building_NutrientPasteDispenser.DispensableDef.ingestible.preferability < (int)minPref || (int)building_NutrientPasteDispenser.DispensableDef.ingestible.preferability > (int)maxPref || !eater.WillEat(building_NutrientPasteDispenser.DispensableDef, getter) || (t.Faction != getter.Faction && t.Faction != getter.HostFaction) || (!allowForbidden && t.IsForbidden(getter)) || !building_NutrientPasteDispenser.CanDispenseNow || (!allowDispenserEmpty && !building_NutrientPasteDispenser.HasEnoughFeedstockInHoppers()) || !t.InteractionCell.Standable(t.Map) || !IsFoodSourceOnMapSociallyProper(t, getter, eater, allowSociallyImproper) || !getter.Map.reachability.CanReachNonLocal(getter.Position, new TargetInfo(t.InteractionCell, t.Map), PathEndMode.OnCell, TraverseParms.For(getter, Danger.Some)))
            //{
            //    return false;
            //}

            // Which is in short
            //
            //ThingDefOf.MealNutrientPaste
            // with
            //building_NutrientPasteDispenser.DispensableDef
            //
            // and
            //
            //building_NutrientPasteDispenser.powerComp.PowerOn
            // with
            //building_NutrientPasteDispenser.CanDispenseNow

            // As we are not modifying much, only replacing a few IL statement with a few more, we can easily just do a general foreach
            // and replace the relevant parts without having to keep track of where we are exactly.

            var removeNext = 0;

            foreach (var inst in instructions)
            {
                if (removeNext > 0 && removeNext-- > int.MinValue)
                    continue;

                yield return inst;
            }
        }
    }
}