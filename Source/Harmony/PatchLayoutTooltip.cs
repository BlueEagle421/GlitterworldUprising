using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace USH_GE;

[HarmonyPatch(typeof(BillUtility))]
[HarmonyPatch("LayoutTooltip", MethodType.Normal)]
public static class Patch_BillUtility_LayoutTooltip
{
    private static ModExtension_UseGlittertechBill _cachedExt;
    private static RecipeDef _lastRecipe;

    private static bool TryGetRecipeExt(RecipeDef recipe, out ModExtension_UseGlittertechBill ext)
    {
        if (_cachedExt != null && _lastRecipe == recipe)
        {
            ext = _cachedExt;
            return true;
        }

        _cachedExt = null;
        _lastRecipe = null;

        if (!recipe.HasModExtension<ModExtension_UseGlittertechBill>())
        {
            ext = null;
            return false;
        }

        ext = recipe.GetModExtension<ModExtension_UseGlittertechBill>();
        _cachedExt = ext;
        _lastRecipe = recipe;
        return true;
    }
    private static IEnumerable<TextureAndColor> _cachedPowerTexArg;
    private static IEnumerable<TextureAndColor> PowerTexArg
    {
        get
        {
            _cachedPowerTexArg ??= Gen.YieldSingle(USHDefOf.USH_PowerStored.ToTextureAndColor());

            return _cachedPowerTexArg;
        }
    }

    private static MethodInfo _cachedIngredientsMethod;
    private static MethodInfo IngredientsMethod
    {
        get
        {
            if (_cachedIngredientsMethod == null)
                _cachedIngredientsMethod = AccessTools.Method(typeof(BillUtility), "DisplayIngredientIconRow");

            return _cachedIngredientsMethod;
        }
    }

    private static RecipeTooltipLayout _cachedTooltip;
    private static RecipeTooltipLayout Tooltip
    {
        get
        {
            _cachedTooltip ??= AccessTools.Field(typeof(BillUtility), "recipeTooltipLayout").GetValue(null) as RecipeTooltipLayout;

            return _cachedTooltip;
        }
    }

    private static TextureAndColor ToTextureAndColor(this ThingDef td)
    {
        return new TextureAndColor(Widgets.GetIconFor(td), td.uiIconColor);
    }

    private static MethodInfo GetTargetMethod()
    {
        return AccessTools.Method(
            typeof(GenList),
            nameof(GenList.NullOrEmpty),
            null,
            generics: [typeof(IngredientCount)]
        );
    }

    private static MethodInfo GetMethodToInject()
    {
        return AccessTools.Method(
            typeof(Patch_BillUtility_LayoutTooltip),
            nameof(AfterReset));
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);

        var targetMethod = GetTargetMethod();
        var injectMethod = GetMethodToInject();

        for (int i = 0; i < codes.Count; i++)
        {
            if (!codes[i].Calls(targetMethod))
                continue;

            codes.InsertRange(i + 1, [
                new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Ldarg_2),
                    new CodeInstruction(OpCodes.Ldarg_3),
                    new CodeInstruction(OpCodes.Call, injectMethod),
                ]);

            i += 5;
        }

        return codes;
    }

    private static void AfterReset(RecipeDef recipe, BodyPartRecord part, Pawn pawn, bool draw)
    {
        if (!TryGetRecipeExt(recipe, out var ext))
            return;

        Tooltip.Newline();
        Tooltip.Label("USH_GE_PowerNeed".Translate().AsTipTitle(), draw);
        Tooltip.Newline();

        var args = new object[] { PowerTexArg, draw, new float?(ext.powerNeeded), null };
        IngredientsMethod.Invoke(null, args);
    }
}