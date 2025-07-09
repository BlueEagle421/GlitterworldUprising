using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace USH_GE;

public class CompProperties_MemoryPylon : CompProperties
{
    public JobDef jobDef;

    public CompProperties_MemoryPylon()
    {
        compClass = typeof(CompMemoryPylon);
    }
}

public class CompMemoryPylon : ThingComp
{
    public CompProperties_MemoryPylon CellProps => (CompProperties_MemoryPylon)props;

    private CompMemoryCellContainer _compContainer;
    private CompGlower _compGlower;
    private CompPowerTrader _compPower;

    private bool _isWorkingNow;

    private float WorkRadius => parent.def.specialDisplayRadius;
    private float _cachedRadiusSquared = -1;
    private float WorkRadiusSquared
    {
        get
        {
            if (_cachedRadiusSquared == -1)
                _cachedRadiusSquared = WorkRadius * WorkRadius;

            return _cachedRadiusSquared;
        }
    }

    private const int TICK_CHECK_INTERVAL = 60;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);

        _compContainer = parent.GetComp<CompMemoryCellContainer>();
        _compContainer.OnInserted += StartWorking;
        _compContainer.OnExtracted += StopWorking;

        _compGlower = parent.GetComp<CompGlower>();

        _compPower = parent.GetComp<CompPowerTrader>();
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        _compContainer.OnInserted -= StartWorking;
        _compContainer.OnExtracted -= StopWorking;
        RemoveAllPylonMemories();

        base.PostDeSpawn(map, mode);
    }

    public override string CompInspectStringExtra()
    {
        StringBuilder sb = new();

        sb.AppendLine(base.CompInspectStringExtra());

        if (_compContainer.Full)
            sb.AppendLine(_compContainer.ContainedCellComp.MemoryCellData.GetInspectString());

        return sb.ToString().Trim();
    }

    private void StartWorking()
    {
        if (_isWorkingNow)
            return;

        if (!CanWork())
            return;

        _compGlower.GlowColor = ColorInt.FromHdrColor(
            MemoryUtils.GetThoughtColor(_compContainer.ContainedCellComp.MemoryCellData.IsPositive()));

        _compPower.PowerOutput = -_compPower.Props.PowerConsumption;

        CreatePylonMemoriesInRadius();

        _isWorkingNow = true;
    }

    private void StopWorking()
    {
        if (!_isWorkingNow)
            return;

        _compGlower.GlowColor = ColorInt.FromHdrColor(Color.clear);

        _compPower.PowerOutput = -_compPower.Props.idlePowerDraw;

        RemoveAllPylonMemories();

        _isWorkingNow = false;
    }

    public override void CompTick()
    {
        base.CompTick();

        if (Find.TickManager.TicksGame % TICK_CHECK_INTERVAL != 0)
            return;

        if (!CanWork())
        {
            if (_isWorkingNow)
                StopWorking();

            return;
        }
        else if (!_isWorkingNow)
            StartWorking();

        CreatePylonMemoriesInRadius();
    }

    private void CreatePylonMemoriesInRadius()
    {
        foreach (var p in parent.Map.mapPawns.FreeColonistsSpawned)
        {
            float dist = p.Position.DistanceToSquared(parent.Position);

            if (dist <= WorkRadiusSquared)
            {
                var mems = p.needs.mood.thoughts.memories;
                if (!mems.Memories.Any(IsMemoryFromHere))
                    CreatePylonMemory(p);
            }
            else
                RemovePylonMemory(p);
        }
    }

    private void CreatePylonMemory(Pawn p)
    {
        Thought_MemoryPylon toGive = ThoughtMaker.MakeThought(USH_DefOf.USH_MemoryPylonThought) as Thought_MemoryPylon;
        toGive.MemoryCellData = _compContainer.ContainedCellComp.MemoryCellData;
        toGive.SourceThing = parent;

        p.needs.mood.thoughts.memories.TryGainMemory(toGive);
    }

    private void RemovePylonMemory(Pawn p)
    {
        var mems = p.needs.mood.thoughts.memories;
        Thought_Memory toRemove = mems.Memories.Find(IsMemoryFromHere);

        if (toRemove != null)
            mems.RemoveMemory(toRemove);
    }

    private void RemoveAllPylonMemories()
    {
        parent.Map.mapPawns.FreeColonistsSpawned.ForEach(RemovePylonMemory);
    }

    private bool IsMemoryFromHere(Thought_Memory m)
    {
        if (m is not Thought_MemoryPylon thoughtPylon)
            return false;

        if (thoughtPylon.SourceCompPylon != this)
            return false;

        return true;
    }

    private bool CanWork()
    {
        if (_compContainer.ContainedCellComp == null)
            return false;

        if (!_compPower.PowerOn)
            return false;

        return true;
    }
}