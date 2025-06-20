using RimWorld;
using Verse;

namespace GlitterworldUprising
{
    [DefOf]
    public static class USH_DefOf
    {
        static USH_DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(USH_DefOf));
        }

        public static ThingDef USH_GlitterSlime;
    }

}

