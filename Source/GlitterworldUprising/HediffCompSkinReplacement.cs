using UnityEngine;
using Verse;

namespace GliterworldUprising
{
    public class HediffCompProperties_SkinReplacement : HediffCompProperties
    {
        public ColorInt skinColor;

        public HediffCompProperties_SkinReplacement() => compClass = typeof(HediffCompSkinReplacement);
    }

    public class HediffCompSkinReplacement : HediffComp
    {
        public HediffCompProperties_SkinReplacement Props => (HediffCompProperties_SkinReplacement)props;

        public override void CompPostMake()
        {
            base.CompPostMake();

            Color toSave = parent.pawn.story.SkinColor;
            SkinSaveComp.Instance.AddPawnSkinColor(parent.pawn, toSave);

            if (!LoadedModManager.GetMod<GUMod>().GetSettings<GUSettings>().shouldChangeColor)
                ChangePawnColor(parent.pawn, Props.skinColor.ToColor);
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            ChangePawnColor(parent.pawn, SkinSaveComp.Instance.GetPawnSkinColor(parent.pawn));
        }
        private void ChangePawnColor(Pawn pawn, Color color)
        {
            pawn.story.skinColorOverride = color;
            pawn.Drawer.renderer.SetAllGraphicsDirty();
        }
    }
}