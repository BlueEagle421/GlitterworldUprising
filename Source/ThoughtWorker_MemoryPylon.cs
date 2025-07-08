using System.Collections.Generic;
using RimWorld;
using Verse;

namespace USH_GE;

public class ThoughtWorker_MemoryPylon : ThoughtWorker
{
    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        if (!p.Spawned)
            return false;

        List<Thing> list = p.Map.listerThings.ThingsOfDef(MemoryPylonDef);

        return list.Any(x => IsMemoryPylonAvailable(p, x));
    }

    private bool IsMemoryPylonAvailable(Pawn p, Thing t)
    {
        CompPowerTrader compPowerTrader = t.TryGetComp<CompPowerTrader>();

        if (compPowerTrader != null && !compPowerTrader.PowerOn)
            return false;

        if (p.Position.InHorDistOf(t.Position, MemoryPylonDef.specialDisplayRadius))
            return false;

        return true;
    }

    private ThingDef MemoryPylonDef => USH_DefOf.USH_MemoryPylon;
}