
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace USH_GE;

public class IngestionOutcomeDoer_MemoryCell : IngestionOutcomeDoer
{


    protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
    {
        if (pawn.needs == null)
            return;



    }
}