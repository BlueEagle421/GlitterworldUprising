using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace USH_GE;

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

        if (!GE_Mod.Settings.ChangeSkinColor.Value)
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
