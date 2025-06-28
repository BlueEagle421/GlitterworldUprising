using HarmonyLib;
using RimWorld;
using Verse;

namespace GlittertechExpansion
{
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