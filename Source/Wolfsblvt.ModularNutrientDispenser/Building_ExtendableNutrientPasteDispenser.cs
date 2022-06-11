using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        /// <summary>[PERSISTENT] The value of already processed mats that is readily available</summary>
        protected float ProcessedMat;

        /// <summary>[PERSISTENT] An internal number showing how much raw mats the dispenser can pull in. It's there to limit which kind of mats can be pulled in on which ticks.</summary>
        protected float RawMatPullPower;

        /// <summary>[STAT] The maximum capacity of the processed material. A stat that can be modified.</summary>
        protected float ProcessedMatCapacity => 10f; // Hardcoded at the moment. Will come out of the def with a stat later

        /// <summary>[STAT] The amount of the mat that can be pulled in per <see cref="TickRare" />. A stat that can be modified.</summary>
        protected float RawMatPullPerTick => 0.002f; // Hardcoded at the moment. Will come out of the def with a stat later

        /// <summary>[STAT] The exact amount of units that will be pulled for a stack at maximum. A stat that can be modified.</summary>
        protected float RawMatPullCapacityPerHopper => 0.25f; // Hardcoded at the moment. Will come out of the def with a stat later

        /// <summary>[DEF] The stat that will be used as a material base.</summary>
        protected StatDef StatForDispensable => StatDefOf.Nutrition;

        /// <summary>[DEF] Defined in the def will be the thing that can be dispensed. Strongly connected to <see cref="StatForDispensable" />.</summary>
        public override ThingDef DispensableDef => ThingDefOf.MealNutrientPaste;

        /// <summary>[DEF] The conversion rate how much of the stat from the raw material will be converted into the stat of the target material.</summary>
        protected float MatConversion => 3.0f; // Hardcoded at the moment. Will come out of the def with a stat later
        
        [NotNull] protected readonly HashSet<ThingDef> CurrentlyContainedMats = new HashSet<ThingDef>();

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

            // The power itself increases by the defined stat behind. We still limit it somewhere, to prevent overflows.
            RawMatPullPower += RawMatPullPower.AddCapped(RawMatPullPerTick, ProcessedMatCapacity);

            // Then the main processing starts.
            // If we got enough to pull something in, we do it here
            TryProcessIngredients();

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
            sb.AppendLine($"Progress: {(ProcessedMat / ProcessedMatCapacity).ToStringPercent()} (___ left)");

            sb.AppendLine($"Available {Find.ActiveLanguageWorker.Pluralize(DispensableDef.label, DispensableAvailable)}: {DispensableAvailable}");

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
            // Only returns a list of raw ingredients if there is enough available
            var rawIngredients = TryGrabRawIngredients(RawMatPullPower);
            
            // Not enough available for that processing here
            if (rawIngredients == null)
                return false;

            // If we have enough available, we pull here until we reach exactly it.
            // We are just pulling right away, as we are sure there is enough available. Otherwise the method would've responded with null.
            var pullPower = RawMatPullPower;
            foreach (var ingredient in rawIngredients)
            {
                var ingredientStatValue = ingredient.GetStatValue(StatForDispensable);
                var stackContainsStat = ingredient.stackCount * ingredientStatValue;

                // We have to limit how much we can pull from one hopper at max
                var canPullStat = Math.Min(stackContainsStat, RawMatPullCapacityPerHopper);

                // If we got enough on that stack for what we want to pull
                // If not, we pull as much as we can from that stack and check the next ingredient
                int amount;
                if (canPullStat >= pullPower)
                    amount = Mathf.FloorToInt(pullPower / ingredientStatValue);
                else
                    amount = ingredient.stackCount;

                // We pull as much as is available here. And check after if this was enough.
                ProcessAndUseRawMat(ingredient, amount);
                pullPower -= amount * ingredientStatValue;

                // We can stop here if we got enough on that stack for what we want to pull
                if (stackContainsStat >= pullPower)
                    break;
            }
            
            RawMatPullPower = pullPower;
            Log.Message($"Pulled until a remaining power of {pullPower}");

            return true;
        }

        [Pure]
        [CanBeNull]
        protected virtual IEnumerable<Thing> TryGrabRawIngredients(float amount)
        {
            // We are doing a simple ordering on smallest stack size, so that we empty mostly empty hoppers first.
            var possibleIngredients = IngredientsInHoppers()
                .OrderBy(x => x.stackCount);

            var grabbedThings = new List<Thing>();
            var totalGrabbedMatCount = 0f;

            foreach (var ingredient in possibleIngredients)
            {
                // We need to stop here if we are over the maximum capacity we can pull at all
                if (totalGrabbedMatCount >= ProcessedMatCapacity)
                    break;

                // We also need to stop if a single ingredient is needing more than the amount is we can maximally get
                var statValue = ingredient.GetStatValue(StatForDispensable);
                if (statValue > amount)
                    break;

                // No matter how many items we grab from this thing, we take at least one here so we are registering it
                grabbedThings.Add(ingredient);

                var stackContainsStat = ingredient.stackCount * statValue;
                var canPullStat = Math.Min(stackContainsStat, RawMatPullCapacityPerHopper);

                totalGrabbedMatCount += canPullStat;

                // We can stop here if we got enough, of course
                if (totalGrabbedMatCount >= amount)
                    break;
            }

            return totalGrabbedMatCount >= amount ? grabbedThings : null;
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
                Log.Error("Can't call the process method of a count lower than the stack contains.");

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