using Verse;

namespace USH_GE;

public class Building_GlittershipChunk : Building
{
    private Effecter _effecter;

    protected override void Tick()
    {
        base.Tick();
        _effecter ??= USH_DefOf.USH_GlittershipChunkPulse.SpawnAttached(this, Map);
        _effecter?.EffectTick(this, this);
    }
}
