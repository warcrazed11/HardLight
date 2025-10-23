using Content.Shared.CM14.Xenos;
using Content.Shared.CM14.Xenos.Rest;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Timing;

namespace Content.Server.CM14.Xenos.Rest;

public sealed class XenoRestSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var curTime = _timing.CurTime;

        var query = EntityQueryEnumerator<XenoComponent, XenoRestingComponent, DamageableComponent, MobStateComponent>();
        while (query.MoveNext(out var uid, out var xeno, out var resting, out var damage, out var mob))
        {
            // throttle to once per second
            if (resting.NextTick + TimeSpan.FromSeconds(1) > curTime)
                continue;

            resting.NextTick = curTime;

            if (_mobState.IsDead(uid, mob))
                continue;

            var regen = xeno.RestHealing;
            if (_mobState.IsCritical(uid, mob))
                regen *= xeno.RestHealingCritMultiplier;

            if (regen == null)
                continue;

            _damageable.TryChangeDamage(uid, regen, true, false, damage);
        }
    }
}
