using RimWorld;
using Verse;

namespace GlitterworldUprising
{
    [DefOf]
    public static class USH_DefOf
    {
        static USH_DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(USH_DefOf));
        }

        public static EffecterDef USH_ElectricForming;
        public static EffecterDef USH_ElectricResearchProbe;
        public static StatDef USH_GlittertechPowerStored;
        public static StatDef USH_GlittertechDuration;
    }

}

