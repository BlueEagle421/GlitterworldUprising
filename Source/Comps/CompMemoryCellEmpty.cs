using RimWorld;
using Verse;
using Verse.AI;

namespace USH_GE
{
    public class CompProperties_TargetEffectMemoryCellEmpty : CompProperties
    {
        public JobDef jobDef;

        public CompProperties_TargetEffectMemoryCellEmpty()
        {
            compClass = typeof(CompTargetEffect_MemoryCellEmpty);
        }
    }

    public class CompTargetEffect_MemoryCellEmpty : CompTargetEffect
    {
        public CompProperties_TargetEffectMemoryCellEmpty CellProps => (CompProperties_TargetEffectMemoryCellEmpty)props;
        public override void DoEffectOn(Pawn user, Thing target)
        {
            if (!user.IsColonistPlayerControlled)
                return;

            if (!user.CanReserveAndReach(target, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false))
                return;

            Job job = JobMaker.MakeJob(CellProps.jobDef, target, parent);
            job.count = 1;
            user.jobs.TryTakeOrderedJob(job);
        }
    }
}
