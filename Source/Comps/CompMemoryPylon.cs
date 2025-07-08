using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace USH_GE;

public class CompProperties_MemoryPylon : CompProperties
{
    public JobDef jobDef;

    public CompProperties_MemoryPylon()
    {
        compClass = typeof(CompMemoryPylon);
    }
}

public class CompMemoryPylon : ThingComp
{
    public CompProperties_MemoryPylon CellProps => (CompProperties_MemoryPylon)props;

    private CompMemoryCellContainer _compContainer;
    private CompGlower _compGlower;
    private CompPowerTrader _compPower;

    public override void PostPostMake()
    {
        base.PostPostMake();

        _compContainer = parent.GetComp<CompMemoryCellContainer>();
        _compContainer.OnInserted += StartWorking;
        _compContainer.OnExtracted += StopWorking;

        _compGlower = parent.GetComp<CompGlower>();
        _compPower = parent.GetComp<CompPowerTrader>();
    }

    public override string CompInspectStringExtra()
    {
        StringBuilder sb = new();

        sb.AppendLine(base.CompInspectStringExtra());

        if (_compContainer.Full)
            sb.AppendLine(_compContainer.ContainedCellComp.MemoryCellData.GetInspectString());

        return sb.ToString().Trim();
    }

    private void StartWorking()
    {
        _compGlower.GlowColor = ColorInt.FromHdrColor(
            MemoryUtils.GetThoughtColor(_compContainer.ContainedCellComp.MemoryCellData.IsPositive()));

        _compPower.PowerOutput = -_compPower.Props.PowerConsumption;
    }

    private void StopWorking()
    {
        _compGlower.GlowColor = ColorInt.FromHdrColor(Color.clear);

        _compPower.PowerOutput = -_compPower.Props.idlePowerDraw;
    }

    public bool IsActive()
    {
        if (_compContainer.ContainedCellComp == null)
            return false;

        return true;
    }
}