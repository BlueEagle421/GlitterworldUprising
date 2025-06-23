using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace GlitterworldUprising
{
    public class MapComponent_RepairManager : MapComponent
    {
        private readonly List<CompGlittertechRepairer> repairers = new List<CompGlittertechRepairer>();
        private readonly HashSet<Building> dirtyBuildings = new HashSet<Building>();
        private int _tickCounter = 0;

        public MapComponent_RepairManager(Map map) : base(map) { }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            foreach (var b in map.listerBuildings.allBuildingsColonist)
                foreach (var comp in b.AllComps)
                    if (comp is CompGlittertechRepairer r)
                        repairers.Add(r);
        }

        public void Register(CompGlittertechRepairer comp) => repairers.Add(comp);
        public void Unregister(CompGlittertechRepairer comp) => repairers.Remove(comp);
        public void NotifyDamaged(Building b) => dirtyBuildings.Add(b);

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            _tickCounter++;
            if (_tickCounter >= 250)
            {
                _tickCounter = 0;
                ProcessDamagedBuildings();
            }
        }

        private void ProcessDamagedBuildings()
        {
            if (dirtyBuildings.Count == 0)
                return;

            foreach (var comp in repairers)
                comp.HandleFreshlyDamaged(dirtyBuildings);

            dirtyBuildings.Clear();
        }
    }

    public class CompProperties_GlittertechRepairer : CompProperties
    {
        public float repairRadius = 10;

        public CompProperties_GlittertechRepairer() => compClass = typeof(CompGlittertechRepairer);
    }


    public class CompGlittertechRepairer : ThingComp
    {
        private readonly HashSet<Thing> _toRepair = new HashSet<Thing>();
        public CompProperties_GlittertechRepairer Props => (CompProperties_GlittertechRepairer)props;
        private MapComponent_RepairManager _manager => parent.Map.GetComponent<MapComponent_RepairManager>();

        private Thing _currentlyRepairing;
        private Effecter _repairEffecter;

        private int _tickCounter;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            _manager.Register(this);
            RebuildAll();
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            _manager.Unregister(this);
        }

        private void RebuildAll()
        {
            _toRepair.Clear();
            _toRepair.AddRange(parent.Map.listerBuildingsRepairable.RepairableBuildings(parent.Faction));
        }

        public void HandleFreshlyDamaged(IEnumerable<Building> damaged)
        {
            var radius = Props.repairRadius;
            foreach (var b in damaged)
                if (b.Position.InHorDistOf(parent.Position, radius))
                    _toRepair.Add(b);


            StartRepairing();
        }

        public override void CompTick()
        {
            base.CompTick();

            _repairEffecter?.EffectTick(new TargetInfo(_currentlyRepairing), new TargetInfo(parent));

            _tickCounter++;
            if (_tickCounter < 10)
                return;

            if (_currentlyRepairing == null)
                return;

            _tickCounter = 0;

            _currentlyRepairing.HitPoints = Mathf.Min(_currentlyRepairing.MaxHitPoints, _currentlyRepairing.HitPoints + 1);

            if (_currentlyRepairing.HitPoints == _currentlyRepairing.MaxHitPoints)
            {
                _toRepair.Remove(_currentlyRepairing);
                _currentlyRepairing = null;
                _repairEffecter = null;
                StartRepairing();
            }
        }

        private void StartRepairing()
        {
            if (_toRepair.Count == 0)
                return;

            if (_currentlyRepairing != null)
                return;

            _currentlyRepairing = _toRepair.RandomElement();

            _repairEffecter = _currentlyRepairing.def.repairEffect.Spawn();

            _repairEffecter.Trigger(new TargetInfo(_currentlyRepairing), new TargetInfo(parent), -1);
        }

        public override string CompInspectStringExtra()
        {
            return string.Join(",", _toRepair);
        }

    }



}




