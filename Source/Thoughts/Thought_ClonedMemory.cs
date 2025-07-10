using RimWorld;
using Verse;

namespace USH_GE;

public abstract class Thought_ClonedMemory : Thought_Situational
{
    private MemoryCellData? _cachedMemoryCellData;
    private MemoryCellData? MemoryCellData
    {
        get
        {
            if (_cachedMemoryCellData == null)
            {
                Hediff relevantHediff = pawn.health.hediffSet.GetFirstHediffOfDef(RelevantHediffDef);
                _cachedMemoryCellData = relevantHediff.TryGetComp<HediffCompMemoryCell>().MemoryCellData;
            }

            return _cachedMemoryCellData;
        }
    }

    private float? _cachedClonedMoodOffset;
    private float? ClonedMoodOffset
    {
        get
        {
            _cachedClonedMoodOffset ??= MemoryUtils.MoodOffsetForClonedMemory(pawn, MemoryCellData.Value);
            return _cachedClonedMoodOffset;
        }
    }

    public override float MoodOffset() => ClonedMoodOffset.Value;
    public override string Description => MemoryCellData.Value.GetInspectString();
    protected abstract HediffDef RelevantHediffDef { get; }
}

public class Thought_ClonedMemoryPositive : Thought_ClonedMemory
{
    protected override HediffDef RelevantHediffDef => USH_DefOf.USH_MemoryPositiveHigh;
}

public class Thought_ClonedMemoryNegative : Thought_ClonedMemory
{
    protected override HediffDef RelevantHediffDef => USH_DefOf.USH_MemoryNegativeHigh;
}