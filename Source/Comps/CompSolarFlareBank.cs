using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace USH_GE
{
    public class MapComponent_SolarFlareBank(Map map) : MapComponent(map)
    {
        private readonly HashSet<CompSolarFlareBank> _solarBanks = [];
        public List<CompSolarFlareBank> AllAvailableSolarBanks => [.. _solarBanks.Where(x => x.CanInterceptReport())];
        public void Register(CompSolarFlareBank comp) => _solarBanks.Add(comp);
        public void Unregister(CompSolarFlareBank comp) => _solarBanks.Remove(comp);
    }

    public class CompProperties_SolarFlareBank : CompProperties_Power
    {
        public int fuelConsumption = 1;
        public int dischargeTicks = 60000 * 15; // 1 day * 10
        public CompProperties_SolarFlareBank() => compClass = typeof(CompSolarFlareBank);
    }


    [StaticConstructorOnStartup]
    public class CompSolarFlareBank : CompPowerPlant
    {
        public CompProperties_SolarFlareBank BankProps => (CompProperties_SolarFlareBank)props;
        private static readonly Vector2 BarSize = new(1.2f, 0.14f);
        private static readonly Material PowerPlantSolarBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new(0.5f, 0.475f, 0.1f));
        private static readonly Material PowerPlantSolarBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new(0.15f, 0.15f, 0.15f));

        private CompRefuelable _refuelableComp;
        private bool _isDischarging;
        private int _dischargeTicksLeft = 0;
        private const int DISCHARGE_INTERVAL = 2000;
        private const float Z_OFFSET = .018292684f;
        private const float Y_OFFSET = 0.5f;

        public virtual void Notify_SolarFlareIntercepted()
        {
            StartProduction();
        }

        public override void CompTick()
        {
            base.CompTick();

            if (parent.IsHashIntervalTick(DISCHARGE_INTERVAL))
                _dischargeTicksLeft -= DISCHARGE_INTERVAL;

            if (_dischargeTicksLeft <= 0)
                StopGenerating();
        }

        private void StartProduction()
        {
            if (!CanInterceptReport())
                return;

            _isDischarging = true;
            _dischargeTicksLeft = BankProps.dischargeTicks;
        }

        private void StopGenerating()
        {
            _isDischarging = false;
            _dischargeTicksLeft = 0;
        }

        protected override float DesiredPowerOutput
        {
            get
            {
                if (!_isDischarging)
                    return 0;

                return _dischargeTicksLeft / (float)BankProps.dischargeTicks * -Props.PowerConsumption;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            parent.Map.GetComponent<MapComponent_SolarFlareBank>().Register(this);

            _refuelableComp = parent.GetComp<CompRefuelable>();
        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            map.GetComponent<MapComponent_SolarFlareBank>().Unregister(this);
            base.PostDeSpawn(map, mode);
        }

        public override void PostDraw()
        {
            base.PostDraw();

            DrawBar();
        }

        private void DrawBar()
        {
            if (!_isDischarging)
                return;

            GenDraw.FillableBarRequest r = new()
            {
                center = parent.DrawPos + Vector3.up * Z_OFFSET + Vector3.forward * Y_OFFSET,
                size = BarSize,
                fillPercent = PowerOutput / (0f - Props.PowerConsumption),
                filledMat = PowerPlantSolarBarFilledMat,
                unfilledMat = PowerPlantSolarBarUnfilledMat,
                margin = 0.15f
            };
            Rot4 rotation = parent.Rotation;
            rotation.Rotate(RotationDirection.Clockwise);
            r.rotation = rotation;
            GenDraw.DrawFillableBar(r);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref _isDischarging, "_isDischarging");
            Scribe_Values.Look(ref _dischargeTicksLeft, "_dischargeTicksLeft");
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new();

            sb.AppendLine(base.CompInspectStringExtra());

            if (_isDischarging)
                sb.AppendLine("USH_GE_DischargeTimeLeft".Translate(_dischargeTicksLeft.ToStringTicksToPeriod()));

            if (!CanInterceptReport())
                sb.AppendLine("USH_GE_CantIntercept".Translate(CanInterceptReport().Reason).Colorize(Color.red));

            return sb.ToString().TrimEnd();
        }

        public AcceptanceReport CanInterceptReport()
        {
            if (_refuelableComp?.Fuel < BankProps.fuelConsumption)
                return "NoFuel".Translate();

            return true;
        }
    }

}
