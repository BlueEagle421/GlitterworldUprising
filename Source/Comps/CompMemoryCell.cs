using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace USH_GE;

public class CompProperties_MemoryCell : CompProperties
{
    public JobDef jobDef;

    public CompProperties_MemoryCell()
    {
        compClass = typeof(CompMemoryCell);
    }
}

public class CompMemoryCell : ThingComp
{
    public MemoryCellData MemoryCellData;
    public CompProperties_MemoryCell CellProps => (CompProperties_MemoryCell)props;

    public override string CompInspectStringExtra()
    {
        StringBuilder sb = new();
        sb.AppendLine(base.CompInspectStringExtra());

        sb.AppendLine("Cloned from: " + MemoryCellData.sourcePawnLabel);
        sb.AppendLine($"{MemoryCellData.label} ({MemoryCellData.moodOffset})");
        sb.AppendLine("Description: " + MemoryCellData.description);

        return sb.ToString().Trim();
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
    }

}

public struct MemoryCellData : IExposable
{
    public string label;
    public string description;
    public int moodOffset;
    public string sourcePawnLabel;
    public Def thoughtDef;

    public MemoryCellData() { }

    public void ExposeData()
    {
        Scribe_Values.Look(ref label, nameof(label));
        Scribe_Values.Look(ref description, nameof(description));
        Scribe_Values.Look(ref moodOffset, nameof(moodOffset));
        Scribe_Values.Look(ref sourcePawnLabel, nameof(sourcePawnLabel));
        Scribe_Defs.Look(ref thoughtDef, nameof(thoughtDef));
    }

}