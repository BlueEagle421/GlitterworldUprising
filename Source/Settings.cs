using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace USH_GE
{
    public class SkinSaveComp : WorldComponent
    {
        List<Pawn> _pawns;
        List<Color> _colors = [];
        private Dictionary<Pawn, Color> _pawnSkinColors = [];

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
        public bool ShouldChangeColor;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ShouldChangeColor, "USH_ShouldChangeColor");
        }
    }

    public class GUMod : Mod
    {
        private readonly GUSettings _settings;

        public GUMod(ModContentPack content) : base(content)
        {
            _settings = GetSettings<GUSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("USH_GE_SkinSettingLabel".Translate(), ref _settings.ShouldChangeColor, "USH_GE_SkinSettingTooltip".Translate());
            listingStandard.Label("USH_GE_SkinSettingExplanation".Translate());
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory() => "Glittertech Expansion";
    }
}