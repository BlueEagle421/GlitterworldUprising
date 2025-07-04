using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace USH_GE
{
    public class CompProperties_Telepad : CompProperties_Interactable
    {
        public int fuelConsumption;
        public HediffDef hediffDefToRemove;
        public FleckDef fleckDef;
        public SoundDef soundDef;

        public CompProperties_Telepad() => compClass = typeof(CompTelepad);
    }

    [StaticConstructorOnStartup]
    public class CompTelepad : CompInteractable, ITargetingSource
    {
        public CompProperties_Telepad ModuleProps => (CompProperties_Telepad)props;
        private readonly TargetingParameters TargetingParameters;

        public CompTelepad()
        {
            TargetingParameters = new()
            {
                canTargetSelf = false,
                canTargetBuildings = false,
                validator = t => CanBeTeleported(t.Thing),
            };
        }

        private void Teleport(Pawn toTel)
        {
            if (!CanBeTeleported(toTel))
                return;

            Interact(toTel, true);

            SoundDefOf.Psycast_Skip_Entry.PlayOneShot(parent);
            SkipUtility.SkipTo(toTel, parent.Position, parent.Map);
            SpawnFleckEffect(parent.Position);
        }

        private void TargetPawnToTeleport()
        {
            Find.Targeter.BeginTargeting(TargetingParameters, delegate (LocalTargetInfo t)
            {
                Teleport(t.Pawn);
            });
        }

        private AcceptanceReport CanBeTeleported(Thing t)
        {
            if (t is not Pawn p)
                return false;

            var interactionReport = CanInteract(p);

            if (!interactionReport)
                return interactionReport.Reason;

            if (p.RaceProps.IsFlesh && !p.health.hediffSet.HasHediff(USHDefOf.USH_InstalledTelepadIntegrator))
                return "USH_GE_MissingIntegrator".Translate();

            return true;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (HideInteraction)
                yield break;

            if (!parent.SpawnedOrAnyParentSpawned)
                yield break;

            string defaultLabel = (!string.IsNullOrEmpty(Props.activateLabelString)) ? Props.activateLabelString : ((string)("OrderActivation".Translate() + "..."));
            string defaultDesc = ExtraNamedArg.HasValue ? ((!string.IsNullOrEmpty(Props.activateDescString)) ? ((string)Props.activateDescString.Formatted(parent.Named("THING"), ExtraNamedArg.Value)) : ((string)"OrderActivationDesc".Translate(parent.Named("THING"), ExtraNamedArg.Value))) : ((!string.IsNullOrEmpty(Props.activateDescString)) ? ((string)Props.activateDescString.Formatted(parent.Named("THING"))) : ((string)"OrderActivationDesc".Translate(parent.Named("THING"))));
            Command_Action command_Action = new()
            {
                defaultLabel = defaultLabel,
                defaultDesc = defaultDesc,
                icon = UIIcon,
                groupable = false,
                action = delegate
                {
                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                    TargetPawnToTeleport();
                }
            };

            AcceptanceReport acceptanceReport = CanInteract();

            if (!acceptanceReport.Accepted)
                command_Action.Disable(acceptanceReport.Reason.CapitalizeFirst());

            yield return command_Action;


            if (DebugSettings.ShowDevGizmos && OnCooldown)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Reset cooldown",
                    action = delegate
                    {
                        cooldownTicks = 0;
                        CooldownEnded();
                    }
                };
            }
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (HideInteraction)
                yield break;

            AcceptanceReport acceptanceReport = CanBeTeleported(selPawn);
            FloatMenuOption floatMenuOption = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(ActivateOptionLabel, delegate
            {
                Teleport(selPawn);
            }), selPawn, parent);
            if (!acceptanceReport.Accepted)
            {
                floatMenuOption.Disabled = true;
                floatMenuOption.Label = floatMenuOption.Label + " (" + acceptanceReport.Reason.UncapitalizeFirst() + ")";
            }

            yield return floatMenuOption;

        }

        private void SpawnFleckEffect(IntVec3 position) => FleckMaker.Static(position, parent.Map, FleckDefOf.PsycastSkipFlashEntry);
    }
}
