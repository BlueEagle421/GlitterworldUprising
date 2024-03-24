//inspired by https://gist.github.com/erdelf/84dce0c0a1f00b5836a9d729f845298a
using System.Collections.Generic;
using Verse;
using UnityEngine;

namespace GliterworldUprising
{
    public class GUSettings : ModSettings
    {
        public bool shouldChangeColor;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref shouldChangeColor, "shouldChangeColor");
            base.ExposeData();
        }
    }

    public class GUMod : Mod
    {
        GUSettings settings;

        public GUMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<GUSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("USH_GU_SkinSettingLabel".Translate(), ref settings.shouldChangeColor, "USH_GU_SkinSettingTooltip".Translate());
            listingStandard.Label("USH_GU_SkinSettingExplanation".Translate());
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "USH_GU_ModName".Translate();
        }
    }
}