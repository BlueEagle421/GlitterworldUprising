using Verse;
using Verse.Sound;

namespace USH_GE;

public class Building_GlittershipChunk : Building
{
    private Effecter _effecter;
    private Sustainer _sustainer;

    protected override void Tick()
    {
        base.Tick();
        _effecter ??= USH_DefOf.USH_GlittershipChunkPulse.SpawnAttached(this, Map);
        _effecter?.EffectTick(this, this);

        if (_sustainer == null || _sustainer.Ended)
            _sustainer = USH_DefOf.USH_GlittershipAmbience.TrySpawnSustainer(SoundInfo.InMap(this));

        _sustainer.Maintain();
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        USH_DefOf.USH_GlittershipStopped.PlayOneShot(this);
        USH_DefOf.USH_GlittershipChunkDestroyed.Spawn(this, Map);
        _sustainer?.End();
        base.DeSpawn(mode);

        Log.Message("DESTROY!");
    }
}
