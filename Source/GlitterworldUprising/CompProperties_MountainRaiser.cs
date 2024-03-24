using System.Collections.Generic;
using Verse;

namespace GliterworldUprising
{
    public class CompProperties_MountainRaiser : CompProperties
    {
        public float timeOffset;
        public FleckDef fleck;
        public ThingDef mote;
        public int moteCount = 3;
        public FloatRange moteOffsetRange = new FloatRange(0.2f, 0.4f);
        public SoundDef raiseSound;
        public List<GlitterThingToTurnInto> recipes = new List<GlitterThingToTurnInto>();
        
        public CompProperties_MountainRaiser() => this.compClass = typeof(CompMountainRaiser);
    }

    public class GlitterThingToTurnInto
    {
        public ThingDef item, building;
    }
}
