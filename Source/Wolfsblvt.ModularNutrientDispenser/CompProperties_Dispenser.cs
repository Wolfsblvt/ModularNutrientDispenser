using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Wolfsblvt.ModularNutrientDispenser
{
    [UsedImplicitly]
    [SuppressMessage("ReSharper", "UnassignedField.Global", Justification = "Comp properties are loaded automtically from xaml")]
    public class CompProperties_Dispenser : CompProperties
    {
        /// <summary>[DEF] Defined in the def will be the thing that can be dispensed. Strongly connected to <see cref="statForDispensable" />.</summary>
        public ThingDef dispensableDef;

        /// <summary>[DEF] The stat that will be used as a material base.</summary>
        public StatDef statForDispensable;

        /// <summary>[DEF] The conversion rate how much of the stat from the raw material will be converted into the stat of the target material.</summary>
        public float matConversion;


        public CompProperties_Dispenser()
        {
            compClass = typeof(CompDispenser);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var error in base.ConfigErrors(parentDef))
                yield return error;

            if (dispensableDef == null)
                yield return $"{nameof(dispensableDef)} has to be defined.";
            if (statForDispensable == null)
                yield return $"{nameof(statForDispensable)} has to be defined.";
        }
    }
}