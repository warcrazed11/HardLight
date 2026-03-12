using Content.Shared.Movement.Components;
using Content.Shared.Silicons.Laws.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Utility;

namespace Content.Shared.HL.Silicons;

public static class GovernorLawAccessShared
{
    public const string ManageLawsLocKey = "silicon-law-ui-verb";
    public static readonly ResPath ManageLawsIconRsiPath = new("/Textures/Interface/Actions/actions_borg.rsi");
    public const string ManageLawsIconState = "state-laws";

    public static bool IsSiliconUser(EntityUid user, IEntityManager entMan)
    {
        if (entMan.HasComponent<SiliconLawBoundComponent>(user))
            return true;

        return entMan.TryGetComponent<MovementRelayTargetComponent>(user, out var relay)
               && entMan.HasComponent<SiliconLawBoundComponent>(relay.Source);
    }
}
