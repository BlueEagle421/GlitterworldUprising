using Verse;

namespace USH_GE;

public class HediffCompProperties_MemoryCell : HediffCompProperties
{
    public HediffCompProperties_MemoryCell() => compClass = typeof(HediffCompMemoryCell);
}

public class HediffCompMemoryCell : HediffComp
{
    public MemoryCellData MemoryCellData;

}
