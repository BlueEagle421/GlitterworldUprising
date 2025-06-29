using HarmonyLib;
using RimWorld;
using Verse;

namespace USH_GE
{
    [HarmonyPatch(typeof(GameConditionManager), nameof(GameConditionManager.RegisterCondition))]
    public static class GameConditionManager_RegisterCondition
    {
        private const int INTERCEPT_LETTER_DELAY = 60;

        [HarmonyPrefix]
        public static bool Prefix(
            GameConditionManager __instance,
            GameCondition cond)
        {
            if (cond.def == IncidentDefOf.SolarFlare.gameCondition)
            {
                var component = __instance.ownerMap.GetComponent<MapComponent_SolarFlareBank>();

                if (!component.AllAvailableSolarBanks.NullOrEmpty())
                {
                    InterceptSolarFlare(component);
                    return false;
                }
            }

            return true;
        }

        private static void InterceptSolarFlare(MapComponent_SolarFlareBank component)
        {
            component.AllAvailableSolarBanks?.ForEach(x => x.Notify_SolarFlareIntercepted());

            string label = "USH_GE_SolarFlareInterceptedLabel".Translate();
            string text = "USH_GE_SolarFlareInterceptedText".Translate(component.AllAvailableSolarBanks.Count);
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.PositiveEvent, null, INTERCEPT_LETTER_DELAY);
        }
    }
}