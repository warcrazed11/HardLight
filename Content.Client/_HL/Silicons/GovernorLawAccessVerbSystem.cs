using Content.Shared.HL.Silicons.Components;
using Content.Shared.HL.Silicons;
using Content.Shared.Inventory;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Verbs;
using Content.Shared.Wires;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client.HL.Silicons;

public sealed class GovernorLawAccessVerbSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;

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

        if (args.User == ent.Owner)
            return;

        if (!_inventory.TryGetSlotEntity(ent, "neck", out var neckItem))
            return;

        if (!HasComp<GovernorLawAccessComponent>(neckItem))
            return;

        if (TryComp<WiresPanelComponent>(ent, out var panel) && !panel.Open)
            return;

        // Keep this verb descriptor aligned with server-side GovernorLawAccessSystem so execution matches.
        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString(GovernorLawAccessShared.ManageLawsLocKey),
            Icon = new SpriteSpecifier.Rsi(GovernorLawAccessShared.ManageLawsIconRsiPath, GovernorLawAccessShared.ManageLawsIconState)
        });
    }

}
