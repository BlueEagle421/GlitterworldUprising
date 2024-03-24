using Verse;
using UnityEngine;

namespace GliterworldUprising
{
    public class CompProperties_DesensitizingModule : CompProperties
    {
        public ActivateGizmo activateGizmo;
        public int fuelPerDesensitization;
        public FleckDef fleck;
        public ThingDef mote;
        public int moteCount = 3;
        public FloatRange moteOffsetRange = new FloatRange(0.2f, 0.4f);
        public CompProperties_DesensitizingModule() => this.compClass = typeof(Comp_DesensitizingModule);
    }

    public class ActivateGizmo
    {
        public Texture2D tex;
        public string texPath;
        public string labelKey;
        public string descKey;
    }

}
