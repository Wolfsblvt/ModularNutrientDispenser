using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
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
            
            // Caching reflection results
            FieldInfo f1 = null, f2 = null;

            var removeNext = 0;

            foreach (var inst in instructions)
            {
                if (removeNext > 0 && removeNext-- > int.MinValue)
                    continue;
                
                // Replace all direct meal references
                //IL_0023: ldsfld class Verse.ThingDef RimWorld.ThingDefOf::MealNutrientPaste
                var isPasteMealReference = inst.LoadsField(f1 ?? (f1 = AccessTools.Field(typeof(ThingDefOf), nameof(ThingDefOf.MealNutrientPaste))));
                if (isPasteMealReference)
                {
                    //IL_00b5: ldloc.0
                    //IL_00b6: ldfld class RimWorld.ThingDef RimWorld.Building_NutrientPasteDispenser::DispensableDef
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Building_NutrientPasteDispenser),nameof(Building_NutrientPasteDispenser.DispensableDef)));

                    // We are not using the original instruction for that meal
                    continue;
                }

                // Replace all power references
                //IL_00b5: ldloc.0
                //IL_00b6: ldfld class RimWorld.CompPowerTrader RimWorld.Building_NutrientPasteDispenser::powerComp
                //IL_00bb: callvirt instance bool RimWorld.CompPowerTrader::get_PowerOn()
                var isPowerCompReference = inst.LoadsField(f2 ?? (f2 = AccessTools.Field(typeof(Building_NutrientPasteDispenser), nameof(Building_NutrientPasteDispenser.powerComp))));
                if (isPowerCompReference)
                {
                    //IL_00b5: ldloc.0
                    //IL_00b6: bool RimWorld.Building_NutrientPasteDispenser::CanDispenseNow
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Building_NutrientPasteDispenser), nameof(Building_NutrientPasteDispenser.CanDispenseNow)));
                        
                    removeNext = 1;
                    continue;
                }

                yield return inst;
            }
        }
    }
}