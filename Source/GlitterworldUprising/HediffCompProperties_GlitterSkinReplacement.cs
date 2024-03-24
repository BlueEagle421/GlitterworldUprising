using Verse;

namespace GliterworldUprising
{
    public class HediffCompProperties_GlitterSkinReplacement : HediffCompProperties
    {
        public ColorInt skinColor;

        public HediffCompProperties_GlitterSkinReplacement() => this.compClass = typeof(HediffComp_GlitterSkinReplacement);
    }
}
