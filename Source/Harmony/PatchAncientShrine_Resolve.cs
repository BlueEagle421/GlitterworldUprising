using HarmonyLib;
using RimWorld.BaseGen;
using USH_GE;
using Verse;

[HarmonyPatch(typeof(SymbolResolver_AncientShrine), "Resolve")]
public static class Patch_SymbolResolver_AncientShrine_Resolve
{
    private static bool _crateSpawned;

    public static void Postfix(ResolveParams rp)
    {
        if (_crateSpawned)
            return;

        if (rp.rect == null)
            return;

        ResolveParams crateParams = rp;
        crateParams.singleThingDef = USH_DefOf.USH_Glittercrate;
        crateParams.thingRot = Rot4.North;

        BaseGen.symbolStack.Push("thing", crateParams);

        _crateSpawned = true;
    }
}