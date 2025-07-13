using LudeonTK;

namespace USH_GE;

public static class USH_DebugActions
{
    [DebugAction("Ushankas", "Spawn glittertech chunk")]
    public static void DebugSpawnChunk() => WorldComponent_GlittershipChunk.Instance.EventSpawnChunk(out _);
}