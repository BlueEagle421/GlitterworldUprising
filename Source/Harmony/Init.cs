using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace USH_GE
{
    [StaticConstructorOnStartup]
    public static class Start
    {
        static Start()
        {
            Harmony harmony = new("GlittertechExpansion");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Log.Message("The almighty power of Harmony has been initialized by the humble mod creator BlueEagle421".Colorize(Color.cyan));
        }
    }
}