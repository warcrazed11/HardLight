using Content.Server.EUI;
using Content.Shared.HL.Silicons;
using Content.Shared.HL.Silicons.Components;
using Content.Shared.Inventory;
using Content.Shared.Verbs;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Wires;
using Robust.Server.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.HL.Silicons;

public sealed class GovernorLawAccessSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly EuiManager _eui = default!;
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly Content.Server.Silicons.Laws.SiliconLawSystem _laws = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SiliconLawBoundComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAlternativeVerb);
    }

    private void OnGetAlternativeVerb(Entity<SiliconLawBoundComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (GovernorLawAccessShared.IsSiliconUser(args.User, EntityManager))
            return;

        // The silicon wearing the governor collar cannot edit its own laws via this path.
        if (args.User == ent.Owner)
            return;

        if (!_inventory.TryGetSlotEntity(ent, "neck", out var neckItem))
            return;

        if (!HasComp<GovernorLawAccessComponent>(neckItem))
            return;

        // Match cyborg interaction behavior: if there is a wires panel, it must be open.
        if (TryComp<WiresPanelComponent>(ent, out var panel) && !panel.Open)
            return;

        var user = args.User;
        var target = ent.Owner;
        var lawBound = ent.Comp;

        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString(GovernorLawAccessShared.ManageLawsLocKey),
            Icon = new SpriteSpecifier.Rsi(GovernorLawAccessShared.ManageLawsIconRsiPath, GovernorLawAccessShared.ManageLawsIconState),
            Act = BuildOpenLawVerbAction(user, target, lawBound)
        });
    }

    private Action BuildOpenLawVerbAction(EntityUid user, EntityUid target, SiliconLawBoundComponent lawBound)
    {
        return () =>
        {
            if (!_players.TryGetSessionByEntity(user, out var session) || session is not { } playerSession)
                return;

            var ui = new GovernorLawAccessEui(_laws, _inventory, EntityManager);
            _eui.OpenEui(ui, playerSession);
            ui.UpdateLaws(target, lawBound);
        };
    }
}
