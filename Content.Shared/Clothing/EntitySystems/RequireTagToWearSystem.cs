using Content.Shared.Clothing.Components;
using Content.Shared.Inventory.Events;
using Content.Shared.Tag;
using Robust.Shared.GameStates;

namespace Content.Shared.Clothing.EntitySystems;

public sealed class SharedRequireTagToWearSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tagSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RequireTagToWearComponent, IsEquippingAttemptEvent>(OnEquipAttempt);
        SubscribeLocalEvent<RequireTagToWearComponent, BeingEquippedAttemptEvent>(OnEquipAttempt);
    }

    private void OnEquipAttempt(Entity<RequireTagToWearComponent> item, ref IsEquippingAttemptEvent args)
    {
        // Check if the person trying to wear it has the required tag
        if (!_tagSystem.HasTag(args.EquipTarget, item.Comp.Tag))
        {
            args.Cancel();
            args.Reason = item.Comp.DenialMessage;
        }
    }

    private void OnEquipAttempt(Entity<RequireTagToWearComponent> item, ref BeingEquippedAttemptEvent args)
    {
        // Check if the person being equipped has the required tag
        if (!_tagSystem.HasTag(args.EquipTarget, item.Comp.Tag))
        {
            args.Cancel();
            args.Reason = item.Comp.DenialMessage;
        }
    }
}
