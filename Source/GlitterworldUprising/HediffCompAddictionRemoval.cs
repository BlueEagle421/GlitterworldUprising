﻿using RimWorld;
using System.Collections.Generic;
using Verse;

namespace GliterworldUprising
{
    public class HediffCompProperties_AddictionRemoval : HediffCompProperties
    {
        public List<HediffDef> hediffDefBlackList = new List<HediffDef>();

        public HediffCompProperties_AddictionRemoval() => compClass = typeof(HediffCompAddictionRemoval);
    }

    public class HediffCompAddictionRemoval : HediffComp
    {
        public HediffCompProperties_AddictionRemoval Props => (HediffCompProperties_AddictionRemoval)props;

        public override void CompPostMake()
        {
            base.CompPostMake();
            RemoveAddictions();
        }

        private void RemoveAddictions()
        {
            List<Hediff> allHediffs = new List<Hediff>();
            parent.pawn.health.hediffSet.GetHediffs(ref allHediffs);

            foreach (Hediff hediff in allHediffs)
            {
                if (hediff.def.hediffClass != typeof(Hediff_Addiction))
                    continue;

                if (Props.hediffDefBlackList.Contains(hediff.def))
                    continue;

                parent.pawn.health.RemoveHediff(hediff);
            }
        }
    }
}