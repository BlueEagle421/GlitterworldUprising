using Verse;
using UnityEngine;

namespace GliterworldUprising
{
    public class CompProperties_AutoBuilder : CompProperties
    {
        public float radius;
        public int workAmount, rareTicksBeforeCheck, rareTickPerCheck;
        public ThingDef moteDef;
        public CompProperties_AutoBuilder() => this.compClass = typeof(Comp_AutoBuilder);
    }

}
