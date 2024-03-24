using Verse;

namespace GliterworldUprising
{
    public class CompProperties_MobileComforter : CompProperties
    {
        public float joyGain;
        public int doseOffset;
        public SoundDef doseSound;
        public FleckDef fleck;
        public ThingDef mote;
        public int moteCount = 3;
        public FloatRange moteOffsetRange = new FloatRange(0.2f, 0.4f);
        public CompProperties_MobileComforter() => this.compClass = typeof(CompMobileComforter);
    }
}
