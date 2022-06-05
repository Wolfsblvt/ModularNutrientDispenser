using System.Text;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using Wolfsblvt.ModularNutrientDispenser.Extensions;

namespace Wolfsblvt.ModularNutrientDispenser
{
    [UsedImplicitly]
    public class Building_ExtendableNutrientPasteDispenser : Building_NutrientPasteDispenser
    {
        /// <summary>[PERSISTENT] The nutrition that was drawn into and is currently being processed</summary>
        protected float NutritionRaw;

        /// <summary>[PERSISTENT] The value of already processed nutrition that is readily available</summary>
        protected float ProcessedNutrition;

        protected float NutritionCapacity => 10f; // Hardcoded at the moment. Will come out of the def with a stat later
        protected float NutritionPerTick => 0.1f; // Hardcoded at the moment. Will come out of the def with a stat later

        protected float DispensableNutritionCost => DispensableDef.GetStatValueAbstract(StatDefOf.Nutrition);
        protected int DispensableAvailable => (int)Mathf.Floor(ProcessedNutrition / DispensableNutritionCost);

        [CanBeNull]
        public override Thing TryDispenseFood()
        {
            Log.Message("Oooooh, we are trying to dispense something.");

            if (ProcessedNutrition - DispensableNutritionCost < 0)
            {
                Log.Message("Can't dispense meal, not enough available.");
                return null;
            }

            ProcessedNutrition -= DispensableNutritionCost;

            return base.TryDispenseFood();
        }

        public override bool HasEnoughFeedstockInHoppers()
        {
            if (ProcessedNutrition < DispensableNutritionCost)
                return false;

            return base.HasEnoughFeedstockInHoppers();
        }

        public override void TickRare()
        {
            base.TickRare();

            ProcessedNutrition = ProcessedNutrition.AddCapped(NutritionPerTick, NutritionCapacity);
            Log.Warning($"Dispenser Tick. Progress: {ProcessedNutrition}");
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref NutritionRaw, nameof(NutritionRaw));
            Scribe_Values.Look(ref ProcessedNutrition, nameof(ProcessedNutrition));
        }

        public override string GetInspectString()
        {
            var sb = new StringBuilder(255);
            sb.Append(base.GetInspectString());

            if (sb.Length > 0)
                sb.AppendLine();

            sb.AppendLine($"Contains {ProcessedNutrition.ToStringDecimalIfSmall()} / {NutritionCapacity} nutrition.");
            sb.AppendLine($"Progress: {(ProcessedNutrition / NutritionCapacity).ToStringPercent()} (___ left)");

            sb.AppendLine($"Available {Find.ActiveLanguageWorker.Pluralize(DispensableDef.label, DispensableAvailable)}: {DispensableAvailable}");

            return sb.ToString().TrimEndNewlines();
        }
    }
}