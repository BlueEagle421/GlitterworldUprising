using RimWorld;
using System.Collections.Generic;
using Verse;

namespace GliterworldUprising
{
    public class HediffCompProperties_USH_AddictionRemoval : HediffCompProperties
    {
        public List<string> removalBlackList = new List<string>();

        public HediffCompProperties_USH_AddictionRemoval() => this.compClass = typeof(HediffWithCompsAddictionRemoval);
    }

    public class HediffWithCompsAddictionRemoval : HediffComp
    {

        public HediffCompProperties_USH_AddictionRemoval Props => (HediffCompProperties_USH_AddictionRemoval)this.props;

        public override void CompPostMake()
        {
            base.CompPostMake();
            List<Hediff> allHediffs = new List<Hediff>();
            this.parent.pawn.health.hediffSet.GetHediffs(ref allHediffs);
            foreach (Hediff hediff in allHediffs)
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