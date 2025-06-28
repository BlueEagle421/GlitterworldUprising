using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace GlitterworldUprising;

public class CompGlittertechTargeter : CompInteractableRocketswarmLauncher
{
    private Building_GlittertechTargeterGun ParentGun => (Building_GlittertechTargeterGun)parent;

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

public class Building_GlittertechTargeterGun : Building_TurretRocket
{
    public int VerbIndex { get; set; }
    public override Verb AttackVerb => GunCompEq.AllVerbs[VerbIndex];

    public override Material TurretTopMaterial => def.building.turretTopMat;

    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {

    }
}

public class Command_SetTargetType : Command
{
    private CompGlittertechTargeter targeter;

    private static readonly Texture2D BombardmentTex = ContentFinder<Texture2D>.Get("Things/Item/Equipment/WeaponSpecial/OrbitalTargeterBombardment/OrbitalTargeterBombardment");
    private static readonly Texture2D PowerBeamTex = ContentFinder<Texture2D>.Get("Things/Item/Equipment/WeaponSpecial/OrbitalTargeterPowerBeam/OrbitalTargeterPowerBeam");

    public Command_SetTargetType(CompGlittertechTargeter targeter)
    {
        this.targeter = targeter;
        switch (targeter.TargeterType)
        {
            case TargeterType.Bombardment:
                defaultLabel = "USH_CommandSetForBombardmentLabel".Translate();
                icon = BombardmentTex;
                break;
            case TargeterType.PowerBeam:
                defaultLabel = "USH_CommandSetForPowerBeamLabel".Translate();
                icon = PowerBeamTex;
                break;
            default:
                Log.Error($"Unknown target type selected for {nameof(CompGlittertechTargeter)}: {targeter.TargeterType}");
                break;
        }
        defaultDesc = "USH_CommandSetForTargeterTypeDesc".Translate();
    }

    public override void ProcessInput(Event ev)
    {
        base.ProcessInput(ev);
        List<FloatMenuOption> list =
        [
            new FloatMenuOption("USH_CommandSetForBombardmentLabel".Translate(), delegate
            {
                targeter.SetTargeterType(TargeterType.Bombardment);
            }, BombardmentTex, Color.white),
            new FloatMenuOption("USH_CommandSetForPowerBeamLabel".Translate(), delegate
            {
                targeter.SetTargeterType(TargeterType.PowerBeam);
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