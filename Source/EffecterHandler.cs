using UnityEngine;
using Verse;

public class EffecterHandler
{
    private Thing _source;
    private Effecter _effecter;
    private int _ticksLeft;
    private EffecterDef _def;

    public EffecterHandler(Thing source, EffecterDef effecterDef)
    {
        _source = source;
        _def = effecterDef;
    }

    public void StartMaintaining() => StartMaintaining(_def.maintainTicks);

    public void StartMaintaining(int forTicks, float yOffset = 0f)
    {
        if (_effecter == null && _source.Spawned && _source.Map != null)
        {
            _effecter = _def.Spawn(_source, _source.Map, new Vector3(0, 0, yOffset));

            _effecter.Trigger(
                new TargetInfo(_source),
                new TargetInfo(_source)
            );
        }

        _ticksLeft = forTicks;
    }

    public void Tick()
    {
        if (_effecter != null && _ticksLeft > 0)
        {
            _effecter.EffectTick(
                new TargetInfo(_source),
                new TargetInfo(_source)
            );

            _ticksLeft--;
            return;
        }

        StopMaintaining();
    }

    public void StopMaintaining()
    {
        if (_effecter == null)
            return;

        _effecter?.Cleanup();
        _effecter = null;
    }
}
