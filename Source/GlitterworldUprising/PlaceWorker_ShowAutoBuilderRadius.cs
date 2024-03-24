using UnityEngine;
using Verse;

namespace GliterworldUprising
{
    public class PlaceWorker_ShowAutoBuilderRadius : PlaceWorker
    {
        public override void DrawGhost(
          ThingDef def,
          IntVec3 center,
          Rot4 rot,
          Color ghostCol,
          Thing thing = null)
        {
            CompProperties_AutoBuilder compProperties = def.GetCompProperties<CompProperties_AutoBuilder>();
            if (compProperties == null)
                return;
            GenDraw.DrawRadiusRing(center, compProperties.radius);
        }
    }
}
