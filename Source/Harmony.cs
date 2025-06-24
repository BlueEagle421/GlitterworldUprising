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
}