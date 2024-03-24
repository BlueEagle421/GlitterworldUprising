using Verse;
using UnityEngine;

namespace GliterworldUprising
{
    public class CompProperties_AutoRepairer : CompProperties
    {
        public float radius;
        public int repairAmount, rareTicksPerCheck, overclockPowerConsumption, defaultPowerConsumtion, ticksToOverheat;
        public ThingDef moteDef;
        public CompProperties_AutoRepairer() => this.compClass = typeof(Comp_AutoRepairer);
    }

}
