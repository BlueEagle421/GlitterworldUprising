using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;

namespace GliterworldUprising
{
    public class HediffComp_GlitterSkinReplacement : HediffComp
    {

        private Color savedColor;

        public HediffCompProperties_GlitterSkinReplacement Props => (HediffCompProperties_GlitterSkinReplacement)this.props;
       
        public override void CompPostMake()
        {
            base.CompPostMake();

            //Check for double hediffs
            Map map = this.parent.pawn.Map;
            foreach (Hediff hediff in this.parent.pawn.health.hediffSet.GetHediffs<Hediff>())
            {
                if (hediff == this.parent)
                    continue;
                else if (hediff.def.defName.Contains("USH_") && hediff.def.defName.Contains("SkinReplacement"))
                {
                    GenSpawn.Spawn(hediff.def.spawnThingOnRemoved, this.parent.pawn.Position, map);
                    this.parent.pawn.health.RemoveHediff(hediff);
                }
            }

            //Save the orginal color
            savedColor = this.parent.pawn.story.SkinColor;

            //Change the color
            if (!LoadedModManager.GetMod<GUMod>().GetSettings<GUSettings>().shouldChangeColor)
            {
                changeColor(this.Props.skinColor.ToColor);
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            changeColor(savedColor);
        }

        private void changeColor(Color color)
        {
            this.parent.pawn.story.skinColorOverride = color;
            this.parent.pawn.Drawer.renderer.graphics.SetAllGraphicsDirty();
        }
    }
}