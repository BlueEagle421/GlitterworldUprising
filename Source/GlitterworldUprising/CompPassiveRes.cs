using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using System.Text;
using Verse.AI;
using Verse.Sound;

namespace GliterworldUprising
{
    [StaticConstructorOnStartup]
    public class CompPassiveRes : ThingComp
    {
        private int nextProduceRareTick = -1;
        private float researchConducted;
        Map map;

        public CompProperties_PassiveRes Props => (CompProperties_PassiveRes)this.props;


        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            map = this.parent.Map;
        }

        public override void CompTickRare()
        {
            base.CompTickRare();

            int ticksRareGame = Find.TickManager.TicksGame / 250;
            if (this.nextProduceRareTick == -1)
                this.nextProduceRareTick = ticksRareGame + 10;
            else if (ticksRareGame >= this.nextProduceRareTick)
            {
                this.nextProduceRareTick = ticksRareGame + 10;
                if(this.parent.GetComp<CompPowerTrader>() != null)
                {
                    if (this.parent.GetComp<CompPowerTrader>().PowerOn)
                        ConductResearch(this.parent.GetStatValue(StatDef.Named("USH_PassiveResPerDay")) / 24);
                } else
                    ConductResearch(this.parent.GetStatValue(StatDef.Named("USH_PassiveResPerDay")) / 24);

            }
                
        }

        private void ConductResearch(float amount)
        {
            ResearchManager researchManager = Find.ResearchManager;
            if (this.Props.isFacility)
            {
                if (this.parent.GetComp<CompFacility>().LinkedBuildings.Count > 0)
                {
                    if (researchManager.currentProj == null)
                        return;
                    researchManager.ResearchPerformed(amount / 0.00825f, (Pawn)null);
                    researchConducted += amount;
                }
            }
            else
            {
                
                if (researchManager.currentProj == null)
                    return;
                researchManager.ResearchPerformed(amount / 0.00825f, (Pawn)null);
                researchConducted += amount;
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (this != null)
            {
                stringBuilder.Append((string)"USH_GU_ResConducted".Translate() + ": " + (Math.Round(researchConducted)).ToString());
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString().TrimEnd();
        }

    }
}
