using RimWorld;
using Verse;

namespace USH_GE
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
        public static DamageDef USH_ADP;
        public static WorkGiverDef USH_DoBillsUpgrades;
        public static HediffDef USH_InstalledTelepadIntegrator;
        public static JobDef USH_EnterBiocoder;
        public static JobDef USH_CarryToBiocoder;
        public static ThingDef USH_GlittertechTargeter;
    }

}

