using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace USH_GE
{
    public class MapComponent_RepairManager(Map map) : MapComponent(map)
    {
        private readonly List<CompGlittertechRepairer> repairers = [];
        private readonly HashSet<Building> dirtyBuildings = [];
        private int _tickCounter = 0;

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
        public float repairRadius = 8.9f;
        public float repairInterval = 10;
        public string activeOverlayPath;

        public CompProperties_GlittertechRepairer() => compClass = typeof(CompGlittertechRepairer);
    }


    public class CompGlittertechRepairer : ThingComp
    {
        private HashSet<Thing> _toRepair = [];
        public CompProperties_GlittertechRepairer Props => (CompProperties_GlittertechRepairer)props;
        private MapComponent_RepairManager _manager => parent.Map.GetComponent<MapComponent_RepairManager>();

        private bool IsRepairing => _currentlyRepairing != null && CanRepair();
        private Thing _currentlyRepairing;
        private Effecter _repairEffecter;
        private int _repairTickCounter;

        private const float Y_OFFSET = .018292684f;

        private Color _startColor;
        private Color _targetColor;
        private int _colorTransitionTicks;
        private int _colorTicksElapsed;

        private bool _isFading;
        private const float FADE_DURATION_TICKS = 60f;
        private float _fadeTicks = FADE_DURATION_TICKS;
        private bool _lastIsRepairing = true;

        private CompPowerTrader _powerTrader;
        public CompPowerTrader PowerTrader
        {
            get
            {
                _powerTrader ??= parent.TryGetComp<CompPowerTrader>();

                return _powerTrader;
            }
        }

        private CompStunnable _stunnable;
        public CompStunnable Stunnable
        {
            get
            {
                _stunnable ??= parent.TryGetComp<CompStunnable>();

                return _stunnable;
            }
        }

        private CompGlower _glower;
        public CompGlower Glower
        {
            get
            {
                _glower ??= parent.TryGetComp<CompGlower>();

                return _glower;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_References.Look(ref _currentlyRepairing, "_currentlyRepairing");

            Scribe_Values.Look(ref _repairTickCounter, "_repairTickCounter");
            Scribe_Values.Look(ref _startColor, "_startColor");
            Scribe_Values.Look(ref _targetColor, "_targetColor");
            Scribe_Values.Look(ref _colorTransitionTicks, "_transitionTicks");
            Scribe_Values.Look(ref _colorTicksElapsed, "_ticksElapsed");
            Scribe_Values.Look(ref _isFading, "_isFading");
            Scribe_Values.Look(ref _lastIsRepairing, "_lastIsRepairing");

            Scribe_Collections.Look(ref _toRepair, "_toRepair", LookMode.Reference);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            _manager.Register(this);

            RebuildAll();
            RepairStopped();
            TryToStartRepairing();
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            _manager.Unregister(this);
        }

        private void RebuildAll()
        {
            _toRepair.AddRange(parent.Map.listerBuildingsRepairable.RepairableBuildings(parent.Faction));
        }

        public void HandleFreshlyDamaged(IEnumerable<Building> damaged)
        {
            var radius = Props.repairRadius;
            foreach (var b in damaged)
                if (b.Position.InHorDistOf(parent.Position, radius))
                    _toRepair.Add(b);

            TryToStartRepairing();
        }

        public override void CompTick()
        {
            base.CompTick();

            _repairEffecter?.EffectTick(new TargetInfo(_currentlyRepairing), new TargetInfo(parent));

            PowerOutputTick();

            OverlayFadeTick();

            ColorTransitionTick();

            RepairTick();
        }

        private void PowerOutputTick()
        {
            if (!parent.IsHashIntervalTick(60))
                return;

            var props = PowerTrader.Props;

            float toSet = IsRepairing ? props.PowerConsumption : props.idlePowerDraw;

            PowerTrader.PowerOutput = -toSet;
        }

        private void OverlayFadeTick()
        {
            if (IsRepairing != _lastIsRepairing)
                _fadeTicks = 0f;

            _fadeTicks = Mathf.Min(_fadeTicks + 1f, FADE_DURATION_TICKS);
            _lastIsRepairing = IsRepairing;
        }

        private void ColorTransitionTick()
        {
            if (!_isFading)
                return;

            _colorTicksElapsed++;
            float t = Mathf.Clamp01((float)_colorTicksElapsed / _colorTransitionTicks);
            Color currentColor = Color.Lerp(_startColor, _targetColor, t);
            SetGlowerColor(currentColor);

            if (_colorTicksElapsed >= _colorTransitionTicks)
                _isFading = false;
        }

        private void RepairTick()
        {
            if (!CanRepair())
            {
                RepairStopped();
                return;
            }

            _repairTickCounter++;

            if (_repairTickCounter < Props.repairInterval)
                return;

            if (_currentlyRepairing == null)
                return;

            _repairTickCounter = 0;

            _currentlyRepairing.HitPoints = Mathf.Min(_currentlyRepairing.MaxHitPoints, _currentlyRepairing.HitPoints + 1);

            if (_currentlyRepairing.HitPoints == _currentlyRepairing.MaxHitPoints)
                RepairFinished();
        }

        private void RepairStopped()
        {
            SetGlowerColorSmooth(Color.clear);
            _repairEffecter?.Cleanup();
            _repairEffecter = null;
        }

        private void RepairFinished()
        {
            _toRepair.Remove(_currentlyRepairing);
            _currentlyRepairing = null;

            RepairStopped();

            TryToStartRepairing();
        }

        private bool TryToStartRepairing()
        {
            if (!CanRepair())
                return false;

            if (_toRepair.Count == 0)
                return false;

            if (_currentlyRepairing != null)
                return false;

            StartRepairing();

            return true;
        }

        private void StartRepairing()
        {
            SetGlowerColorSmooth(Color.white);

            _currentlyRepairing = _toRepair.RandomElement();

            _repairEffecter = USHDefOf.USH_GlittertechRepair.Spawn();

            _repairEffecter.Trigger(new TargetInfo(_currentlyRepairing), new TargetInfo(parent), -1);
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new();

            if (_currentlyRepairing != null)
                sb.AppendLine($"Repairing: {_currentlyRepairing.Label}");

            return sb.ToString().TrimEnd();
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);

            DrawPulse(drawLoc);
        }

        private void DrawPulse(Vector3 drawLoc)
        {
            Vector3 loc = drawLoc;
            loc += parent.def.graphicData.drawOffset;
            loc.y += Y_OFFSET;

            float t = _fadeTicks / FADE_DURATION_TICKS;

            float alphaMultiplier = _lastIsRepairing
                ? Mathf.Lerp(0f, 1f, t)
                : Mathf.Lerp(1f, 0f, t);

            Material transparentMat = MaterialPool.MatFrom(Props.activeOverlayPath, ShaderDatabase.Transparent);
            transparentMat.color = new Color(1f, 1f, 1f, alphaMultiplier * Mathf.Abs(Mathf.PingPong(Find.TickManager.TicksGame * 0.02f, 1f)));

            Mesh mesh = parent.Graphic.MeshAt(Rot4.North);
            Quaternion quat = parent.Graphic.QuatFromRot(parent.Rotation);

            Graphics.DrawMesh(mesh, loc, quat, transparentMat, 0);
        }

        public void StartColorFade(Color from, Color to, int durationTicks)
        {
            _startColor = from;
            _targetColor = to;
            _colorTransitionTicks = durationTicks;
            _colorTicksElapsed = 0;
            _isFading = true;
        }

        private AcceptanceReport CanRepair()
        {
            if (!PowerTrader.PowerOn)
                return false;

            if (Stunnable.StunHandler.Stunned)
                return false;

            return true;
        }


        private void SetGlowerColorSmooth(Color color, int durationTicks = 30)
        {
            StartColorFade(Glower.GlowColor.ToColor, color, durationTicks);
        }

        private void SetGlowerColor(Color color)
        {
            Glower.GlowColor = ColorInt.FromHdrColor(color);
        }
    }
}