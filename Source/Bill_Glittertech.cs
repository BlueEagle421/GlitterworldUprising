using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Verse.AI;
using System.Collections.Generic;

namespace USH_GE;

public class ModExtension_UseGlittertechBill : DefModExtension
{
    public int powerNeeded;
    public int fuelNeeded;
    public float analyzerOffsetY = 0.7f;
    public List<ThingDef> requiredFacilities;
}

public class Bill_Glittertech : Bill_Autonomous
{
    private Pawn boundPawn;
    private int gestationCycles;
    public Pawn BoundPawn => boundPawn;
    public int GestationCyclesCompleted => gestationCycles;
    public Building_GlittertechFabricator Fabricator => (Building_GlittertechFabricator)billStack.billGiver;
    public ModExtension_UseGlittertechBill GlittertechExt;

    protected override Color BaseColor
    {
        get
        {
            if (suspended)
                return base.BaseColor;

            return Color.white;
        }
    }

    protected override string StatusString
    {
        get
        {
            switch (State)
            {
                case FormingState.Gathering:
                    break;

                case FormingState.Preparing:
                    return "USH_GE_WaitingForMaintenance".Translate();

                case FormingState.Forming:
                    return "USH_GE_Forming".Translate();

                case FormingState.Formed:
                    return "USH_GE_WaitingForCompletion".Translate();
            }
            return null;
        }
    }

    public Bill_Glittertech() { }

    public Bill_Glittertech(RecipeDef recipe, Precept_ThingStyle precept = null) : base(recipe, precept)
    {
        GlittertechExt = recipe.GetModExtension<ModExtension_UseGlittertechBill>();
    }

    public override void Notify_DoBillStarted(Pawn billDoer)
    {
        base.Notify_DoBillStarted(billDoer);

        if (boundPawn != billDoer)
            boundPawn = billDoer;
    }


    public override void Reset()
    {
        base.Reset();
        gestationCycles = 0;
        boundPawn = null;
    }


    public void ForceCompleteAllCycles()
    {
        gestationCycles = recipe.gestationCycles;
        formingTicks = 0f;
    }

    public float FormingSpeedMultiplier()
    {
        float statValue = Fabricator.GetStatValue(USH_DefOf.USH_GlittertechDuration);
        float settingValue = GE_Mod.Settings.FormingSpeedMultiplier.Value;
        return 1f / (statValue * settingValue);
    }

    public override void BillTick()
    {
        if (suspended || state != FormingState.Forming)
            return;

        formingTicks -= 1f * FormingSpeedMultiplier();

        if (formingTicks > 0f)
            return;

        gestationCycles++;
        if (gestationCycles >= recipe.gestationCycles)
        {
            state = FormingState.Formed;
            Fabricator.Notify_FormingCompleted();
            return;
        }
        formingTicks = recipe.formingTicks;
        state = FormingState.Preparing;
    }

    public override bool PawnAllowedToStartAnew(Pawn p)
    {
        if (State == FormingState.Gathering)
        {

            if (!Fabricator.HasStoredPower(GlittertechExt.powerNeeded))
            {
                JobFailReason.Is("USH_GE_NoPowerStoredShort".Translate(Fabricator.PowerNeededWithStat(this)), null);
                return false;
            }

            var facilities = GlittertechExt.requiredFacilities;
            if (facilities != null)
            {
                CompAffectedByFacilities compAffected = Fabricator.TryGetComp<CompAffectedByFacilities>();
                List<Thing> linkedFacilities = compAffected.LinkedFacilitiesListForReading;

                for (int i = 0; i < facilities.Count; i++)
                    if (linkedFacilities.Find(x => IsRequiredFacility(compAffected, x, facilities[i])) == null)
                    {
                        JobFailReason.Is("USH_GE_NoFacility".Translate(facilities[i].label), null);
                        return false;
                    }
            }
        }

        return base.PawnAllowedToStartAnew(p);
    }

    private bool IsRequiredFacility(CompAffectedByFacilities compAffected, Thing thing, ThingDef requiredThingDef)
    {
        return thing.def == requiredThingDef && compAffected.IsFacilityActive(thing);
    }

    public override void AppendInspectionData(StringBuilder sb)
    {
        if (State != FormingState.Forming && State != FormingState.Preparing)
            return;

        if (State is FormingState.Forming)
            sb.AppendLine("USH_GE_CurrentFormingCycle".Translate() + ": " + ((int)(formingTicks / FormingSpeedMultiplier())).ToStringTicksToPeriod(true, false, true, true, false));

        if (State is FormingState.Preparing)
            sb.AppendLine("USH_GE_WaitingForMaintenance".Translate());

        sb.AppendLine("USH_GE_RemainingFormingCycles".Translate() + ": " + (recipe.gestationCycles - GestationCyclesCompleted).ToString() + " (" + "OfLower".Translate() + " " + recipe.gestationCycles.ToString() + ")");
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref boundPawn, "boundPawn", false);
        Scribe_Values.Look(ref gestationCycles, "gestationCycles", 0, false);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
            GlittertechExt = recipe.GetModExtension<ModExtension_UseGlittertechBill>();
    }
}

