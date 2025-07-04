using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace USH_GE;


public class JobDriver_EnterBiocoder : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed) => pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed, false);
    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);

        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell, false);

        Toil toil = Toils_General.Wait(500, TargetIndex.None);
        toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
        toil.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);

        yield return toil;

        Toil enter = ToilMaker.MakeToil("MakeNewToils");
        enter.initAction = delegate ()
        {
            Pawn actor = enter.actor;
            Building_Biocoder pod = (Building_Biocoder)actor.CurJob.targetA.Thing;
            void action()
            {
                if (pod.TryAcceptThing(actor, true) && actor.DeSpawnOrDeselect(DestroyMode.Vanish))
                    Find.Selector.Select(actor, false, false);
            }

            if (pod.def.building.isPlayerEjectable)
            {
                action();
                return;
            }
            if (Map.mapPawns.FreeColonistsSpawnedOrInPlayerEjectablePodsCount <= 1)
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("CasketWarning".Translate(actor.Named("PAWN")).AdjustedFor(actor, "PAWN", true), action, false, null, WindowLayer.Dialog));
                return;
            }
            action();
        };
        enter.defaultCompleteMode = ToilCompleteMode.Instant;

        yield return enter;
    }
}

public class JobDriver_CarryToBiocoder : JobDriver
{
    protected Pawn Takee => (Pawn)job.GetTarget(TargetIndex.A).Thing;
    protected Building_Biocoder DropPod => (Building_Biocoder)job.GetTarget(TargetIndex.B).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        if (pawn.Reserve(Takee, job, 1, -1, null, errorOnFailed))
        {
            return pawn.Reserve(DropPod, job, 1, -1, null, errorOnFailed);
        }
        return false;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDestroyedOrNull(TargetIndex.A);
        this.FailOnDestroyedOrNull(TargetIndex.B);
        this.FailOnAggroMentalState(TargetIndex.A);

        this.FailOn(() => !DropPod.Accepts(Takee));

        Toil goToTakee = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnDespawnedNullOrForbidden(TargetIndex.B)
            .FailOn(() => DropPod.GetDirectlyHeldThings().Count > 0)
            .FailOn(() => !pawn.CanReach(Takee, PathEndMode.OnCell, Danger.Deadly))
            .FailOnSomeonePhysicallyInteracting(TargetIndex.A);

        Toil startCarryingTakee = Toils_Haul.StartCarryThing(TargetIndex.A);

        Toil goToThing = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell);

        yield return Toils_Jump.JumpIf(goToThing, () => pawn.IsCarryingPawn(Takee));

        yield return goToTakee;

        yield return startCarryingTakee;

        yield return goToThing;

        Toil toil = Toils_General.Wait(500, TargetIndex.B);
        toil.FailOnCannotTouch(TargetIndex.B, PathEndMode.InteractionCell);
        toil.WithProgressBarToilDelay(TargetIndex.B);

        yield return toil;

        Toil toil2 = ToilMaker.MakeToil("MakeNewToils");
        toil2.initAction = delegate
        {
            DropPod.TryAcceptThing(Takee);
        };
        toil2.defaultCompleteMode = ToilCompleteMode.Instant;

        yield return toil2;
    }
}