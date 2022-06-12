using JetBrains.Annotations;
using Verse;

namespace Wolfsblvt.ModularNutrientDispenser
{
    [StaticConstructorOnStartup]
    [UsedImplicitly]
    public static class StartupHelper
    {
        static StartupHelper()
        {
            Log.Message($"{nameof(ModularNutrientDispenser)} loaded successfully.");
        }
    }
}
