using RimWorld;
using UnityEngine;
using Verse;

namespace USH_GE;

public class GUSettings : ModSettings
{
    public bool ShouldChangeColor;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ShouldChangeColor, "USH_ShouldChangeColor");
    }
}

public class GE_Mod : Mod
{
    public static GE_Settings Settings { get; private set; }
    private static Vector2 scrollPosition = new(0f, 0f);
    private static float totalContentHeight = 1000f;
    private const float ScrollBarWidthMargin = 18f;

    public GE_Mod(ModContentPack content) : base(content)
    {
        Settings = GetSettings<GE_Settings>();
    }
    public override void DoSettingsWindowContents(Rect inRect)
    {
        Rect outerRect = inRect.ContractedBy(10f);

        bool scrollBarVisible = totalContentHeight > outerRect.height;
        var scrollViewTotal = new Rect(0f, 0f, outerRect.width - (scrollBarVisible ? ScrollBarWidthMargin : 0), totalContentHeight);
        Widgets.BeginScrollView(outerRect, ref scrollPosition, scrollViewTotal);

        Listing_Standard listingStandard = new();
        listingStandard.Begin(new Rect(0f, 0f, scrollViewTotal.width, 9999f));


        //FormingSpeedMultiplier
        listingStandard.Label("USH_GE_FormingMultiplierSetting".Translate().Colorize(Color.cyan));
        float formingSliderValue = listingStandard.Slider(Settings.FormingSpeedMultiplier.Value, 0.25f, 3f);
        listingStandard.Label("USH_GE_FormingMultiplierSettingDesc".Translate(formingSliderValue.ToStringPercent()));
        Settings.FormingSpeedMultiplier.Value = formingSliderValue;

        //PositiveMoodMultiplier
        listingStandard.Label("\n");
        listingStandard.Label("USH_GE_PositiveMultiplierSetting".Translate().Colorize(Color.cyan));
        float positiveSliderValue = listingStandard.Slider(Settings.PositiveMoodMultiplier.Value, 0.25f, 1f);
        listingStandard.Label("USH_GE_PositiveMultiplierSettingDesc".Translate(positiveSliderValue.ToStringPercent()));
        Settings.PositiveMoodMultiplier.Value = positiveSliderValue;

        //NegativeMoodMultiplier
        listingStandard.Label("\n");
        listingStandard.Label("USH_GE_NegativeMultiplierSetting".Translate().Colorize(Color.cyan));
        float negativeSliderValue = listingStandard.Slider(Settings.NegativeMoodMultiplier.Value, 0.5f, 2f);
        listingStandard.Label("USH_GE_NegativeMultiplierSettingDesc".Translate(negativeSliderValue.ToStringPercent()));
        Settings.NegativeMoodMultiplier.Value = negativeSliderValue;

        //PylonMoodMultiplier
        listingStandard.Label("\n");
        listingStandard.Label("USH_GE_PylonMultiplierSetting".Translate().Colorize(Color.cyan));
        float pylonSliderValue = listingStandard.Slider(Settings.PylonMoodMultiplier.Value, 0.1f, 1f);
        listingStandard.Label("USH_GE_PylonMultiplierSettingDesc".Translate(pylonSliderValue.ToStringPercent()));
        Settings.PylonMoodMultiplier.Value = pylonSliderValue;

        //ChangeSkinColor
        listingStandard.Label("\n");
        listingStandard.CheckboxLabeled("USH_GE_SkinSetting".Translate().Colorize(Color.cyan), ref Settings.ChangeSkinColor.Value);
        listingStandard.Label("USH_GE_SkinSettingDesc".Translate());

        //Reset button
        listingStandard.Label("\n");
        bool shouldReset = listingStandard.ButtonText("USH_GE_ResetSettings".Translate());
        if (shouldReset) Settings.ResetAll();
        listingStandard.Label("\n");

        //End
        listingStandard.End();
        totalContentHeight = listingStandard.CurHeight + 10f;
        Widgets.EndScrollView();
        base.DoSettingsWindowContents(inRect);
    }

    public override string SettingsCategory() => "Glittertech Expansion";
}

public class GE_Settings : ModSettings
{
    public Setting<float> FormingSpeedMultiplier = new(1f);
    public Setting<float> PositiveMoodMultiplier = new(0.5f);
    public Setting<float> NegativeMoodMultiplier = new(1f);
    public Setting<float> PylonMoodMultiplier = new(0.25f);
    public Setting<bool> ChangeSkinColor = new(true);

    public void ResetAll()
    {
        FormingSpeedMultiplier.ToDefault();
        PositiveMoodMultiplier.ToDefault();
        NegativeMoodMultiplier.ToDefault();
        PylonMoodMultiplier.ToDefault();
        ChangeSkinColor.ToDefault();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        FormingSpeedMultiplier.ExposeData(nameof(FormingSpeedMultiplier));
        PositiveMoodMultiplier.ExposeData(nameof(PositiveMoodMultiplier));
        NegativeMoodMultiplier.ExposeData(nameof(NegativeMoodMultiplier));
        PylonMoodMultiplier.ExposeData(nameof(PylonMoodMultiplier));
        ChangeSkinColor.ExposeData(nameof(ChangeSkinColor));
    }

    public class Setting<T>(T defaultValue)
    {
        public T Value = defaultValue;
        public T DefaultValue { get; private set; } = defaultValue;

        public void ToDefault() => Value = DefaultValue;
        public void ExposeData(string key) => Scribe_Values.Look(ref Value, $"USH_{key}", DefaultValue);
    }
}