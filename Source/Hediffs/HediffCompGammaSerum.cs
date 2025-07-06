using Verse;

namespace USH_GE
{
    public class HediffCompProperties_GammaSerum : HediffCompProperties
    {
        public HediffCompProperties_GammaSerum() => compClass = typeof(HediffCompGammaSerum);
    }

    public class HediffCompGammaSerum : HediffComp
    {
        public HediffCompProperties_GammaSerum Props => (HediffCompProperties_GammaSerum)props;

        public override void CompPostMake()
        {
            base.CompPostMake();
            RemoveWillAndCertainty();
        }

        private void RemoveWillAndCertainty()
        {
            Pawn target = parent.pawn;

            target.guest.Recruitable = true;

            target.guest.resistance = 0f;
            target.guest.will = 0f;

            if (ModsConfig.IdeologyActive)
                target.ideo.OffsetCertainty(0f - target.ideo.Certainty);
        }
    }
}