using HarmonyLib;
using RimWorld;
using Verse;

namespace USH_GE;

[HarmonyPatch(typeof(StunHandler), "CanBeStunnedByDamage")]
static class StunHandler_CanBeStunnedByDamage_Patch
{
    static bool Prefix(DamageDef def, StunHandler __instance, ref bool __result)
    {
        if (def == USHDefOf.USH_ADP)
        {
            __result = __instance.parent is not Pawn p || !p.RaceProps.IsFlesh;

            return false;
        }

        return true;
    }
}