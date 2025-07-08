using Verse;

namespace USH_GE;

public class HediffCompProperties_MemoryCell : HediffCompProperties
{
    public HediffCompProperties_MemoryCell() => compClass = typeof(HediffCompMemoryCell);
}

public class HediffCompMemoryCell : HediffComp
{
    public MemoryCellData MemoryCellData;

    public override void CompExposeData()
    {
        base.CompExposeData();

        Scribe_Deep.Look(ref MemoryCellData, nameof(MemoryCellData));
    }

}
