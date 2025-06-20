using Verse;
using HarmonyLib;
using RimWorld;
using System.Reflection;
using System;

namespace GlitterworldUprising
{
    [StaticConstructorOnStartup]
    public static class Start
    {
        static Start()
        {
            Log.Message("GlitterworldUprising loaded successfully!");

            var m = typeof(BillUtility)
                        .GetMethod("MakeNewBill",
                                   BindingFlags.Public | BindingFlags.Static,
                                   null,
                                   new[] { typeof(RecipeDef), typeof(Precept_ThingStyle) },
                                   null);
            Log.Message($"Found MakeNewBill? {(m != null ? "YES" : "NO")}");

            Harmony harmony = new Harmony("GlitterworldUprising");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(BillUtility), nameof(BillUtility.MakeNewBill),
        new Type[] { typeof(RecipeDef), typeof(Precept_ThingStyle) })]
    public static class BillUtility_MakeNewBill_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(RecipeDef recipe, Precept_ThingStyle precept, ref Bill __result)
        {
            Log.Message($"MakeNewBill PREFIX for {recipe.defName} precept={(precept == null ? "null" : precept.GetType().Name)}");
            if (recipe.HasModExtension<ModExtension_UseGlittertechBill>())
            {
                __result = new Bill_Glittertech(recipe, precept);
                return false;   // skip the vanilla MakeNewBill entirely
            }
            return true;        // run vanilla, __result ignored
        }
    }
}