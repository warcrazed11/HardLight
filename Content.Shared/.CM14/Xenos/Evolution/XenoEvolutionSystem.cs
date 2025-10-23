using Content.Shared.Actions;
using Content.Shared.Mind;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Shared.CM14.Xenos.Evolution;

public sealed class XenoEvolutionSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenoEvolveActionComponent, MapInitEvent>(OnXenoEvolveActionMapInit);
    }

    private void OnXenoEvolveActionMapInit(Entity<XenoEvolveActionComponent> ent, ref MapInitEvent args)
    {
        _action.SetCooldown(ent, _timing.CurTime, _timing.CurTime + ent.Comp.Cooldown);
    }

}
