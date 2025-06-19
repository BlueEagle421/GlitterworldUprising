using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace GlitterworldUprising
{
    public class SkinSaveComp : WorldComponent
    {
        List<Pawn> _pawns;
        List<Color> _colors = new List<Color>();
        private Dictionary<Pawn, Color> _pawnSkinColors = new Dictionary<Pawn, Color>();

        public static SkinSaveComp Instance { get; private set; }

        public SkinSaveComp(World world) : base(world) => Instance = this;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref _pawnSkinColors, "USH_PawnSkinColors", LookMode.Reference, LookMode.Value, ref _pawns, ref _colors);
        }

        public void AddPawnSkinColor(Pawn pawn, Color color)
        {
            if (_pawnSkinColors.ContainsKey(pawn))
                return;

            _pawnSkinColors.Add(pawn, color);
        }

        public Color GetPawnSkinColor(Pawn pawn) => _pawnSkinColors.TryGetValue(pawn);
    }

    public class GUSettings : ModSettings
    {
        public bool shouldChangeColor;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref shouldChangeColor, "USH_ShouldChangeColor");
        }
    }

    public class GUMod : Mod
    {
        GUSettings settings;

        public GUMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<GUSettings>();
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

        public override string SettingsCategory() => "Glitterworld Uprising";
    }
}