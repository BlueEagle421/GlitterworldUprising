using RimWorld;
using Verse;

namespace USH_GE
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
        public static EffecterDef USH_GlittertechRepair;
        public static StatDef USH_GlittertechPowerStored;
        public static StatDef USH_GlittertechDuration;
        public static ThingDef USH_PowerStored;
        public static DamageDef USH_ADP;
        public static HediffDef USH_InstalledTelepadIntegrator;
        public static JobDef USH_EnterBiocoder;
        public static JobDef USH_CarryToBiocoder;
        public static ThingDef USH_GlittertechTargeter;
        public static ThingDef USH_MemoryCellPositive;
        public static ThingDef USH_MemoryCellNegative;
        public static HediffDef USH_MemoryPositiveHigh;
        public static HediffDef USH_MemoryNegativeHigh;
        public static ThingDef USH_MemoryPylon;
        public static JobDef USH_InsertMemoryCell;
    }
}