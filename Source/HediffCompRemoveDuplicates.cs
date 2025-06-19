using System.Collections.Generic;
using Verse;

namespace GlitterworldUprising
{
    public class HediffCompProperties_RemoveDuplicates : HediffCompProperties
    {
        public List<HediffDef> hediffsConsideredSame = new List<HediffDef>();

        public HediffCompProperties_RemoveDuplicates() => compClass = typeof(HediffCompRemoveDuplicates);
    }

    public class HediffCompRemoveDuplicates : HediffComp
    {
        public HediffCompProperties_RemoveDuplicates Props => (HediffCompProperties_RemoveDuplicates)props;

        public override void CompPostMake()
        {
            base.CompPostMake();

            RemoveHediffDuplicates(parent.pawn, Props.hediffsConsideredSame);
        }

        private void RemoveHediffDuplicates(Pawn pawn, List<HediffDef> duplicates)
        {
            List<Hediff> allHediffs = new List<Hediff>();
            pawn.health.hediffSet.GetHediffs(ref allHediffs);
            foreach (Hediff hediff in allHediffs)
            {
                if (hediff == parent)
                    continue;

                if (duplicates.Contains(hediff.def))
                {
                    GenSpawn.Spawn(hediff.def.spawnThingOnRemoved, pawn.Position, pawn.Map);
                    pawn.health.RemoveHediff(hediff);
                }
            }
        }
    }
}