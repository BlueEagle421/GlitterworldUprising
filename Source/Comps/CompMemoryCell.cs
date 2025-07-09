using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
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

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            yield return gizmo;

        Command_Action eraseCommand = new()
        {
            action = EraseMemory,
            defaultLabel = "USH_GE_CommandEraseMemory".Translate(),
            defaultDesc = "USH_GE_CommandEraseMemoryDesc".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Gizmos/EraseMemory")
        };
        yield return eraseCommand;
    }

    private void EraseMemory()
    {
        IntVec3 pos = parent.Position;
        Map map = parent.Map;
        parent.Destroy();

        var newThing = ThingMaker.MakeThing(USH_DefOf.USH_MemoryCellEmpty);

        GenPlace.TryPlaceThing(newThing, pos, map, ThingPlaceMode.Near);

        Find.Selector.Select(newThing, playSound: false, forceDesignatorDeselect: false);
    }

    public override void PostExposeData()
    {
        base.PostExposeData();

        Scribe_Deep.Look(ref MemoryCellData, nameof(MemoryCellData));
    }
}

public struct MemoryCellData : IExposable
{
    public string label;
    public string description;
    public string sourcePawnLabel;
    public int moodOffset;
    public ThoughtDef thoughtDef;

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