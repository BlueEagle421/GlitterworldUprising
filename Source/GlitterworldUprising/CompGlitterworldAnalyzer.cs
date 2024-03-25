using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace GliterworldUprising
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
        private int _nextProduceTick;
        private List<IntVec3> _adjCells;
        private bool _isAnalyzing;

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

            _isAnalyzing = true;
            _nextProduceTick = -1;
        }

        public override void PostDeSpawn(Map map) => _nextProduceTick = -1;

        public override void CompTickRare()
        {
            base.CompTickRare();

            if (!_powerTraderComp.PowerOn)
                return;

            if (!_isAnalyzing)
            {
                TryProducePortion();
                return;
            }

            int ticksGame = Find.TickManager.TicksGame;

            if (_nextProduceTick == -1)
            {
                _nextProduceTick = ticksGame + TicksPerProduction();
                return;
            }

            if (ticksGame >= _nextProduceTick)
            {
                TryProducePortion();
                _isAnalyzing = false;
                _nextProduceTick = ticksGame + TicksPerProduction();
            }
        }

        private int TicksPerProduction() => (int)parent.GetStatValue(StatDef.Named("USH_AnalyzerDaysPerProduction")) * 60000;

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (!_powerTraderComp.PowerOn)
                return string.Empty;

            stringBuilder.AppendLine("USH_GU_PowerPerProduction".Translate(PowerPerProduction() + " W"));

            if (_isAnalyzing)
                stringBuilder.AppendLine("USH_GU_AnalyzerTimeLeft".Translate(Props.thingDef.label, DaysToProduce().ToString()));

            if (!_isAnalyzing && !_lastProductionReport.Accepted)
                stringBuilder.AppendLine("USH_GU_CantProduce".Translate(_lastProductionReport.Reason).Colorize(ColorLibrary.RedReadable));

            return stringBuilder.ToString().TrimEnd();
        }

        private float PowerPerProduction() => parent.GetStatValue(StatDef.Named("USH_AnalyzerPowerPerProduction"));

        private float DaysToProduce()
        {
            float ticksLeft = _nextProduceTick - Find.TickManager.TicksGame;
            return (float)Math.Round(ticksLeft / 60000f * 100f) / 100f;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            if (Prefs.DevMode)
            {
                Command_Action commandAction1 = new Command_Action();
                commandAction1.defaultLabel = "DEBUG: Produce now";
                commandAction1.action = () => _nextProduceTick = Find.TickManager.TicksGame;
                Command_Action produce = commandAction1;
                yield return produce;
                produce = null;
            }
        }

        private void TryProducePortion()
        {
            _lastProductionReport = ProductionReport();

            if (!_lastProductionReport.Accepted)
                return;

            _refuelableComp.ConsumeFuel(3);

            DrawPowerFromNet(PowerPerProduction(), _powerTraderComp.PowerNet);

            SpawnThingAt(Props.thingDef, _adjCells, parent.Map);

            _isAnalyzing = true;
        }

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

        private float PowerInNet(PowerNet powerNet)
        {
            float power = 0;

            if (_powerTraderComp.PowerNet != null)
                foreach (CompPowerBattery battery in _powerTraderComp.PowerNet.batteryComps)
                    power += battery.StoredEnergy;

            return power;
        }

        private AcceptanceReport ProductionReport()
        {
            if (_powerTraderComp != null && !_powerTraderComp.PowerOn)
                return "NoPower".Translate();

            if (_powerTraderComp != null && PowerInNet(_powerTraderComp.PowerNet) < PowerPerProduction())
                return "Not enough power stored";

            if (_refuelableComp != null && _refuelableComp.Fuel < Props.fuelConsumption)
                return "NoFuel".Translate();

            return true;
        }
    }
}
