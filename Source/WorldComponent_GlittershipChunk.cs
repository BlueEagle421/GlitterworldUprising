using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;

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
    private const int THREAT_POINTS = 600;

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

        if (!EventSpawnChunk(out IntVec3 _))
            return;

        _didEvent = true;
    }

    public bool EventSpawnChunk(out IntVec3 spawnPos)
    {
        Map map = Find.AnyPlayerHomeMap;
        spawnPos = IntVec3.Zero;

        if (map == null)
            return false;

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

        Thing shipChunk = ThingMaker.MakeThing(USH_DefOf.USH_GlittershipChunk);
        spawnPos = pos;
        SpawnChunk(pos, shipChunk, map);

        float points = THREAT_POINTS;
        List<Pawn> mechanoids = [.. PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
        {
            groupKind = PawnGroupKindDefOf.Combat,
            tile = map.Tile,
            faction = Faction.OfMechanoids,
            points = points
        })];

        mechanoids.ForEach(x => x.TryGetComp<CompCanBeDormant>()?.ToSleep());

        shipChunk.SetFaction(Faction.OfMechanoids);
        LordMaker.MakeNewLord(Faction.OfMechanoids, new LordJob_SleepThenMechanoidsDefend([shipChunk], Faction.OfMechanoids, 28f, pos, canAssaultColony: false, isMechCluster: false), map, mechanoids);
        DropPodUtility.DropThingsNear(pos, map, mechanoids.Cast<Thing>());

        string label = "USH_GE_LetterLabelGlittershipChunk".Translate();
        string desc = "USH_GE_LetterGlittershipChunk".Translate();
        Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter(label, desc, LetterDefOf.NegativeEvent, new LookTargets(spawnPos, map)));

        return true;
    }

    private List<Thing> FindThingsOfDef(List<ThingDef> defs)
    {
        var listerThings = Find.CurrentMap.listerThings;
        List<Thing> result = [];

        defs.ForEach(x => result.AddRange(listerThings.ThingsOfDef(x)));
        return result;
    }


    private void SpawnChunk(IntVec3 pos, Thing innerThing, Map map)
    {
        ThingDef skyfaller = USH_DefOf.USH_GlittershipChunkIncoming;

        SkyfallerMaker.SpawnSkyfaller(skyfaller, innerThing, pos, map);
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