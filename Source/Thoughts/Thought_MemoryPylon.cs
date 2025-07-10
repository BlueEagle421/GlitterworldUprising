using RimWorld;
using Verse;

namespace USH_GE;

public class Thought_MemoryPylon : Thought_Memory
{
    private CompMemoryPylon _sourceCompPylon;

    public CompMemoryPylon SourceCompPylon
    {
        get
        {
            _sourceCompPylon ??= SourceThing.TryGetComp<CompMemoryPylon>();
            return _sourceCompPylon;
        }
    }

    public ThingWithComps SourceThing;
    public MemoryCellData MemoryCellData;

    private float? _cachedClonedMoodOffset;
    private float? PylonMoodOffset
    {
        get
        {
            _cachedClonedMoodOffset ??= MemoryUtils.MoodOffsetForClonedMemory(pawn, MemoryCellData) * 0.5f;
            return _cachedClonedMoodOffset;
        }
    }
    public override string Description => def.stages[0].description;
    public override float MoodOffset() => PylonMoodOffset.Value;
    public override bool TryMergeWithExistingMemory(out bool showBubble)
    {
        showBubble = false;
        return false;
    }

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Deep.Look(ref MemoryCellData, nameof(MemoryCellData));
        Scribe_References.Look(ref SourceThing, nameof(SourceThing));
    }
}