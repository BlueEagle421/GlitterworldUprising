using Verse;
using UnityEngine;

namespace GliterworldUprising
{
    public class CompProperties_AutoMaintainer : CompProperties
    {
        public float radius;
        public int rareTickPerCheck, rareTicksPerMaintain;
        public ThingDef moteDef;
        public CompProperties_AutoMaintainer() => this.compClass = typeof(Comp_AutoMaintainer);
    }

}
