using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Verse;

namespace GliterworldUprising
{
    [StaticConstructorOnStartup]
    public class CompGlitterTechnologyAnalyzer : ThingComp
    {
        private CompPowerTrader powerComp;
        private CompRefuelable refuelableComp;
        private int nextProduceTick = -1, ticksPerProduction;
        private List<IntVec3> adjCells;
        private bool netHasPower;


        public CompProperties_GlitterTechnologyAnalyzer Props => (CompProperties_GlitterTechnologyAnalyzer)this.props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            Map map = this.parent.Map;
            this.powerComp = this.parent.GetComp<CompPowerTrader>();
            this.refuelableComp = this.parent.GetComp<CompRefuelable>();
            this.adjCells = GenAdj.CellsAdjacent8Way((Thing)this.parent).ToList<IntVec3>();
            this.CheckForPower(false, false);

        }

        public override void PostDeSpawn(Map map) => this.nextProduceTick = -1;

        public override void CompTick()
        {
            base.CompTick();
            
            ticksPerProduction = (int)this.parent.GetStatValue(StatDef.Named("USH_DaysPerGlitterProduction")) * 60000;
            
            int ticksGame = Find.TickManager.TicksGame;
            if (this.nextProduceTick == -1)
                this.nextProduceTick = ticksGame + ticksPerProduction;
            else if (ticksGame >= this.nextProduceTick)
                this.CheckForPower(true, true);
            if (!powerComp.PowerOn)
                netHasPower = false;
            if (!netHasPower)
            {
                this.nextProduceTick = ticksGame + ticksPerProduction;
                this.CheckForPower(false, false);
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (this != null)
            {
                stringBuilder.Append((string)"USH_GU_PowerPerOperation".Translate() + ": " + this.parent.GetStatValue(StatDef.Named("USH_PowerPerGlitterProduction")).ToString() + " W");
                stringBuilder.AppendLine();

                if (netHasPower)
                    stringBuilder.Append("The next " + this.Props.thing.label + " in " + ((float)Math.Round((nextProduceTick - Find.TickManager.TicksGame) / 60000f * 100f) / 100f).ToString() + " days");
                else
                    stringBuilder.Append((string)"USH_GU_NoPower".Translate());
            }
            return stringBuilder.ToString().TrimEnd();
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
                yield return gizmo;
            
            if (Prefs.DevMode)
            {
                Command_Action commandAction1 = new Command_Action();
                commandAction1.defaultLabel = "DEBUG: Produce now";
                commandAction1.action = (Action)(() => this.nextProduceTick = Find.TickManager.TicksGame);
                Command_Action produce = commandAction1;
                yield return (Gizmo)produce;
                produce = (Command_Action)null;
            }
        }

        private void CheckForPower(bool shouldProduce, bool shouldMessage)
        {
            this.powerComp = this.parent.GetComp<CompPowerTrader>();

            float powerInTheNet = 0;
            if(this.powerComp.PowerNet != null)
            {
                foreach (CompPowerBattery battery in this.powerComp.PowerNet.batteryComps)
                {
                    powerInTheNet += battery.StoredEnergy;
                }
            }
            if (powerInTheNet >= this.parent.GetStatValue(StatDef.Named("USH_PowerPerGlitterProduction")))
            {
                netHasPower = true;
                this.nextProduceTick = Find.TickManager.TicksGame + ticksPerProduction;
                if (shouldProduce)
                {
                    TryProducePortion();
                }
            } else
            {
                if(shouldMessage)
                    Messages.Message((string)"USH_GU_AnalyzerFoundNoPower".Translate(), this.parent, MessageTypeDefOf.NeutralEvent, false);
                netHasPower = false;
            }
        }

        private void TryProducePortion()
        {
            Map map = this.parent.Map;
            if (this.powerComp.PowerOn && this.refuelableComp.Fuel >= this.Props.fuelNeeded)
            {
                //Draw fuel
                this.refuelableComp.ConsumeFuel(3);

                //Draw energy from the net
                float powerToDrain = this.parent.GetStatValue(StatDef.Named("USH_PowerPerGlitterProduction"));
                foreach (CompPowerBattery battery in this.powerComp.PowerNet.batteryComps)
                {
                    if (powerToDrain >= battery.StoredEnergy)
                    {
                        powerToDrain -= battery.StoredEnergy;
                        battery.DrawPower(battery.StoredEnergy);
                    } else
                    {
                        battery.DrawPower(powerToDrain);
                        break;
                    }
                }

                //Spawn the item
                for (int index = 0; index < this.adjCells.Count; ++index)
                {
                    IntVec3 adjCell = this.adjCells[index];
                    if (adjCell.Walkable(map))
                    {
                        Thing firstThing = adjCell.GetFirstThing(map, this.Props.thing);
                        if (firstThing != null)
                        {
                            if (firstThing.stackCount + 1 <= firstThing.def.stackLimit)
                            {
                                firstThing.stackCount += 1;
                                break;
                            }
                        }
                        else
                        {
                            Thing thing = ThingMaker.MakeThing(this.Props.thing);
                            thing.stackCount = 1;
                            if (GenPlace.TryPlaceThing(thing, adjCell, map, ThingPlaceMode.Near))
                                break;
                        }
                    }
                }
            } else
            {
                if (this.refuelableComp.Fuel < this.Props.fuelNeeded)
                {
                    Messages.Message((string)"USH_GU_AnalyzerFoundNoFuel".Translate(), this.parent, MessageTypeDefOf.NeutralEvent, false);
                }
            }
        }

    }
}
