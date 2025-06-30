using HarmonyLib;
using RimWorld;
using Verse;

namespace USH_GE;

[HarmonyPatch(typeof(StunHandler), "CanAdaptToDamage")]
static class StunHandler_CanAdaptToDamage_Patch
{
    static bool Prefix(DamageDef def, ref bool __result)
    {
        if (def != null && def == USHDefOf.USH_ADP)
        {
            __result = false;
            return false;
        }

        return true;
    }
}