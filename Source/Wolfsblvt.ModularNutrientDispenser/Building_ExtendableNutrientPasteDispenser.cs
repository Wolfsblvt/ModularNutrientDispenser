using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Wolfsblvt.ModularNutrientDispenser
{
    [UsedImplicitly]
    public class Building_ExtendableNutrientPasteDispenser : Building_NutrientPasteDispenser
    {
        protected int NutritionCount;
        protected float ProgressInt;

        public override Thing TryDispenseFood()
        {
            Log.Message("Oooooh, we are trying to dispense something.");
            return base.TryDispenseFood();
        }

        public override void TickRare()
        {
            base.TickRare();

            ProgressInt += 0.1f;

            Log.Warning($"Dispenser Tick. Progress: {ProgressInt}");
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref NutritionCount, "NutritionCount");
            Scribe_Values.Look(ref ProgressInt, "progress");
        }
    }
}