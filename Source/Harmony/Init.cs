using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace GlitterworldUprising
{
    [StaticConstructorOnStartup]
    public static class Start
    {
        static Start()
        {
            Harmony harmony = new Harmony("GlitterworldUprising");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Log.Message("The almighty power of Harmony has been initialized by the humble mod creator BlueEagle421".Colorize(Color.cyan));
        }
    }
}