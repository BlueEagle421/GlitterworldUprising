using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace USH_GE;

public class WorldComponent_GlittershipChunk : WorldComponent
{
    public static WorldComponent_GlittershipChunk Instance { get; private set; }
    public WorldComponent_GlittershipChunk(World world) : base(world)
    {
        Instance = this;
        _ticksToFire = _tickDelayRange.RandomInRange;
    }

    private IntRange _tickDelayRange = new(2500, 2500 * 3); //1 hour, 3 hours
    private int _ticksToFire;
    private int _ticksPassed;
    private bool _didEvent;

    private const int TICK_CHECK_INTERVAL = 250;

    public override void WorldComponentTick()
    {
        base.WorldComponentTick();

        if (_didEvent)
            return;

        _ticksPassed++;
        if (_ticksPassed % TICK_CHECK_INTERVAL != 0)
            return;

        if (!USH_DefOf.USH_GlittertechFabrication.IsFinished)
            return;

        _ticksToFire -= TICK_CHECK_INTERVAL;

        if (_ticksToFire <= 0)
            FireEvent();
    }

    private void FireEvent()
    {
        Map map = Find.AnyPlayerHomeMap;

        if (map == null)
            return;

        EventSpawnChunk();

        string label = "USH_GE_LetterLabelGlittershipChunk".Translate();
        string desc = "USH_GE_LetterGlittershipChunk".Translate();
        Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter(label, desc, LetterDefOf.PositiveEvent));

        _didEvent = true;
    }

    public void EventSpawnChunk()
    {
        Map map = Find.AnyPlayerHomeMap;

        if (map == null)
            return;

        if (!TryFindShipChunkDropCell(map.Center, map, 999999, out var pos))
        {
            List<ThingDef> potentialSpawnDefs =
            [
                ThingDefOf.Bed,
                ThingDefOf.SimpleResearchBench,
                USH_DefOf.HiTechResearchBench,
                USH_DefOf.USH_ResearchProbe,
            ];

            pos = FindThingsOfDef(potentialSpawnDefs).First().Position;
        }

        SpawnChunk(pos, map);
    }

    private List<Thing> FindThingsOfDef(List<ThingDef> defs)
    {
        var listerThings = Find.CurrentMap.listerThings;
        List<Thing> result = [];

        defs.ForEach(x => result.AddRange(listerThings.ThingsOfDef(x)));
        return result;
    }


    private void SpawnChunk(IntVec3 pos, Map map)
    {
        ThingDef skyfaller = USH_DefOf.USH_GlittershipChunkIncoming;
        ThingDef chunk = USH_DefOf.USH_GlittershipChunk;

        SkyfallerMaker.SpawnSkyfaller(skyfaller, chunk, pos, map);
    }

    private static bool TryFindShipChunkDropCell(IntVec3 nearLoc, Map map, int maxDist, out IntVec3 pos)
    {
        return CellFinderLoose.TryFindSkyfallerCell(
            ThingDefOf.ShipChunkIncoming,
            map,
            ThingDefOf.ShipChunk.terrainAffordanceNeeded,
            out pos,
            10,
            nearLoc,
            maxDist);
    }

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref _didEvent, nameof(_didEvent));
    }
}