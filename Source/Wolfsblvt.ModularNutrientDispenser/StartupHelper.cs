using JetBrains.Annotations;
using Verse;

namespace Wolfsblvt.ModularNutrientDispenser
{
    [UsedImplicitly]
    [StaticConstructorOnStartup]
    public static class StartupHelper
    {
        static StartupHelper()
        {
            Log.Message($"{nameof(ModularNutrientDispenser)} loaded successfully.");
        }
    }
}
