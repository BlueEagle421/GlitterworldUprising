using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace USH_GE;

[HarmonyPatch(typeof(GameConditionManager), nameof(GameConditionManager.RegisterCondition))]
public static class GameConditionManager_RegisterCondition
{
    private const int INTERCEPT_LETTER_DELAY = 60;

    [HarmonyPrefix]
    public static bool Prefix(
        GameConditionManager __instance,
        GameCondition cond)
    {
        if (cond.def != IncidentDefOf.SolarFlare.gameCondition)
            return true;

        var component = __instance.ownerMap.GetComponent<MapComponent_SolarFlareBank>();

        if (!component.AllAvailableSolarBanks.NullOrEmpty())
        {
            InterceptSolarFlare(component.AllAvailableSolarBanks, component);
            return false;
        }

        return true;
    }

    private static void InterceptSolarFlare(List<CompSolarFlareBank> allBankComps, MapComponent_SolarFlareBank component)
    {
        allBankComps.ForEach(x => x.Notify_SolarFlareIntercepted());

        string label = "USH_GE_SolarFlareInterceptedLabel".Translate();
        string text = "USH_GE_SolarFlareInterceptedText".Translate(allBankComps.Count);

        Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.PositiveEvent, null, INTERCEPT_LETTER_DELAY);
    }
}
