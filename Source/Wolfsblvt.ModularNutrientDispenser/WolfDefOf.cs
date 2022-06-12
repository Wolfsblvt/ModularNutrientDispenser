using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace Wolfsblvt.ModularNutrientDispenser
{
    [DefOf]
    [SuppressMessage("ReSharper", "UnassignedField.Global")]
    public static class WolfDefOf
    {
        [DefAlias("WOLF_ProcessedMatCapacity")]
        public static StatDef ProcessedMatCapacity;

        [DefAlias("WOLF_RawMatPullPerDay")]
        public static StatDef RawMatPullPerDay;

        [DefAlias("WOLF_MaxRawMatPerPull")]
        public static StatDef MaxRawMatPerPull;
    }
}
