using System.Text;
using Verse;

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
        sb.AppendLine(MemoryCellData.GetInspectString());

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
    public string sourcePawnLabel;
    public int moodOffset;
    public Def thoughtDef;

    public MemoryCellData() { }

    public readonly string GetInspectString()
    {
        StringBuilder sb = new();

        sb.AppendLine("Cloned from: " + sourcePawnLabel);

        string moodFormatted = moodOffset.ToString()
            .Colorize(MemoryUtils.GetThoughtColor(moodOffset > 0));

        sb.AppendLine($"{label} ({moodFormatted})");
        sb.AppendLine("Description: " + description);

        return sb.ToString().Trim();
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref label, nameof(label));
        Scribe_Values.Look(ref description, nameof(description));
        Scribe_Values.Look(ref moodOffset, nameof(moodOffset));
        Scribe_Values.Look(ref sourcePawnLabel, nameof(sourcePawnLabel));
        Scribe_Defs.Look(ref thoughtDef, nameof(thoughtDef));
    }
}