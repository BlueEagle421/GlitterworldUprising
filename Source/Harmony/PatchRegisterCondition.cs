using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace USH_GE;

[HarmonyPatch(typeof(GameConditionManager), nameof(GameConditionManager.RegisterCondition))]
public static class GameConditionManager_RegisterCondition
{
    private const int INTERCEPT_LETTER_DELAY = 60;

    [HarmonyPostfix]
    public static void Postfix(
        GameConditionManager __instance,
        GameCondition cond)
    {
        if (cond == null)
            return;

        if (cond.def == null)
            return;

        if (__instance == null)
            return;

        if (cond.def != IncidentDefOf.SolarFlare.gameCondition)
            return;

        var component = __instance.ownerMap.GetComponent<MapComponent_SolarFlareBank>();

        if (component.AllAvailableSolarBanks.NullOrEmpty())
            return;

        InterceptSolarFlare(component.AllAvailableSolarBanks, cond);
    }

    private static void InterceptSolarFlare(List<CompSolarFlareBank> allBankComps, GameCondition condition)
    {
        allBankComps.ForEach(x => x.Notify_SolarFlareIntercepted());

        string label = "USH_GE_SolarFlareInterceptedLabel".Translate();
        string text = "USH_GE_SolarFlareInterceptedText".Translate(allBankComps.Count);

        condition.End();
        Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.PositiveEvent, null, INTERCEPT_LETTER_DELAY);
    }
}
