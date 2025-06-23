using RimWorld;
using Verse;

namespace GlitterworldUprising
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
        public static EffecterDef Repair;
        public static StatDef USH_GlittertechPowerStored;
        public static StatDef USH_GlittertechDuration;
    }

}

