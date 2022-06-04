using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Wolfsblvt.ModularNutrientDispenser
{
    [UsedImplicitly]
    public class Building_ExtendableNutrientPasteDispenser : Building_NutrientPasteDispenser
    {
        public override Thing TryDispenseFood()
        {
            Log.Message("Oooooh, we are trying to dispense something.");
            return base.TryDispenseFood();
        }
    }
}