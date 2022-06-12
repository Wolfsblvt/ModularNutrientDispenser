using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Wolfsblvt.ModularNutrientDispenser.Patches;

namespace Wolfsblvt.ModularNutrientDispenser
{
    [StaticConstructorOnStartup]
    [UsedImplicitly]
    [SuppressMessage("ReSharper", "RedundantAssignment", Justification = "Harmony parameters are predefined by name. We just use them")]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("wolfsblvt.modularnutrientdispenser");
            harmony.PatchAll();

            // Do some manual patches
            Patch_FoodUtility_BestFoodSourceOnMap_foodValidator(harmony);
        }

        private static void Patch_FoodUtility_BestFoodSourceOnMap_foodValidator([NotNull] Harmony harmony)
        {
            var foodValidator = AccessTools.FindIncludingInnerTypes(typeof(FoodUtility), type => AccessTools.FirstMethod(type,
                method =>
                {
                    // Most generic checks
                    if (method.IsStatic || method.IsConstructor || method.ReturnType != typeof(bool))
                        return false;
                    // Parameter must be Thing
                    var parms = method.GetParameters();
                    if (parms.Length != 1 || parms[0].ParameterType != typeof(Thing))
                        return false;
                    // Okay, now it gets more complicated. We have to check if this delegate starts with our relevant instructions.
                    // We only take the first 20 statements, to save some performance. If it doesn't popup there, it shouldn't be this one.
                    // Or we have to fix the transpiler anyway.
                    return PatchProcessor.ReadMethodBody(method)
                        .Take(10)
                        .Any(x =>
                        {
                            if (x.Key != OpCodes.Isinst)
                                return false;
                            var inst = new CodeInstruction(x.Key, x.Value);
                            return inst.OperandIs(typeof(Building_NutrientPasteDispenser));
                        });
                }));
            harmony.Patch(foodValidator, transpiler: new HarmonyMethod(
                typeof(FoodUtility_Patch),
                nameof(FoodUtility_Patch.Transpiler_BestFoodSourceOnMap_FixNutrientPasteDispenserCheck)));
        }
    }
}