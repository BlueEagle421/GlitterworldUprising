using HarmonyLib;
using RimWorld;
using Verse;

namespace USH_GE
{
    [HarmonyPatch(typeof(Thing), nameof(Thing.PostApplyDamage))]
    public static class Patch_Thing_PostApplyDamage
    {
        public static void Postfix(Thing __instance)
        {
            if (__instance == null)
                return;

            if (__instance is not Building b)
                return;

            if (b.Faction == null)
                return;

            if (b.Faction != Faction.OfPlayer)
                return;

            b.Map?.GetComponent<MapComponent_RepairManager>().NotifyDamaged(b);
        }
    }
}