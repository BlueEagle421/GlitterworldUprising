using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace GlitterworldUprising
{
    public class CompProperties_GlitterworldAnalyzer : CompProperties
    {
        public ThingDef thingDef;
        public float powerPerDayMultiplier;
        public int fuelConsumption;
        public CompProperties_GlitterworldAnalyzer() => compClass = typeof(CompGlitterworldAnalyzer);
    }

    public class CompGlitterworldAnalyzer : ThingComp
    {
        private CompPowerTrader _powerTraderComp;
        private CompRefuelable _refuelableComp;
        private int _analyzingTicksPassed;
        private List<IntVec3> _adjCells;
        private bool _isTryingToProduce;

        private AcceptanceReport _lastProductionReport;

        public CompProperties_GlitterworldAnalyzer Props => (CompProperties_GlitterworldAnalyzer)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            _powerTraderComp = parent.GetComp<CompPowerTrader>();
            _refuelableComp = parent.GetComp<CompRefuelable>();
            _adjCells = GenAdj.CellsAdjacent8Way(parent).ToList();
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);

            _isTryingToProduce = false;
            _analyzingTicksPassed = -1;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref _isTryingToProduce, "USH_IsTryingToProduce", false);
            Scribe_Values.Look(ref _analyzingTicksPassed, "USH_NextProduceTick", -1);
        }

        public override void CompTickRare()
        {
            base.CompTickRare();

            if (!_powerTraderComp.PowerOn)
                return;

            if (_isTryingToProduce)
            {
                TryToProduceThing();
                return;
            }

            _analyzingTicksPassed += 250;

            if (_analyzingTicksPassed >= TicksPerProduction())
            {
                _isTryingToProduce = true;
                TryToProduceThing();
                _analyzingTicksPassed = 0;
            }
        }


        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (!_powerTraderComp.PowerOn)
                return string.Empty;

            stringBuilder.AppendLine("USH_GU_PowerPerProduction".Translate(PowerPerProduction() + " W"));

            if (!_isTryingToProduce)
                stringBuilder.AppendLine("USH_GU_AnalyzerTimeLeft".Translate(Props.thingDef.label, DaysToProduce().ToString()));

            if (_isTryingToProduce && !_lastProductionReport.Accepted)
                stringBuilder.AppendLine("USH_GU_CantProduce".Translate(_lastProductionReport.Reason).Colorize(ColorLibrary.RedReadable));

            return stringBuilder.ToString().TrimEnd();
        }

        private float DaysPerProduction() => parent.GetStatValue(StatDef.Named("USH_AnalyzerDaysPerProduction"));
        private float PowerPerProduction() => parent.GetStatValue(StatDef.Named("USH_AnalyzerPowerPerProduction"));
        private int TicksPerProduction() => (int)DaysPerProduction() * 60000;
        private float DaysToProduce()
        {
            float ticksLeft = TicksPerProduction() - _analyzingTicksPassed;

            if (ticksLeft <= 0)
                return 0;

            return (float)Math.Round(ticksLeft / 60000f * 100f) / 100f;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            if (!DebugSettings.ShowDevGizmos)
                yield break;

            yield return new Command_Action
            {
                defaultLabel = "DEBUG: End analyzing now",
                action = () => _analyzingTicksPassed = TicksPerProduction()
            };
        }

        private bool TryToProduceThing()
        {
            _lastProductionReport = ProductionReport();

            if (!_lastProductionReport.Accepted)
                return false;

            ProduceThing();
            return true;
        }

        private void ProduceThing()
        {
            _refuelableComp.ConsumeFuel(3);

            SpawnDistortionEffect();

            DrawPowerFromNet(PowerPerProduction(), _powerTraderComp.PowerNet);

            SpawnThingAt(Props.thingDef, _adjCells, parent.Map);

            _isTryingToProduce = false;
        }

        private void SpawnDistortionEffect() => FleckMaker.Static(parent.Position, parent.Map, FleckDefOf.PsycastAreaEffect, 1.5f);

        private void DrawPowerFromNet(float power, PowerNet powerNet)
        {
            foreach (CompPowerBattery battery in powerNet.batteryComps)
            {
                if (power >= battery.StoredEnergy)
                {
                    power -= battery.StoredEnergy;
                    battery.DrawPower(battery.StoredEnergy);
                }
                else
                {
                    battery.DrawPower(power);
                    break;
                }
            }
        }

        private void SpawnThingAt(ThingDef thingDef, List<IntVec3> cells, Map map)
        {
            for (int index = 0; index < cells.Count; ++index)
            {
                IntVec3 adjCell = cells[index];

                if (!adjCell.Walkable(map))
                    continue;

                Thing firstThing = adjCell.GetFirstThing(map, thingDef);
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
                    Thing thing = ThingMaker.MakeThing(thingDef);
                    thing.stackCount = 1;
                    if (GenPlace.TryPlaceThing(thing, adjCell, map, ThingPlaceMode.Near))
                        break;
                }
            }
        }

        private bool EnoughPowerStoredInNet(PowerNet powerNet, float powerNeeded)
        {
            if (DebugSettings.unlimitedPower)
                return true;

            return PowerStoredInNet(powerNet) >= powerNeeded;
        }

        private float PowerStoredInNet(PowerNet powerNet)
        {
            float power = 0;

            if (powerNet == null)
                return 0;

            foreach (CompPowerBattery battery in powerNet.batteryComps)
                power += battery.StoredEnergy;

            return power;
        }

        private AcceptanceReport ProductionReport()
        {
            if (_powerTraderComp != null && !_powerTraderComp.PowerOn)
                return "NoPower".Translate();

            if (_powerTraderComp != null && !EnoughPowerStoredInNet(_powerTraderComp.PowerNet, PowerPerProduction()))
                return "USH_GU_NoPowerStored".Translate();

            if (_refuelableComp != null && _refuelableComp.Fuel < Props.fuelConsumption)
                return "NoFuel".Translate();

            return true;
        }
    }
}
