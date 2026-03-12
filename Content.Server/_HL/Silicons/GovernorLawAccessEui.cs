using Content.Server.EUI;
using Content.Server.Silicons.Laws;
using Content.Shared.Eui;
using Content.Shared.FixedPoint;
using Content.Shared.HL.Silicons;
using Content.Shared.HL.Silicons.Components;
using Content.Shared.Inventory;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Wires;

namespace Content.Server.HL.Silicons;

public sealed class GovernorLawAccessEui : BaseEui
{
    private readonly SiliconLawSystem _siliconLawSystem;
    private readonly InventorySystem _inventory;
    private readonly EntityManager _entityManager;

    private List<SiliconLaw> _laws = new();
    private EntityUid _target;

    public GovernorLawAccessEui(
        SiliconLawSystem siliconLawSystem,
        InventorySystem inventory,
        EntityManager entityManager)
    {
        _siliconLawSystem = siliconLawSystem;
        _inventory = inventory;
        _entityManager = entityManager;
    }

    public override EuiStateBase GetNewState()
    {
        return new SiliconLawsEuiState(_laws, _entityManager.GetNetEntity(_target));
    }

    public void UpdateLaws(EntityUid target, SiliconLawBoundComponent? lawBoundComponent = null)
    {
        _target = target;

        if (!IsAllowed())
            return;

        var laws = _siliconLawSystem.GetLaws(target, lawBoundComponent);
        _laws = laws.Laws;
        StateDirty();
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        if (msg is not SiliconLawsSaveMessage message)
            return;

        if (!IsAllowed())
            return;

        var target = _entityManager.GetEntity(message.Target);
        if (!_entityManager.TryGetComponent<SiliconLawProviderComponent>(target, out var provider))
            return;

        // Governor collar access cannot alter corruption flags. Preserve the current overrides by order.
        var existingLaws = _siliconLawSystem.GetLaws(target).Laws;
        var existingOverrides = new Dictionary<FixedPoint2, (string? Identifier, string? Print)>(existingLaws.Count);
        foreach (var law in existingLaws)
        {
            existingOverrides[law.Order] = (law.LawIdentifierOverride, law.LawPrintOverride);
        }

        foreach (var law in message.Laws)
        {
            if (existingOverrides.TryGetValue(law.Order, out var existing))
            {
                law.LawIdentifierOverride = existing.Identifier;
                law.LawPrintOverride = existing.Print;
                continue;
            }

            law.LawIdentifierOverride = null;
            law.LawPrintOverride = null;
        }

        _siliconLawSystem.SetLaws(message.Laws, target, provider.LawUploadSound);
    }

    private bool IsAllowed()
    {
        if (_target == default || !_entityManager.EntityExists(_target) || _entityManager.Deleted(_target))
            return false;

        if (Player.AttachedEntity is not { } attached)
            return false;

        if (attached == _target)
            return false;

        if (GovernorLawAccessShared.IsSiliconUser(attached, _entityManager))
            return false;

        if (_entityManager.TryGetComponent<WiresPanelComponent>(_target, out var panel) && !panel.Open)
            return false;

        return _inventory.TryGetSlotEntity(_target, "neck", out var neckItem)
               && _entityManager.HasComponent<GovernorLawAccessComponent>(neckItem);
    }
}
