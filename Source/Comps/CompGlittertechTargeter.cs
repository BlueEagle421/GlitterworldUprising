using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace USH_GE;

public class CompGlittertechTargeter : CompInteractableRocketswarmLauncher
{
    private Building_Biocoder ParentGun => (Building_Biocoder)parent;

    public TargeterType TargeterType;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);

        SetTargeterType(TargeterType);
    }

    public void SetTargeterType(TargeterType targeterType)
    {
        TargeterType = targeterType;

        ParentGun.VerbIndex = (int)targeterType;
    }

    public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
    {
        AcceptanceReport baseReport = base.CanInteract(activateBy, checkOptionalItems);

        if (!baseReport)
            return baseReport;

        if (!ParentGun.HasAnyContents)
            return "USH_GE_EmptyBiocoder".Translate();

        return true;
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (var gizmo in base.CompGetGizmosExtra())
            yield return gizmo;

        yield return new Command_SetTargetType(this);
    }

    public override void PostExposeData()
    {
        base.PostExposeData();

        Scribe_Values.Look(ref TargeterType, "TargeterType");
    }
}


[StaticConstructorOnStartup]
public class Command_SetTargetType : Command
{
    private readonly CompGlittertechTargeter _targeter;

    private static readonly Texture2D BombardmentTex = ContentFinder<Texture2D>.Get("Things/Item/Equipment/WeaponSpecial/OrbitalTargeterBombardment/OrbitalTargeterBombardment");
    private static readonly Texture2D PowerBeamTex = ContentFinder<Texture2D>.Get("Things/Item/Equipment/WeaponSpecial/OrbitalTargeterPowerBeam/OrbitalTargeterPowerBeam");

    public Command_SetTargetType(CompGlittertechTargeter targeter)
    {
        _targeter = targeter;
        switch (targeter.TargeterType)
        {
            case TargeterType.Bombardment:
                defaultLabel = "USH_GE_CommandSetForBombardmentLabel".Translate();
                icon = BombardmentTex;
                break;
            case TargeterType.PowerBeam:
                defaultLabel = "USH_GE_CommandSetForPowerBeamLabel".Translate();
                icon = PowerBeamTex;
                break;
            default:
                Log.Error($"Unknown target type selected for {nameof(CompGlittertechTargeter)}: {targeter.TargeterType}");
                break;
        }
        defaultDesc = "USH_GE_CommandSetForTargeterTypeDesc".Translate();
    }

    public override void ProcessInput(Event ev)
    {
        base.ProcessInput(ev);
        List<FloatMenuOption> list =
        [
            new FloatMenuOption("USH_GE_CommandSetForBombardmentLabel".Translate(), delegate
            {
                _targeter.SetTargeterType(TargeterType.Bombardment);
            }, BombardmentTex, Color.white),
            new FloatMenuOption("USH_GE_CommandSetForPowerBeamLabel".Translate(), delegate
            {
                _targeter.SetTargeterType(TargeterType.PowerBeam);
            }, PowerBeamTex, Color.white),
        ];

        Find.WindowStack.Add(new FloatMenu(list));
    }
}

public enum TargeterType
{
    Bombardment,
    PowerBeam
}