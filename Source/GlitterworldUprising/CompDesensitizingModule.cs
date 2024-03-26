using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.Sound;

namespace GliterworldUprising
{
    public class CompProperties_DesensitizingModule : CompProperties_Activable
    {
        public int fuelConsumption;
        public HediffDef hediffDefToRemove;
        public FleckDef fleckDef;
        public SoundDef soundDef;

        public CompProperties_DesensitizingModule() => compClass = typeof(CompDesensitizingModule);
    }

    public class CompDesensitizingModule : CompActivable
    {
        Map _currentMap;
        CompFacility _facilityComp;
        CompRefuelable _refuelableComp;

        public CompProperties_DesensitizingModule Props => (CompProperties_DesensitizingModule)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            _currentMap = parent.Map;
            _facilityComp = parent.GetComp<CompFacility>();
            _refuelableComp = parent.GetComp<CompRefuelable>();
        }

        public override void Activate()
        {
            base.Activate();

            DesensitizePawns();
        }
        protected override bool TryUse()
        {
            if (_refuelableComp.Fuel < Props.fuelConsumption)
                return false;

            return true;
        }

        private void DesensitizePawns()
        {
            foreach (Pawn pawn in PawnsInLinkedFacilities())
            {
                Hediff toRemove = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDefToRemove);

                if (toRemove == null)
                    continue;

                pawn.health.RemoveHediff(toRemove);
                _refuelableComp.ConsumeFuel(Props.fuelConsumption);

                PlaySoundEffect();

                SpawnFleckEffect(pawn.Position);
                SpawnFleckEffect(parent.Position);
            }
        }

        private void SpawnFleckEffect(IntVec3 position) => FleckMaker.Static(position, _currentMap, Props.fleckDef);

        private void PlaySoundEffect() => Props.soundDef.PlayOneShot(new TargetInfo(parent.Position, parent.Map, false));

        private List<Pawn> PawnsInLinkedFacilities()
        {
            List<Pawn> result = new List<Pawn>();

            foreach (Thing building in _facilityComp.LinkedBuildings)
                foreach (Thing thing in _currentMap.thingGrid.ThingsAt(building.Position))
                    if (thing as Pawn != null)
                        result.Add(thing as Pawn);

            return result;
        }

        public override AcceptanceReport CanActivate(Pawn activateBy = null)
        {
            if (_refuelableComp.Fuel < Props.fuelConsumption)
                return "NoFuel".Translate();

            return base.CanActivate(activateBy);
        }

        public override string CompInspectStringExtra() => "USH_GU_DesensitizeCost".Translate(Props.fuelConsumption);
    }
}
