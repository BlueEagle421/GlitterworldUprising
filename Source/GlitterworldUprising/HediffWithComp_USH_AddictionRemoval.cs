using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace GliterworldUprising
{
    public class HediffComp_USH_AddictionRemoval : HediffComp
    {

        public HediffCompProperties_USH_AddictionRemoval Props => (HediffCompProperties_USH_AddictionRemoval)this.props;

        public override void CompPostMake()
        {
            base.CompPostMake();
            foreach (Hediff hediff in this.parent.pawn.health.hediffSet.GetHediffs<Hediff>())
            {
                if (hediff.def.hediffClass == typeof(Hediff_Addiction))
                {
                    if (Props.removalBlackList.Contains(hediff.def.defName))
                    {
                        continue;
                    }
                    this.parent.pawn.health.RemoveHediff(hediff);
                }
            }
        }
    }
}