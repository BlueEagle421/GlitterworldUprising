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
            Building_Biocoder biocoder = (Building_Biocoder)actor.CurJob.targetA.Thing;

            bool flag = actor.DeSpawnOrDeselect(DestroyMode.Vanish);
            if (biocoder.TryAcceptThing(actor, true) && flag)
                Find.Selector.Select(actor, false, false);
        };
        enter.defaultCompleteMode = ToilCompleteMode.Instant;

        yield return enter;
    }
}