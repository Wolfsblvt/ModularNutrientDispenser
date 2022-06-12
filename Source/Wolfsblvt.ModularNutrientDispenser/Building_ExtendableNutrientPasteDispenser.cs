using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using Wolfsblvt.ModularNutrientDispenser.Extensions;

namespace Wolfsblvt.ModularNutrientDispenser
{
    /// <summary>
    ///     Replaces the vanilla dispenser to something more generic and more flexible.
    ///     It will be able to dispense configurable things, and it internally processes the raw ingredients if there is power available.
    ///     The dispense itself will happen based on the processed mats, and doesn't need power anymore.
    ///     Mats - meaning materials - is a more generic replacement of nutrition for this generic dispenser.
    ///     <para />
    ///     The dispenser itself keeps track of how many raw mats are processed, and how many raw ingredients it can currently pull in.
    /// </summary>
    [UsedImplicitly]
    public class Building_ExtendableNutrientPasteDispenser : Building_NutrientPasteDispenser
    {
        [NotNull] protected readonly HashSet<ThingDef> CurrentlyContainedMats = new HashSet<ThingDef>();

        /// <summary>[PERSISTENT] The value of already processed mats that is readily available</summary>
        protected float ProcessedMat;

        /// <summary>[PERSISTENT] An internal number showing how much raw mats the dispenser can pull in. It's there to limit which kind of mats can be pulled in on which ticks.</summary>
        protected float RawMatPullPower;

        /// <summary>[STAT] The maximum capacity of the processed material. A stat that can be modified.</summary>
        protected float ProcessedMatCapacity => 10f; // Hardcoded at the moment. Will come out of the def with a stat later

        /// <summary>[STAT] The amount of the mat that can be pulled in per day. Will be split onto each <see cref="TickRare" />. A stat that can be modified.</summary>
        protected float RawMatPullPerDay => 1f; // Hardcoded at the moment. Will come out of the def with a stat later

        /// <summary>[STAT] The Maximum amount of the mat that can be pulled in per <see cref="TickRare" />. A stat that can be modified.</summary>
        protected float MaxRawMatPull => 0.5f; // Hardcoded at the moment. Will come out of the def with a stat later

        /// <summary>[DEF] The stat that will be used as a material base.</summary>
        protected StatDef StatForDispensable => StatDefOf.Nutrition;

        /// <summary>[DEF] Defined in the def will be the thing that can be dispensed. Strongly connected to <see cref="StatForDispensable" />.</summary>
        public override ThingDef DispensableDef => ThingDefOf.MealNutrientPaste;

        /// <summary>[DEF] The conversion rate how much of the stat from the raw material will be converted into the stat of the target material.</summary>
        protected float MatConversion => 3.0f; // Hardcoded at the moment. Will come out of the def with a stat later

        protected virtual float DispensableMatRawCost => DispensableMatResultCost / MatConversion;
        protected virtual float DispensableMatResultCost => DispensableDef.GetStatValueAbstract(StatForDispensable);
        protected virtual int DispensableAvailable => (int)Mathf.Floor(ProcessedMat / DispensableMatResultCost);

        [CanBeNull]
        public override Thing TryDispenseFood()
        {
            // Check if we can dispense right now
            if (!(DispensableAvailable > 0))
            {
                Log.Warning($"Can't dispense {DispensableDef.label}, not enough available. Should not happen based on the checks before.");
                return null;
            }

            // Reduce the processed mats based on what we are dispensing right now
            ProcessedMat -= DispensableMatResultCost;

            def.building.soundDispense.PlayOneShot(new TargetInfo(Position, Map));
            var thing = ThingMaker.MakeThing(DispensableDef);
            RegisterIngredients(thing);
            return thing;

            // Base call a thing of the past, we do our own dispensing
            // return base.TryDispenseFood();
        }

        public override Thing FindFeedInAnyHopper()
        {
            // Currently we are still working with food, so the base call works. In the future, we want o be able to use the stats based on StatForDispensable.
            Log.Warning("This is a legacy method from vanilla, that shouldn't be called anymore.");
            return base.FindFeedInAnyHopper();
        }

        /// <summary>
        ///     We are overriding vanilla functionality and "abusing" this method. It checks if enough food is available in the hoppers for this dispenser.
        ///     This check returns whether it is possible to acquire a meal here.
        ///     So we replace this logic with our own internal availability check.
        /// </summary>
        /// <returns>Whether the dispenser can actually dispense something right now</returns>
        public override bool HasEnoughFeedstockInHoppers()
        {
            return DispensableAvailable > 0;
        }

