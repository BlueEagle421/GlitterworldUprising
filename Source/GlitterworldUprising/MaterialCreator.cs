using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace GliterworldUprising
{
    [StaticConstructorOnStartup]
    public class MaterialCreator
    {

        static MaterialCreator()
        {
            List<ThingDef> defsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;
            for (int index = 0; index < defsListForReading.Count; ++index)
            {
                CompProperties_DesensitizingModule compProperties = defsListForReading[index].GetCompProperties<CompProperties_DesensitizingModule>();
                if (compProperties != null && compProperties.activateGizmo != null)
                    compProperties.activateGizmo.tex = ContentFinder<Texture2D>.Get(compProperties.activateGizmo.texPath);
            }
        }
    }
}
