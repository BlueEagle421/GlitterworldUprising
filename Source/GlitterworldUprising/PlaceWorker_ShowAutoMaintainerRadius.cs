using UnityEngine;
using Verse;

namespace GliterworldUprising
{
    public class PlaceWorker_ShowAutoMaintainerRadius : PlaceWorker
    {
        public override void DrawGhost(
          ThingDef def,
          IntVec3 center,
          Rot4 rot,
          Color ghostCol,
          Thing thing = null)
        {
            CompProperties_AutoMaintainer compProperties = def.GetCompProperties<CompProperties_AutoMaintainer>();
            if (compProperties == null)
                return;
            GenDraw.DrawRadiusRing(center, compProperties.radius);
        }
    }
}