        public override void TickRare()
        {
            base.TickRare();

            // Okay, so what we do might sound complicated. But it's easy.
            // With every tick, the "power" to pull raw material in increases.
            // We have that value because most likely raw material can't be pulled into on each tick, so we have to keep track how often we want and can do it
            if (powerComp.PowerOn)
            {
                const float rareTicksPerDay = (float)GenDate.TicksPerDay / GenTicks.TickRareInterval;
                var pullPerRareTick = RawMatPullPerDay / rareTicksPerDay;

                // So on each tick we increase the pull power by a calculated value. We limit it somewhere still we can't shlurp in all at once.
                RawMatPullPower = RawMatPullPower.AddCapped(pullPerRareTick, MaxRawMatPull);

                // Then the main processing starts.
                // If we got enough to pull something in, we do it here
                TryProcessIngredients();
            }

            Log.Warning($"Dispenser Tick. Progress: {ProcessedMat}");
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ProcessedMat, nameof(ProcessedMat));
            Scribe_Values.Look(ref RawMatPullPower, nameof(RawMatPullPower));
        }

        public override string GetInspectString()
        {
            var sb = new StringBuilder(base.GetInspectString());

            if (sb.Length > 0)
                sb.AppendLine();

            sb.AppendLine($"Contains {ProcessedMat.ToStringDecimalIfSmall()} / {ProcessedMatCapacity} {StatForDispensable.label}.");
            //sb.AppendLine($"Progress: {(ProcessedMat / ProcessedMatCapacity).ToStringPercent()} (___ left)");

            sb.AppendLine($"Available {Find.ActiveLanguageWorker.Pluralize(DispensableDef.label, DispensableAvailable)}: {DispensableAvailable}");

            if (Prefs.DevMode)
                sb.AppendLine($"Pull power: {RawMatPullPower}");

            return sb.ToString().TrimEndNewlines();
        }

        protected virtual void RegisterIngredients([NotNull] Thing thing)
        {
            var compIngredients = thing.TryGetComp<CompIngredients>();
            foreach (var t in CurrentlyContainedMats)
                compIngredients.RegisterIngredient(t);
        }

        protected virtual bool TryProcessIngredients()
        {
            // If we are already at full capacity, we can't process more
            if (ProcessedMat >= ProcessedMatCapacity)
                return false;

            // Only returns an ingredient if there is one available
            var ingredient = TryGrabRawIngredient();
            if (ingredient == null)
                return false;

            var statValue = ingredient.GetStatValue(StatForDispensable);

            // Calculate how much of that mat we could pull. Don't go further that the stack size though
            var amount = Mathf.FloorToInt(RawMatPullPower / statValue).Cap(ingredient.stackCount);

            // And don't pull more than we need to reach max capacity
            var remainingCapacity = ProcessedMatCapacity - ProcessedMat;
            var amountToCapacity = Mathf.CeilToInt(remainingCapacity / statValue);
            if (amount > amountToCapacity)
                amount = amountToCapacity;

            ProcessAndUseRawMat(ingredient, amount);

            // Pull power gets reduced by the amount we pulled
            RawMatPullPower -= amount * statValue;
            Log.Message($"Pulled until a remaining power of {RawMatPullPower}");

            return true;
        }

        [Pure]
        [CanBeNull]
        protected virtual Thing TryGrabRawIngredient()
        {
            // We are doing a simple ordering on smallest stack size, so that we empty mostly empty hoppers first.
            // We don't want to take the first available which is possible right now, we really wait for the first one, until we have the capacity to do so.
            var ingredient = IngredientsInHoppers()
                .OrderBy(x => x.stackCount)
                .FirstOrDefault();

            if (ingredient == null)
                return null;

            // Check how much mats that ingredient has. If it's too much to pull in, we can't return it.
            var statValue = ingredient.GetStatValue(StatForDispensable);
            if (statValue > RawMatPullPower)
                return null;

            return ingredient;
        }

        [Pure]
        [NotNull]
        protected virtual IEnumerable<Thing> IngredientsInHoppers()
        {
            foreach (var adjCell in AdjCellsCardinalInBounds)
            {
                // Check if on this cell is a hopper. And then also a raw mat we need.
                Thing matInHopper = null;
                Thing hopper = null;
                foreach (var thing in adjCell.GetThingList(Map))
                {
                    if (IsAcceptableRawMat(thing))
                        matInHopper = thing;

                    if (thing.def == ThingDefOf.Hopper)
                        hopper = thing;
                }

                if (matInHopper != null && hopper != null)
                    yield return matInHopper;
            }
        }

        protected virtual void ProcessAndUseRawMat([NotNull] Thing mat, int count)
        {
            if (count > mat.stackCount)
            {
                Log.Error("Can't call the process method of a count lower than the stack contains.");
                return;
            }

            if (count == 0)
                return;

            mat.SplitOff(count);

            CurrentlyContainedMats.Add(mat.def);

            var resultingMats = mat.GetStatValue(StatForDispensable) * count * MatConversion;

            ProcessedMat = ProcessedMat.AddCapped(resultingMats, ProcessedMatCapacity);
        }

        [Pure]
        protected virtual bool IsAcceptableRawMat([NotNull] Thing thing)
        {
            // For current nutrients, we are still using this old method
            return IsAcceptableFeedstock(thing.def);
        }
    }
}