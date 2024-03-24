using UnityEngine;
using Verse;

namespace GliterworldUprising
{
    public class PlaceWorker_ShowAutoRepairerRadius : PlaceWorker
    {
        public override void DrawGhost(
          ThingDef def,
          IntVec3 center,
          Rot4 rot,
          Color ghostCol,
          Thing thing = null)
        {
            CompProperties_AutoRepairer compProperties = def.GetCompProperties<CompProperties_AutoRepairer>();
            if (compProperties == null)
                return;
            GenDraw.DrawRadiusRing(center, compProperties.radius);
        }
    }
}
