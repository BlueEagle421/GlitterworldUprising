using System.Collections.Generic;
using Verse;

namespace GliterworldUprising
{
    public class HediffCompProperties_USH_AddictionRemoval : HediffCompProperties
    {
        public List<string> removalBlackList = new List<string>();

        public HediffCompProperties_USH_AddictionRemoval() => this.compClass = typeof(HediffComp_USH_AddictionRemoval);
    }
}
