using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace GliterworldUprising
{
    public class HediffCompProperties_SkinReplacement : HediffCompProperties
    {
        public ColorInt skinColor;
        public List<HediffDef> hediffsConsideredSame = new List<HediffDef>();

        public HediffCompProperties_SkinReplacement() => compClass = typeof(HediffCompSkinReplacement);
    }

    public class HediffCompSkinReplacement : HediffComp
    {
        public HediffCompProperties_SkinReplacement Props => (HediffCompProperties_SkinReplacement)props;

        public override void CompPostMake()
        {
            base.CompPostMake();

            RemoveHediffDuplicates(parent.pawn, Props.hediffsConsideredSame);

            Color toSave = parent.pawn.story.SkinColor;
            SkinSaveComp.Instance.AddPawnSkinColor(parent.pawn, toSave);

            if (!LoadedModManager.GetMod<GUMod>().GetSettings<GUSettings>().shouldChangeColor)
                ChangePawnColor(Props.skinColor.ToColor);
        }

        private void RemoveHediffDuplicates(Pawn pawn, List<HediffDef> duplicates)
        {
            List<Hediff> allHediffs = new List<Hediff>();
            pawn.health.hediffSet.GetHediffs(ref allHediffs);
            foreach (Hediff hediff in allHediffs)
            {
                if (hediff == parent)
                    continue;

                if (duplicates.Contains(hediff.def))
                {
                    GenSpawn.Spawn(hediff.def.spawnThingOnRemoved, pawn.Position, pawn.Map);
                    pawn.health.RemoveHediff(hediff);
                }
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            ChangePawnColor(SkinSaveComp.Instance.GetPawnSkinColor(parent.pawn));
        }

        private void ChangePawnColor(Color color)
        {
            parent.pawn.story.skinColorOverride = color;
            parent.pawn.Drawer.renderer.graphics.SetAllGraphicsDirty();
        }
    }
}