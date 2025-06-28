using RimWorld;
using Verse;

namespace GlittertechExpansion
{
    [DefOf]
    public static class USHDefOf
    {
        static USHDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(USHDefOf));
        }

        public static EffecterDef USH_ElectricForming;
        public static EffecterDef USH_ElectricResearchProbe;
        public static EffecterDef USH_GlittertechRepair;
        public static StatDef USH_GlittertechPowerStored;
        public static StatDef USH_GlittertechDuration;
        public static ThingDef USH_PowerStored;
    }

}

