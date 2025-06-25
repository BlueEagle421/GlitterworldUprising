using Verse;
using HarmonyLib;
using RimWorld;
using System.Reflection;
using System;
using System.Collections.Generic;

namespace GlitterworldUprising
{
    [StaticConstructorOnStartup]
    public static class Start
    {
        static Start()
        {
            Harmony harmony = new Harmony("GlitterworldUprising");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(BillUtility), nameof(BillUtility.MakeNewBill), new Type[] { typeof(RecipeDef), typeof(Precept_ThingStyle) })]
    public static class BillUtility_MakeNewBill_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(RecipeDef recipe, Precept_ThingStyle precept, ref Bill __result)
        {
            if (recipe.HasModExtension<ModExtension_UseGlittertechBill>())
            {
                __result = new Bill_Glittertech(recipe, precept);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Thing), nameof(Thing.PostApplyDamage))]
    public static class Patch_Thing_PostApplyDamage
    {
        public static void Postfix(Thing __instance)
        {
            if (__instance != null && __instance is Building b && b.Faction == Faction.OfPlayer)
                b.Map.GetComponent<MapComponent_RepairManager>().NotifyDamaged(b);
        }
    }

    [HarmonyPatch(typeof(BillUtility))]
    [HarmonyPatch("LayoutTooltip")]
    [HarmonyPatch(new[] {
        typeof(RecipeDef),
        typeof(BodyPartRecord),
        typeof(Pawn),
        typeof(bool)
    })]
    static class Patch_BillUtility_LayoutTooltip
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
                if (_cachedPowerTexArg == null)
                    _cachedPowerTexArg = Gen.YieldSingle(USHDefOf.USH_PowerStored.ToTextureAndColor());

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
                if (_cachedTooltip == null)
                    _cachedTooltip = AccessTools.Field(typeof(BillUtility), "recipeTooltipLayout").GetValue(null) as RecipeTooltipLayout;

                return _cachedTooltip;
            }
        }

        static void Postfix(RecipeDef recipe, BodyPartRecord part, Pawn pawn, bool draw)
        {
            if (!TryGetRecipeExt(recipe, out var ext))
                return;

            Tooltip.Newline();
            Tooltip.Label("Required power stored: ".AsTipTitle(), draw);
            Tooltip.Newline();

            var args = new object[] { PowerTexArg, draw, new float?(ext.powerNeeded), null };
            IngredientsMethod.Invoke(null, args);

            Tooltip.Expand(6f, 6f);
        }

        private static TextureAndColor ToTextureAndColor(this ThingDef td)
        {
            return new TextureAndColor(Widgets.GetIconFor(td), td.uiIconColor);
        }


    }
}