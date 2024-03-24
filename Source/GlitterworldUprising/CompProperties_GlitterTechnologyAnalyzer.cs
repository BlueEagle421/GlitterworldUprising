using Verse;

namespace GliterworldUprising
{
    public class CompProperties_GlitterTechnologyAnalyzer : CompProperties
    {
        public ThingDef thing;
        public float energyPerDayMultiplier;
        public int fuelNeeded;
        public CompProperties_GlitterTechnologyAnalyzer() => this.compClass = typeof(CompGlitterTechnologyAnalyzer);
    }
}
