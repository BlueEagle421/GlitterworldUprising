using Verse;
using UnityEngine;

namespace GliterworldUprising
{
    public class CompProperties_PassiveRes : CompProperties
    {
        public bool isFacility;
        public CompProperties_PassiveRes() => this.compClass = typeof(CompPassiveRes);
    }

}
